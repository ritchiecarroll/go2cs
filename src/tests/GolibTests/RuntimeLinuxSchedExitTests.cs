using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// THREE LINUX RUNTIME PRIMITIVES WHOSE ONLY GO BODIES ARE ASSEMBLY: sched_getaffinity, osyield and exit
// (runtime/linux/stubs2_impl.cs). Each was a bodyless partial that the stub generator bodied with a
// throw, on linux/amd64 and linux/arm64 alike:
//   - sched_getaffinity: the public SetDefaultGOMAXPROCS reaches it through getCPUCount with no branch
//     on the way.
//   - osyield: the spin loops call it (casgstatus, suspendG and others), so the throw appeared only
//     under contention.
//   - exit: sysAllocOS calls exit(2) when mmap fails with EACCES or EAGAIN.
//
// Arms:
//   1. sched_getaffinity returns the kernel's BYTE COUNT, which getCPUCount slices its buffer by.
//      libc's wrapper of the same name returns 0, and a body over it would count no CPU at all.
//   2. sched_getaffinity returns the NEGATIVE errno on failure, as Go's assembly does (libc's -1 is
//      not it).
//   3. getCPUCount answers the number of CPUs the kernel allows the calling thread, read from procfs
//      and sysfs: an instrument that does not go through the body.
//   4. SetDefaultGOMAXPROCS, the public route to the stub, returns, and sets Go's default. The default
//      is the allowed CPUs lowered to the cgroup's CPU limit, read from cgroupfs.
//   5. osyield returns.
//   6. exit ends the process with its status, in a child: this assembly run again by `dotnet test`
//      (GodebugUpdateHookTests' pattern), filtered to ChildCallsExit and told to run by
//      GO2CS_RUNTIME_EXIT_CHILD_RECORD. The child records "before", calls exit, and records what
//      happened if the call came back. The status is the test host's, which `dotnet test` does not
//      pass on, so it is read from the runner's diagnostic log.
//
// Not constructible here: exit's own caller. Forcing mmap to fail with EACCES or EAGAIN inside a
// .NET host needs a locked-memory limit that fails the host's own allocations first.
//
// The number sched_getaffinity takes is pinned elsewhere: each arch's row in
// LinuxRuntimeSyscallNumberTests, and the row against the converted flavour's own constant in
// LinuxAmd64RuntimeSyscallFlavourTests and LinuxArm64RuntimeSyscallFlavourTests.
//
// Linux flavour only: the probes live in runtime/linux (GolibTests.csproj excludes this file under
// every other $(GoTargetOS)).
[TestClass]
public class RuntimeLinuxSchedExitTests
{
    private const string RecordVariable = "GO2CS_RUNTIME_EXIT_CHILD_RECORD";
    private const int ExitStatus = 7;
    private const string ExitStatusMarker = "exited with exitcode:";

    // A kernel CPU list ("0-3,8") as the set it names.
    private static HashSet<int> CpuList(string text)
    {
        HashSet<int> cpus = [];

        foreach (string part in text.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] ends = part.Split('-');
            int first = int.Parse(ends[0]);
            int last = int.Parse(ends[^1]);

            for (int cpu = first; cpu <= last; cpu++)
                cpus.Add(cpu);
        }

        return cpus;
    }

    // The CPUs the kernel allows the calling thread: its affinity mask, less any CPU that is not online
    // (sched_getaffinity answers the mask the same way, and a hot-pluggable machine allows CPUs it does
    // not have).
    private static int KernelAllowedCpus()
    {
        const string Key = "Cpus_allowed_list:";
        string? line = File.ReadLines("/proc/thread-self/status").FirstOrDefault(l => l.StartsWith(Key, StringComparison.Ordinal));

        Assert.IsNotNull(line, "/proc/thread-self/status has no Cpus_allowed_list line");

        HashSet<int> allowed = CpuList(line![Key.Length..]);
        allowed.IntersectWith(CpuList(File.ReadAllText("/sys/devices/system/cpu/online")));

        return allowed.Count;
    }

    // The smallest CPU limit (quota / period) on the process's cgroup and its ancestors up to the controller's
    // mount, or null when none limits it: read from cgroupfs directly, not through the converted cgroup package.
    // A cgroup v1 cpu controller is taken over a v2 hierarchy when both are mounted, as Go's does.
    private static double? CgroupCpuLimit()
    {
        string? v1Mount = null, v1Root = null, v2Mount = null, v2Root = null;

        foreach (string line in File.ReadLines("/proc/self/mountinfo"))
        {
            // "<id> <parent> <dev> <root> <mountpoint> <options> [optional fields] - <fstype> <source> <super options>"
            string[] halves = line.Split(" - ", 2);
            string[] mount = halves[0].Split(' ');
            string[] fs = halves[1].Split(' ');

            if (fs[0] == "cgroup" && fs.Length > 2 && fs[2].Split(',').Contains("cpu"))
                (v1Mount, v1Root) = (mount[4], mount[3]);
            else if (fs[0] == "cgroup2")
                (v2Mount, v2Root) = (mount[4], mount[3]);
        }

        string? path = null, mountPoint = null, root = null;
        bool v1 = false;

        foreach (string line in File.ReadLines("/proc/self/cgroup"))
        {
            // "<id>:<controllers>:<path>"; v2's line is "0::<path>".
            string[] parts = line.Split(':', 3);

            if (v1Mount is not null && parts[1].Split(',').Contains("cpu"))
                (path, mountPoint, root, v1) = (parts[2], v1Mount, v1Root, true);
            else if (v1Mount is null && v2Mount is not null && parts[0] == "0" && parts[1] == "")
                (path, mountPoint, root) = (parts[2], v2Mount, v2Root);
        }

        if (path is null || mountPoint is null || root is null)
            return null;

        string relative = root == "/" ? path : path.StartsWith(root, StringComparison.Ordinal) ? path[root.Length..] : path;
        string directory = Path.Join(mountPoint, relative.TrimStart('/'));
        double? limit = null;

        while (directory.StartsWith(mountPoint, StringComparison.Ordinal))
        {
            double? here = null;

            if (v1)
            {
                string quota = Path.Join(directory, "cpu.cfs_quota_us"), period = Path.Join(directory, "cpu.cfs_period_us");

                if (File.Exists(quota) && File.Exists(period))
                {
                    long q = long.Parse(File.ReadAllText(quota).Trim()), p = long.Parse(File.ReadAllText(period).Trim());

                    if (q > 0 && p > 0)
                        here = (double)q / p;
                }
            }
            else
            {
                string max = Path.Join(directory, "cpu.max");

                if (File.Exists(max))
                {
                    string[] fields = File.ReadAllText(max).Trim().Split(' ');

                    if (fields[0] != "max")
                        here = double.Parse(fields[0]) / double.Parse(fields[1]);
                }
            }

            if (here is double h && (limit is null || h < limit))
                limit = h;

            if (directory == mountPoint)
                break;

            directory = Path.GetDirectoryName(directory) ?? mountPoint;
        }

        return limit;
    }

    [TestMethod]
    public void SchedGetaffinityReturnsTheKernelsByteCount()
    {
        var (result, bits, failure) = runtime_package.GoSchedGetaffinityProbe();

        Assert.IsNull(failure, failure);
        Console.WriteLine($"READING sched_getaffinity(0, 8192, buf) = {result}, bits set = {bits}");
        Assert.IsTrue(result > 0 && result <= 8192 && result % 8 == 0,
            $"sched_getaffinity(0, 8192, buf) = {result}: the kernel answers the bytes it wrote, a positive multiple of 8");
        Assert.AreEqual(KernelAllowedCpus(), bits, "the bits set in those bytes against the kernel's own list of allowed CPUs");
    }

    [TestMethod]
    public void SchedGetaffinityReturnsTheNegativeErrno()
    {
        var (result, failure) = runtime_package.GoSchedGetaffinityErrorSignProbe();

        Assert.IsNull(failure, failure);
        Assert.AreEqual(-22, result, "sched_getaffinity over a 1-byte buffer: -EINVAL");
    }

    [TestMethod]
    public void GetCPUCountCountsTheAllowedCpus()
    {
        var (count, failure) = runtime_package.GoGetCPUCountProbe();

        Assert.IsNull(failure, failure);
        Assert.AreEqual(KernelAllowedCpus(), count, "getCPUCount against the kernel's own list of allowed CPUs");
    }

    [TestMethod]
    public void SetDefaultGOMAXPROCSReachesNoStub()
    {
        // Which side of its early return the call takes is the host's doing: the two counts differ under
        // DOTNET_PROCESSOR_COUNT or a CPU quota, and it then goes on through the stop-the-world pair.
        var before = runtime_package.GOMAXPROCS(0);

        Console.WriteLine($"READING host processor count = {Environment.ProcessorCount}, getCPUCount = {runtime_package.GoGetCPUCountProbe().count}");
        runtime_package.SetDefaultGOMAXPROCS();

        nint after = runtime_package.GOMAXPROCS(0);

        Console.WriteLine($"READING GOMAXPROCS(0) = {before} before SetDefaultGOMAXPROCS, {after} after");
        Assert.AreEqual(runtime_package.GoDefaultGOMAXPROCSProbe(), after, "SetDefaultGOMAXPROCS against Go's default");
    }

    // Go's default GOMAXPROCS is the CPUs the process may run on, lowered to the cgroup's CPU limit rounded
    // up, and never below 2 by that limit. Without a limit this reads the allowed CPUs; under one (a cgroup
    // with a quota) it reads the limit, which defaultGOMAXPROCSInit's cgroup makes reachable.
    [TestMethod]
    public void TheDefaultIsLoweredToTheCgroupCpuLimit()
    {
        int allowed = KernelAllowedCpus();
        double? limit = CgroupCpuLimit();
        int want = limit is double l ? Math.Min(allowed, Math.Max(2, (int)Math.Ceiling(l))) : allowed;

        Console.WriteLine($"READING allowed CPUs = {allowed}, cgroup CPU limit = {(limit is double v ? v.ToString() : "none")}");
        Assert.AreEqual((nint)want, runtime_package.GoDefaultGOMAXPROCSProbe(), "defaultGOMAXPROCS(0) against the allowed CPUs and the cgroup's limit");
    }

    [TestMethod]
    public void OsyieldReturns()
    {
        string? failure = runtime_package.GoOsyieldProbe(1000);

        Assert.IsNull(failure, failure);
    }

    [TestMethod]
    public void ExitEndsTheProcessWithItsStatus()
    {
        string scratch = Path.Combine(Path.GetTempPath(), "g2cs-exit-" + Guid.NewGuid().ToString("N"));
        string record = Path.Combine(scratch, "record.txt");
        string filter = $"FullyQualifiedName={typeof(RuntimeLinuxSchedExitTests).FullName}.{nameof(ChildCallsExit)}";

        Directory.CreateDirectory(scratch);

        ProcessStartInfo start = new("dotnet", ["test", typeof(RuntimeLinuxSchedExitTests).Assembly.Location, "--filter", filter, "--diag", Path.Combine(scratch, "diag", "log.txt")])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[RecordVariable] = record;

        try
        {
            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(180_000))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail("the exit child did not finish in 180 s");
            }

            string output = $"exit child:\n{stdout.Result}{stderr.Result}";

            Assert.IsTrue(File.Exists(record), $"the exit child wrote no record:\n{output}");

            string[] readings = File.ReadAllLines(record);

            CollectionAssert.Contains(readings, "before", $"the child never reached its call:\n{output}");
            Assert.AreEqual("before", string.Join(";", readings), "runtime.exit came back to its caller instead of ending the process");
            Assert.AreNotEqual(0, process.ExitCode, $"the run whose test host exited should not report success:\n{output}");

            // The test host's own status, as the runner logs it: `dotnet test` reports only that the host went away.
            string[] statuses = Directory.EnumerateFiles(Path.Combine(scratch, "diag"))
                .SelectMany(File.ReadLines)
                .Where(l => l.Contains(ExitStatusMarker, StringComparison.Ordinal))
                .Select(l => l[(l.IndexOf(ExitStatusMarker, StringComparison.Ordinal) + ExitStatusMarker.Length)..].TrimStart().Split(' ')[0])
                .ToArray();

            Assert.IsTrue(statuses.Length > 0, $"the runner's diagnostic log names no test host exit status (\"{ExitStatusMarker}\"): the instrument needs a look, not the body");
            CollectionAssert.Contains(statuses, ExitStatus.ToString(), $"the test host's exit status: {string.Join(", ", statuses)}");
        }
        finally
        {
            Directory.Delete(scratch, recursive: true);
        }
    }

    // The child's half. Outside a child it returns at once, as Go's helper processes do.
    [TestMethod]
    public void ChildCallsExit()
    {
        string? record = Environment.GetEnvironmentVariable(RecordVariable);

        if (string.IsNullOrEmpty(record))
            return;

        File.AppendAllLines(record, ["before"]);

        try
        {
            runtime_package.GoExitProbe(ExitStatus);
            File.AppendAllLines(record, ["returned"]);
        }
        catch (Exception ex)
        {
            File.AppendAllLines(record, [$"threw={ex.GetType().Name}"]);
        }
    }
}

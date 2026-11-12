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
//   4. SetDefaultGOMAXPROCS, the public route to the stub, returns.
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

        // Read, not asserted: past the early return the call sets newprocs, which no hand-owned code reads, so
        // the setting does not move (the BOARD's 2026-11-11 row).
        Console.WriteLine($"READING GOMAXPROCS(0) = {before} before SetDefaultGOMAXPROCS, {runtime_package.GOMAXPROCS(0)} after");
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

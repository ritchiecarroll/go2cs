using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class LinuxDescriptorLimitTests
{
    // syscall's TestPrlimitFileLimit (syscall_linux_test.go:720-734) lowers the process's SOFT
    // RLIMIT_NOFILE to 43 and never restores the PROCESS limit -- its deferred Store puts back only
    // syscall's cached origRlimitNofile pointer. A Go test binary runs the rest of its tests inside 43
    // descriptors. The converted host did not survive it: on the go1.24.13 Linux leg the next test's
    // start threw OutOfMemoryException out of Thread.StartCore and 19 of syscall's rows never ran.
    //
    // Two arms. MECHANISM: under the same limit a managed Thread.Start fails with the host's shape
    // while a NATIVE pthread_create -- a thread with no runtime attached -- succeeds, so what the limit
    // denies is a DESCRIPTOR the CLR's own thread start opens, not the thread; the arm then measures
    // how many free descriptors a managed thread start needs. HOST: a registry whose first test lowers
    // the limit the way TestPrlimitFileLimit does and whose second test is empty -- the runner must
    // start the second test and finish clean, and the limit must be back where the run began.
    //
    // Linux-only by construction (RLIMIT_NOFILE is 7 here, /proc/self/fd is Linux's); the csproj
    // removes this file under any other $(GoTargetOS), and a non-linux host reports Inconclusive.

    [StructLayout(LayoutKind.Sequential)]
    private struct RLimit
    {
        public ulong Cur;
        public ulong Max;
    }

    private const int RLIMIT_NOFILE = 7;
    private const ulong GoMagicLimit = 43; // magicRlimitValue + 1, syscall_linux_test.go:734

    [DllImport("libc", SetLastError = true)]
    private static extern int getrlimit(int resource, out RLimit rlim);

    [DllImport("libc", SetLastError = true)]
    private static extern int setrlimit(int resource, in RLimit rlim);

    [DllImport("libc", SetLastError = true)]
    private static extern int open(string path, int flags);

    [DllImport("libc")]
    private static extern int close(int fd);

    [DllImport("libc")]
    private static extern int pthread_create(out nint thread, nint attr, nint startRoutine, nint arg);

    [DllImport("libc")]
    private static extern int pthread_join(nint thread, nint retval);

    private static void RequireLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Assert.Inconclusive("RLIMIT_NOFILE and /proc/self/fd are the linux flavour's");
    }

    private static RLimit Current()
    {
        Assert.AreEqual(0, getrlimit(RLIMIT_NOFILE, out RLimit limit), "getrlimit(RLIMIT_NOFILE)");
        return limit;
    }

    // Runs body with the SOFT limit at `soft` and ALWAYS puts the original back -- setrlimit opens no
    // descriptor, so the restore cannot itself be denied by the limit it is undoing.
    private static T UnderSoftLimit<T>(ulong soft, Func<T> body)
    {
        RLimit original = Current();

        try
        {
            Assert.AreEqual(0, setrlimit(RLIMIT_NOFILE, new RLimit { Cur = soft, Max = original.Max }), $"setrlimit(NOFILE, {soft})");
            return body();
        }
        finally
        {
            setrlimit(RLIMIT_NOFILE, original);
        }
    }

    private static int OpenDescriptors() => Directory.GetFileSystemEntries("/proc/self/fd").Length;

    private static Exception? TryManagedThreadStart()
    {
        try
        {
            Thread thread = new(() => { });
            thread.Start();
            thread.Join();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // getpid as the start routine: it ignores its argument and returns at once, so the thread lives
    // and dies without ever entering managed code.
    private static int TryNativeThread()
    {
        nint getpid = NativeLibrary.GetExport(NativeLibrary.Load("libc.so.6"), "getpid");
        int rc = pthread_create(out nint thread, 0, getpid, 0);

        if (rc == 0)
            pthread_join(thread, 0);

        return rc;
    }

    [TestMethod]
    public void AManagedThreadStartNeedsDescriptorsGosLimitDenies()
    {
        RequireLinux();

        int open = OpenDescriptors();
        Assert.IsTrue((ulong)open > GoMagicLimit,
            $"PREMISE: this host holds {open} descriptors, so Go's {GoMagicLimit} admits no new one; below that the arm proves nothing");

        (Exception? managed, int native) = UnderSoftLimit(GoMagicLimit, () => (TryManagedThreadStart(), TryNativeThread()));

        Assert.IsInstanceOfType(managed, typeof(OutOfMemoryException),
            $"under NOFILE {GoMagicLimit} a managed Thread.Start must fail the way the syscall row's host did, got {managed?.GetType().Name ?? "success"}");
        Assert.AreEqual(0, native,
            "a NATIVE pthread_create under the same limit must succeed: the thread itself is admissible, so what is denied is a descriptor the runtime's thread start opens");

        // The headroom a managed thread start needs: the smallest k with the soft limit at open + k that
        // lets it through. Re-read the descriptor count per step, since a successful start can leave
        // the process holding a different number than before.
        int? needed = null;

        for (int k = 0; k <= 16 && needed is null; k++)
        {
            int now = OpenDescriptors();

            if (UnderSoftLimit((ulong)(now + k), TryManagedThreadStart) is null)
                needed = k;
        }

        Assert.IsNotNull(needed, "no headroom up to 16 descriptors let a managed thread start through");
        Console.WriteLine($"managed Thread.Start needs {needed} free descriptor(s) (host held {open} at the arm's start)");
        Assert.IsTrue(needed >= 1, "a managed thread start that needs ZERO free descriptors could not have failed under Go's limit");
    }

    [TestMethod]
    public void TheHostRestoresTheDescriptorLimitATestLowered()
    {
        RequireLinux();

        RLimit original = Current();
        Assert.IsTrue(original.Cur > GoMagicLimit, $"PREMISE: the run's limit {original.Cur} must exceed Go's {GoMagicLimit}");

        TestRegistry registry = new("guard", []);
        bool secondRan = false;

        // Ordinal order puts the lowering test first, which is TestPrlimitFileLimit's position relative to
        // everything the syscall row still had to run.
        registry.Add("TestALowersTheLimit", _ =>
            Assert.AreEqual(0, setrlimit(RLIMIT_NOFILE, new RLimit { Cur = GoMagicLimit, Max = original.Max }), "the lowering test's setrlimit"),
            "guard.go", 1);
        registry.Add("TestBRunsAfterward", _ => secondRan = true, "guard.go", 2);

        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(registry, new TestOptions(), reporter, ".", ".");

        nint exit;
        Exception? thrown = null;

        try
        {
            exit = runner.RunAll();
        }
        catch (Exception ex)
        {
            thrown = ex;
            exit = -1;
        }
        finally
        {
            // Put the run's limit back whatever happened, BEFORE any assertion: a red run must not leave
            // the rest of GolibTests inside 43 descriptors.
            RLimit after = Current();
            setrlimit(RLIMIT_NOFILE, original);

            Assert.AreEqual(original.Cur, after.Cur,
                $"the host must restore RLIMIT_NOFILE after the test that lowered it: the run began at {original.Cur} and ended at {after.Cur}");
        }

        Assert.IsNull(thrown, $"the run must survive a test that lowered the limit, got {thrown?.GetType().Name}: {thrown?.Message}");
        Assert.IsTrue(secondRan, "the test after the lowering one must start and run");
        Assert.AreEqual((nint)0, exit, "both tests pass, so the run exits 0");
    }

    // RLIMIT_NOFILE bounds the descriptor NUMBER an open may return, not how many are open. A long-lived
    // host (the full GolibTests run) has holes in its table, so a limit sized from the COUNT still leaves
    // free numbers below it. Punch holes deliberately and the headroom arm must read what it reads on a
    // table without them.
    [TestMethod]
    public void TheHeadroomArmIsNotFooledByHolesInTheDescriptorTable()
    {
        RequireLinux();

        List<int> opened = [];

        try
        {
            for (int i = 0; i < 64; i++)
            {
                int fd = open("/dev/null", 0);
                Assert.IsTrue(fd >= 0, $"open(/dev/null) #{i}");
                opened.Add(fd);
            }

            // Every other one: 32 holes, spread across the numbers the count-sized limit admits.
            for (int i = 0; i < opened.Count; i += 2)
                close(opened[i]);

            opened = opened.Where((_, i) => i % 2 == 1).ToList();

            AManagedThreadStartNeedsDescriptorsGosLimitDenies();
        }
        finally
        {
            foreach (int fd in opened)
                close(fd);
        }
    }
}

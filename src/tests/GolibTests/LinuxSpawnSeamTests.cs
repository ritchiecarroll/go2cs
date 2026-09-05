using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using syscall = go.syscall_package;
using errors = go.errors_package;

namespace GolibTests;

[TestClass]
public class LinuxSpawnSeamTests
{
    // The exec-wall implementation's two measured gates (design §5.1 and OQ-2, ratified
    // 2026-08-22). Both drive the CONVERTED surface — syscall.StartProcess/Wait4 — because the
    // seam under test is the posix_spawn hand-own behind forkExec, not a P/Invoke in isolation.
    // Linux-only by construction (the seam is the linux flavor); on Windows both report
    // Inconclusive rather than vacuous green.

    // §5.1: glibc reports child-setup and exec failures SYNCHRONOUSLY from posix_spawn — the
    // property that lets the hand-own delete Go's status-pipe protocol. A missing binary must
    // fail HERE, as ENOENT from Start, never as a child that exits 127.
    [TestMethod]
    public void SpawnFailureIsSynchronous()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        var attr = new go.syscall_package.ProcAttr
        {
            Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
        };

        var (pid, _, err) = syscall.StartProcess(
            "/nonexistent-go2cs-spawn-gate"u8,
            new slice<@string>(new @string[] { "/nonexistent-go2cs-spawn-gate"u8 }),
            new StandardBox<go.syscall_package.ProcAttr>(attr));

        Assert.AreEqual((nint)0, pid, "a failed spawn must not report a pid");
        Assert.IsNotNull(err, "spawning a missing binary must fail synchronously");

        // The corpus's own errno-comparison idiom: the error interface carries a boxed Errno, and
        // AreEqual is what converted Go uses for `err == ENOENT`. Asserting the VALUE (not a
        // rendered message) is both stronger and rendering-independent.
        Assert.IsTrue(AreEqual(err, syscall.ENOENT),
            $"expected ENOENT from the spawn call itself, got: {err}");
    }

    // OQ-2: the CLR installs its own SIGCHLD handling for System.Diagnostics.Process; the gate
    // proves its reaper is pid-targeted and does NOT steal children this seam spawns. The child
    // exits UNOBSERVED (well before the wait), GC pressure runs meanwhile, and a delayed Wait4
    // must still return the pid with its exit status — ECHILD here would mean the runtime reaped
    // our child out from under Go's wait protocol, the failure mode the design refuses to assume
    // away (the Mono precedent).
    [TestMethod]
    public void UnobservedChildSurvivesUntilWait()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        var attr = new go.syscall_package.ProcAttr
        {
            Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
        };

        var (pid, _, err) = syscall.StartProcess(
            "/bin/true"u8,
            new slice<@string>(new @string[] { "/bin/true"u8 }),
            new StandardBox<go.syscall_package.ProcAttr>(attr));

        Assert.IsNull(err, $"spawning /bin/true failed: {err}");
        Assert.IsTrue(pid > 0, "no pid from a successful spawn");

        // Let the child exit unobserved while the GC churns — the window where a wrong reaper
        // would consume the zombie.
        for (int i = 0; i < 8; i++)
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            System.Threading.Thread.Sleep(200);
        }

        ref var status = ref heap(new go.syscall_package.WaitStatus(), out var Ꮡstatus);
        var (waited, werr) = syscall.Wait4(pid, Ꮡstatus, 0, nil);

        Assert.IsNull(werr, $"Wait4 failed — ECHILD here means the runtime's reaper stole the child: {werr}");
        Assert.AreEqual(pid, waited, "Wait4 returned a different pid");
        Assert.IsTrue(status.Exited(), "child status did not decode as exited");
        Assert.AreEqual((nint)0, status.ExitStatus(), "/bin/true must exit 0");
    }

    // §3.3: the honest wall must answer in GO'S OWN ERROR CURRENCY, not merely with a name. The
    // design already said so — "returns ENOTSUP naming the field ... never a silent drop (reach
    // Go's own gate, fail Go's own way)" — and the implementation drifted to a bare
    // errors.New(message), which names the field but carries no kind.
    //
    // The kind is load-bearing, not cosmetic. Go's own skip guards call
    // testenv.SyscallIsNotSupported, which accepts an Errno of EPERM/EROFS/EINVAL,
    // fs.ErrPermission, or errors.ErrUnsupported — and nothing else. A kindless refusal satisfies
    // none of them, so on an unprivileged host EIGHT tests in Go's syscall suite that Go SKIPS (it
    // attempts the operation, the kernel answers EPERM, the guard fires) instead FAIL against the
    // converted corpus. The sibling hand-own syscall_linux_impl.cs already fixed this exact shape
    // for runtime_doAllThreadsSyscall, recording the same reasoning: a throwing stub "turned that
    // skip into an infrastructure-error", and ENOTSUP restored it.
    //
    // Asserting the KIND and the NAME, never the rendered message — this file's established idiom.
    [TestMethod]
    public void UnsupportedSysProcAttrFieldAnswersErrUnsupported()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        // Cloneflags is outside the mapped set, so the seam must refuse before it ever spawns.
        // /bin/true exists and would otherwise succeed — the refusal itself is under test.
        var sys = new go.syscall_package.SysProcAttr
        {
            Cloneflags = (uintptr)go.syscall_package.CLONE_NEWUSER,
        };

        var attr = new go.syscall_package.ProcAttr
        {
            Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
            Sys = new StandardBox<go.syscall_package.SysProcAttr>(sys),
        };

        var (pid, _, err) = syscall.StartProcess(
            "/bin/true"u8,
            new slice<@string>(new @string[] { "/bin/true"u8 }),
            new StandardBox<go.syscall_package.ProcAttr>(attr));

        Assert.AreEqual((nint)0, pid, "a refused spawn must not report a pid");
        Assert.IsNotNull(err, "an unmapped SysProcAttr field must be refused, never silently dropped");

        // THE GATE: Go's own predicate must accept this error. errors.Is walks the chain and
        // consults Errno.Is, which maps ENOTSUP/ENOSYS/EOPNOTSUPP onto errors.ErrUnsupported.
        // err.Error(), not err: interpolating the error INTERFACE renders the box address
        // ("Got: 0x716590d03af0" in this witness's own first red run), which tells a reader
        // nothing about why the gate failed.
        Assert.IsTrue(errors.Is(err, errors.ErrUnsupported),
            "the seam's refusal must satisfy errors.Is(err, errors.ErrUnsupported) — that is what "
          + $"testenv.SyscallIsNotSupported tests, and eight syscall-suite skips ride on it. "
          + $"Got: {err.Error()}");

        // The design's other half: the wall stays NAMED. A kind without a name would trade one
        // regression for another.
        StringAssert.Contains(err.Error().ToString(), "Cloneflags",
            "the refusal must still name the field it could not express");
    }

    // ---- SysProcAttr.Foreground through the seam (Q15 half 2, 2026-09-04) ----
    // Go's child performs setpgid + ioctl(Ctty, TIOCSPGRP) between fork and exec with every signal
    // blocked; posix_spawn has no such action, so the seam maps it to SETPGROUP plus a parent-side
    // TIOCSPGRP after the spawn returns, SIGTTOU blocked on the calling thread (DESIGN-linux-exec.md
    // section 3.3 as ruled). Two arms: the ioctl is REACHED with the caller's Ctty -- a pipe answers
    // ENOTTY from the ioctl, where the old wall answered ENOTSUP before any spawn -- and, where this
    // process has a controlling terminal, the transfer itself: the child's group is its own pid and
    // the terminal's foreground group. The second arm is measured under a pty (util-linux `script`,
    // which makes the child a session leader with a controlling terminal); without one it is
    // Inconclusive BY NAME, never green by vacuity. Both were skip/skip on every fleet sweep because
    // the detached driver has no terminal -- Go skips them too -- which is why the divergence was
    // invisible until a run under a pty (Go: pass/pass; converted: fail / infrastructure-error).

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, ulong request, ref int arg);

    [DllImport("libc")] private static extern int sigemptyset(IntPtr set);
    [DllImport("libc")] private static extern int sigaddset(IntPtr set, int signum);
    [DllImport("libc")] private static extern int pthread_sigmask(int how, IntPtr set, IntPtr oldset);

    private const ulong TIOCGPGRP = 0x540f;
    private const ulong TIOCSPGRP = 0x5410;

    private static int Tcgetpgrp(int fd)
    {
        int pgrp = 0;
        return ioctl(fd, TIOCGPGRP, ref pgrp) == 0 ? pgrp : -1;
    }

    // tcsetpgrp from a process that may no longer be the foreground group: SIGTTOU blocked on this
    // thread for the call, as Go's TestForeground ignores it for the restore.
    private static void TcsetpgrpBlockingTtou(int fd, int pgrp)
    {
        IntPtr set = Marshal.AllocHGlobal(128), old = Marshal.AllocHGlobal(128);
        try
        {
            sigemptyset(set);
            sigaddset(set, 22 /* SIGTTOU */);
            pthread_sigmask(0 /* SIG_BLOCK */, set, old);
            int arg = pgrp;
            ioctl(fd, TIOCSPGRP, ref arg);
            pthread_sigmask(2 /* SIG_SETMASK */, old, IntPtr.Zero);
        }
        finally
        {
            Marshal.FreeHGlobal(set);
            Marshal.FreeHGlobal(old);
        }
    }

    [TestMethod]
    public void ForegroundReachesTheTerminalIoctl()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        // A pipe is not a terminal: TIOCSPGRP on it is ENOTTY, and only a seam that reached the
        // ioctl can answer that -- the old wall refused Foreground as ENOTSUP before any spawn.
        // O_CLOEXEC on every descriptor the guard opens: raw syscall.Pipe is Pipe2(p, 0) in Go too, and a
        // child that inherits the write end of its own stdin pipe never sees EOF (measured: the first
        // form of the terminal arm hung in Wait4 behind a `cat` holding both pipe ends and /dev/tty).
        var fds = new slice<nint>(new nint[2]);
        error perr = syscall.Pipe2(fds, (nint)syscall.O_CLOEXEC);
        Assert.IsNull(perr, $"pipe: {perr}");

        try
        {
            var sys = new go.syscall_package.SysProcAttr { Foreground = true, Ctty = fds[0] };
            var attr = new go.syscall_package.ProcAttr
            {
                Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
                Sys = new StandardBox<go.syscall_package.SysProcAttr>(sys),
            };

            var (pid, _, err) = syscall.StartProcess(
                "/bin/true"u8,
                new slice<@string>(new @string[] { "/bin/true"u8 }),
                new StandardBox<go.syscall_package.ProcAttr>(attr));

            Assert.AreEqual((nint)0, pid, "a spawn whose foreground transfer failed must not report a pid -- the seam reaps the child it had already spawned");
            Assert.IsNotNull(err, "TIOCSPGRP on a pipe must fail");
            Assert.IsTrue(errors.Is(err, syscall.ENOTTY),
                $"the seam must reach TIOCSPGRP on the given Ctty and surface ITS errno (ENOTTY for a pipe); got: {err.Error()}");
            Assert.IsFalse(errors.Is(err, errors.ErrUnsupported), "Foreground must no longer be refused as unsupported");
        }
        finally
        {
            syscall.Close(fds[0]);
            syscall.Close(fds[1]);
        }
    }

    [TestMethod]
    public void ForegroundPlacesTheChildsGroupInTheTerminalsForeground()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        var (tty, oerr) = syscall.Open("/dev/tty"u8, (nint)(syscall.O_RDWR | syscall.O_CLOEXEC), 0);

        if (oerr != null)
        {
            Assert.Inconclusive("no controlling terminal: run under a pty (util-linux `script -qfc`) to measure the transfer");
            return;
        }

        int fpgrp = Tcgetpgrp((int)tty);
        Assert.IsTrue(fpgrp > 0, "TIOCGPGRP reported no foreground group on the controlling terminal");

        var stdin = new slice<nint>(new nint[2]);
        Assert.IsNull(syscall.Pipe2(stdin, (nint)syscall.O_CLOEXEC), "pipe");

        nint pid = 0;

        try
        {
            var sys = new go.syscall_package.SysProcAttr { Foreground = true, Ctty = tty };
            var attr = new go.syscall_package.ProcAttr
            {
                Files = new slice<uintptr>(new uintptr[] { (uintptr)stdin[0], 1, 2 }),
                Sys = new StandardBox<go.syscall_package.SysProcAttr>(sys),
            };

            error err;
            (pid, _, err) = syscall.StartProcess(
                "/bin/cat"u8,
                new slice<@string>(new @string[] { "/bin/cat"u8 }),
                new StandardBox<go.syscall_package.ProcAttr>(attr));

            Assert.IsNull(err, $"spawn with Foreground under a controlling terminal failed: {err}");

            var (cpgrp, gerr) = syscall.Getpgid(pid);
            Assert.IsNull(gerr, $"Getpgid: {gerr}");
            Assert.AreEqual(pid, cpgrp, "the child's process group must be the child's own pid (Pgid 0)");
            Assert.AreEqual((int)pid, Tcgetpgrp((int)tty), "the child's group must be the terminal's foreground group after the transfer");
        }
        finally
        {
            syscall.Close(stdin[1]); // EOF: cat exits

            if (pid != 0)
            {
                ref var status = ref heap(new go.syscall_package.WaitStatus(), out var Ꮡstatus);
                syscall.Wait4(pid, Ꮡstatus, 0, nil);
            }

            TcsetpgrpBlockingTtou((int)tty, fpgrp); // restore the foreground group, as Go's test does
            syscall.Close(stdin[0]);
            syscall.Close(tty);
        }
    }
    // The Foreground FAILURE path owes a reap, and until this guard it did not perform one.
    //
    // The seam spawns the child, then transfers the terminal's foreground group from the PARENT;
    // when that ioctl fails it SIGKILLs the child it just created and returns the errno. Go's
    // parent, on its own child-setup failure, Wait4s the pid in an EINTR retry loop first -- "to
    // make sure the zombies don't accumulate" (exec_unix.go:234-239) -- and the comment on this
    // path CLAIMED that reap while the code only killed. Nothing else absorbs the child:
    // StartProcess returns pid 0 here, so no os.Process is ever built and the caller has no Wait
    // that could reach it. The zombie would live as long as this process does.
    //
    // Found by C2 in the darwin twin of this seam, 2026-09-05.
    //
    // The gate needs NO controlling terminal, which is what lets it run on every linux host where
    // the sibling Foreground transfer test goes Inconclusive: a NON-tty Ctty (/dev/null) makes the
    // kernel answer ENOTTY, and that IS the failure path.
    //
    // Vacuity: asserting ENOTTY is what proves a child existed to be reaped. That errno can only
    // come from the ioctl, which runs only after posix_spawn has RETURNED A CHILD -- so a spawn
    // that failed earlier (a missing binary, a refused SysProcAttr) cannot satisfy this test with
    // no child in play. Without that assertion the whole gate would be green on an empty path.
    [TestMethod]
    public void AChildKilledOnTheForegroundFailurePathIsReaped()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the posix_spawn seam is the linux flavor");
            return;
        }

        var (devnull, oerr) = syscall.Open("/dev/null"u8, (nint)(syscall.O_RDWR | syscall.O_CLOEXEC), 0);
        Assert.IsNull(oerr, $"opening /dev/null: {oerr}");

        try
        {
            // Only children this call creates may be blamed, so the ones already ours are excluded.
            System.Collections.Generic.HashSet<int> before = OwnChildPids();

            var sys = new go.syscall_package.SysProcAttr { Foreground = true, Ctty = devnull };
            var attr = new go.syscall_package.ProcAttr
            {
                Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
                Sys = new StandardBox<go.syscall_package.SysProcAttr>(sys),
            };

            // A child that STAYS ALIVE until it is killed: if it exited on its own the reap would
            // be untested, because a self-exited child is still a zombie nobody waited for.
            var (pid, _, err) = syscall.StartProcess(
                "/bin/sleep"u8,
                new slice<@string>(new @string[] { "/bin/sleep"u8, "30"u8 }),
                new StandardBox<go.syscall_package.ProcAttr>(attr));

            Assert.AreEqual((nint)0, pid, "the failure path must not report a pid");
            Assert.IsNotNull(err, "a non-tty Ctty under Foreground must fail");
            Assert.IsTrue(AreEqual(err, syscall.ENOTTY),
                $"expected ENOTTY from the TIOCSPGRP transfer (which proves a child was spawned), got: {err}");

            // The reap is synchronous inside StartProcess, so this is true on the first look with
            // the wait in place. Polling is for the neutered direction: without it the SIGKILLed
            // child is a zombie that NEVER leaves, and no amount of waiting clears it.
            System.Collections.Generic.HashSet<int> leaked = new();

            for (int i = 0; i < 100; i++)
            {
                leaked = OwnChildPids();
                leaked.ExceptWith(before);

                if (leaked.Count == 0)
                    break;

                System.Threading.Thread.Sleep(50);
            }

            Assert.AreEqual(0, leaked.Count,
                "the killed child was never reaped -- still our child(ren): " +
                string.Join(", ", System.Linq.Enumerable.Select(leaked, k => $"{k} (state {ProcState(k)})")));
        }
        finally
        {
            syscall.Close(devnull);
        }
    }

    // Every pid whose PPID is this process. /proc/<pid>/stat's second field is the comm in
    // parentheses and CAN contain spaces and parentheses, so the fields are read after the LAST
    // ')' -- state first, PPID second. A pid that exits between the enumeration and the read is
    // simply not ours to count.
    private static System.Collections.Generic.HashSet<int> OwnChildPids()
    {
        int self = Environment.ProcessId;
        System.Collections.Generic.HashSet<int> kids = new();

        foreach (string dir in System.IO.Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(System.IO.Path.GetFileName(dir), out int pid))
                continue;

            string[] fields = StatFieldsAfterComm(pid);

            if (fields.Length > 1 && int.TryParse(fields[1], out int ppid) && ppid == self)
                kids.Add(pid);
        }

        return kids;
    }

    private static string ProcState(int pid)
    {
        string[] fields = StatFieldsAfterComm(pid);
        return fields.Length > 0 ? fields[0] : "gone";
    }

    private static string[] StatFieldsAfterComm(int pid)
    {
        string stat;

        try
        {
            stat = System.IO.File.ReadAllText($"/proc/{pid}/stat");
        }
        catch
        {
            return Array.Empty<string>();
        }

        int close = stat.LastIndexOf(')');

        return close < 0
            ? Array.Empty<string>()
            : stat[(close + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}

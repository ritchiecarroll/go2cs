using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using syscall = go.syscall_package;
using errors = go.errors_package;

namespace GolibTests;

// darwin's hand-owned native calls enter a system call the way Go's do (docs/phase4/DESIGN-native-call-seam.md
// §13.2, S-b2's darwin rows). Every libcCall caller is already inside the window: the converted darwin runtime's
// syscall_syscalln calls entersyscall, which is S-b's hand-owned family. Three hand-owned sites call libc
// directly instead, and Go makes each of them through syscall(), whose entersyscall moves the goroutine to
// _Gsyscall for the length of the call:
//   - syscall's posix_spawn, the parent's wait for the child's exec (Go's parent waits in readlen on the exec
//     pipe, a bracketed read);
//   - syscall's kill on the Foreground-failure path (Go's kill is syscall(), zsyscall_darwin_amd64.go:403);
//   - os's readdir_r (Go's syscall.readdir_r is syscall(), zsyscall_darwin_amd64.go:1260).
//
// What runs in each window here is an [UnmanagedCallersOnly] probe in place of the libc function, reached through
// the same helper the production call goes through (the packages' Go*Call seams make the one call). It reads the
// goroutine's status and the seam's transition depth from INSIDE the window, on the goroutine's own thread: S-b's
// shape. It re-enters no Go code, so nothing in it is owed a ReenterManaged.
//
// The last test drives the real kill path end to end: a Foreground spawn whose Ctty is a pipe makes the parent's
// TIOCSPGRP fail with ENOTTY, and the seam then kills and reaps the child and returns that errno.
//
// Darwin-only by nature: the three seams are declared by the darwin flavour (GolibTests.csproj removes this file
// for any other).
[TestClass]
public unsafe class DarwinNativeTransitionTests
{
    private const uint Grunning = 2;

    [ThreadStatic]
    private static uint t_statusInside;

    [ThreadStatic]
    private static int t_depthInside;

    private static void RecordInside()
    {
        t_statusInside = runtime_package.GoCurrentGStatus();
        t_depthInside = GoNativeCall.TransitionDepth;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int PosixSpawnProbe(int* pid, nint path, nint fileActions, nint attrp, nint argv, nint envp)
    {
        RecordInside();
        return 41;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int KillProbe(int pid, int sig)
    {
        RecordInside();
        return 42;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int ReaddirRProbe(nint dir, nint entry, nint result)
    {
        RecordInside();
        return 43;
    }

    [TestMethod]
    public void APosixSpawnCallRunsInsideGsyscallAndOneSeamTransition() =>
        AssertOneWindow(() => syscall.GoPosixSpawnCall((nint)(delegate* unmanaged[Cdecl]<int*, nint, nint, nint, nint, nint, int>)&PosixSpawnProbe), 41, "posix_spawn");

    [TestMethod]
    public void AKillCallRunsInsideGsyscallAndOneSeamTransition() =>
        AssertOneWindow(() => syscall.GoKillCall((nint)(delegate* unmanaged[Cdecl]<int, int, int>)&KillProbe), 42, "kill");

    [TestMethod]
    public void AReaddirRCallRunsInsideGsyscallAndOneSeamTransition() =>
        AssertOneWindow(() => os_package.GoReaddirRCall((nint)(delegate* unmanaged[Cdecl]<nint, nint, nint, int>)&ReaddirRProbe), 43, "readdir_r");

    private static void AssertOneWindow(Func<int> call, int want, string name)
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive($"darwin's {name} bracket: NOT MEASURED on this host");
            return;
        }

        uint before = 0, inside = 0, after = 0;
        int depthBefore = -2, depthInside = -2, depthAfter = -2;
        int result = 0;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                before = runtime_package.GoCurrentGStatus();
                depthBefore = GoNativeCall.TransitionDepth;

                result = call();

                inside = t_statusInside;
                depthInside = t_depthInside;
                after = runtime_package.GoCurrentGStatus();
                depthAfter = GoNativeCall.TransitionDepth;
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not finish");
        Assert.IsNull(thrown, $"the {name} call threw on a goroutine: {thrown}");
        Assert.AreEqual(want, result, $"premise: the probe ran as {name}'s native target");

        Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
        Assert.AreEqual(0, depthBefore, "and outside native code");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), inside, $"inside {name}'s native window it reads _Gsyscall, as Go's syscall() makes it");
        Assert.AreEqual(1, depthInside, "and the seam saw exactly one transition");
        Assert.AreEqual(Grunning, after, "and it leaves _Grunning");
        Assert.AreEqual(0, depthAfter, "with the transition closed");
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int* fds);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    // The real kill path, end to end. Go's child makes TIOCSPGRP on Ctty and the spawn fails with its errno; the
    // seam makes it from the parent after posix_spawn returns, so a failure there must kill and reap the child it
    // already started and return that errno. A pipe is not a terminal, so the ioctl answers ENOTTY.
    [TestMethod]
    public void AForegroundSpawnOnANonTerminalCttyKillsTheChildAndReturnsENOTTY()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("darwin's posix_spawn seam: NOT MEASURED on this host");
            return;
        }

        int* fds = stackalloc int[2];
        Assert.AreEqual(0, pipe(fds), "premise: a pipe for the Ctty");

        try
        {
            var sys = new syscall.SysProcAttr
            {
                Foreground = true,
                Ctty = fds[0],
            };

            var attr = new syscall.ProcAttr
            {
                Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
                Sys = new StandardBox<syscall.SysProcAttr>(sys),
            };

            var (pid, _, err) = syscall.StartProcess(
                "/usr/bin/true"u8,
                new slice<@string>(new @string[] { "/usr/bin/true"u8 }),
                new StandardBox<syscall.ProcAttr>(attr));

            Assert.AreEqual((nint)0, pid, "a failed Foreground transfer must not report a pid");
            Assert.IsNotNull(err, "the Foreground transfer onto a pipe must fail");
            Assert.IsTrue(errors.Is(err, syscall.ENOTTY), $"the seam must return the ioctl's ENOTTY, got {err.Error()}");
        }
        finally
        {
            close(fds[0]);
            close(fds[1]);
        }
    }
}

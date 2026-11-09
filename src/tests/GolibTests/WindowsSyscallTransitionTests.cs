using System;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// The windows syscall trampoline enters a system call the way Go's does (docs/phase4/DESIGN-native-call-seam.md §9
// F-2, §12.2; COORD's Q2 ruling). Go's syscall.SyscallN reaches runtime.cgocall, whose entersyscall moves the
// goroutine to _Gsyscall for the length of the call, so a goroutine profile taken during it reads "syscall". The
// trampoline here, dll_windows.cs's callNative, made golib's blocked mark and nothing else, so the g status stayed
// _Grunning, and the transition seam (GoNativeCall) was never entered.
//
// What runs in the native window here is an [UnmanagedCallersOnly] probe, reached through SyscallN exactly as any
// native function is: callNative cannot tell it from kernel32 code. It reads the goroutine's status and the seam's
// transition depth from INSIDE the window, on the goroutine's own thread. The probe stands in for native code, not
// for a callback: it re-enters no Go code, so nothing in it is owed a ReenterManaged (that is F-3, S-c).
//
// Windows-only by nature: SyscallN is declared only in syscall's windows/ folder (GolibTests.csproj removes this
// file for any other flavour).
[TestClass]
public unsafe class WindowsSyscallTransitionTests
{
    private const uint Grunning = 2;

    [ThreadStatic]
    private static uint t_statusInside;

    [ThreadStatic]
    private static int t_depthInside;

    [UnmanagedCallersOnly]
    private static nuint ReadTheTransitionFromInside()
    {
        t_statusInside = runtime_package.GoCurrentGStatus();
        t_depthInside = GoNativeCall.TransitionDepth;
        return 42;
    }

    [TestMethod]
    public void ASyscallNCallRunsInsideGsyscallAndOneSeamTransition()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("SyscallN's trampoline is windows': NOT MEASURED on this host");
            return;
        }

        uint before = 0, inside = 0, after = 0;
        int depthBefore = -2, depthInside = -2, depthAfter = -2;
        uintptr r1 = 0;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                before = runtime_package.GoCurrentGStatus();
                depthBefore = GoNativeCall.TransitionDepth;

                (r1, _, _) = syscall_package.SyscallN((uintptr)(nuint)(delegate* unmanaged<nuint>)&ReadTheTransitionFromInside);

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
        Assert.IsNull(thrown, $"the call threw on a goroutine: {thrown}");
        Assert.AreEqual((uintptr)42, r1, "premise: the probe ran as the call's native target");

        Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
        Assert.AreEqual(0, depthBefore, "and outside native code");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), inside, "inside the native window it reads _Gsyscall, as Go's cgocall makes it");
        Assert.AreEqual(1, depthInside, "and the seam saw exactly one transition (Q2: one path, entered once)");
        Assert.AreEqual(Grunning, after, "and it leaves _Grunning");
        Assert.AreEqual(0, depthAfter, "with the transition closed");
    }
}

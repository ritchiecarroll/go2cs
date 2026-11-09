using System;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// The linux cgocaller keystone enters a system call the way Go's does (docs/phase4/DESIGN-native-call-seam.md §13.2,
// S-b2's linux row). Go's syscall.cgocaller is runtime.syscall_cgocaller, which calls cgocall, and cgocall's
// entersyscall brackets the call to the C shim (runtime/cgocall.go:110-114 and :167, go1.26.10): for the length of
// the credential call the goroutine reads _Gsyscall, and the transition seam (GoNativeCall) is open. The converted
// keystone (syscall/linux/cgocaller_linux_impl.cs) made the indirect call and nothing else.
//
// What runs in the window here is an [UnmanagedCallersOnly] probe in place of a cgo_libc_* shim, reached through
// cgocaller exactly as the nine shims are (syscall's GoCgocaller seam makes the one call). It reads the goroutine's
// status and the seam's transition depth from INSIDE the window, on the goroutine's own thread: S-b's shape. It
// re-enters no Go code, so nothing in it is owed a ReenterManaged.
//
// Linux-only by nature: cgocaller's body is the linux flavour's (GolibTests.csproj removes this file for any other).
[TestClass]
public unsafe class LinuxCgocallerTransitionTests
{
    private const uint Grunning = 2;

    [ThreadStatic]
    private static uint t_statusInside;

    [ThreadStatic]
    private static int t_depthInside;

    [UnmanagedCallersOnly]
    private static nuint ReadTheTransitionFromInside(nuint argument)
    {
        t_statusInside = runtime_package.GoCurrentGStatus();
        t_depthInside = GoNativeCall.TransitionDepth;
        return argument + 1;
    }

    [TestMethod]
    public void ACgocallerCallRunsInsideGsyscallAndOneSeamTransition()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Inconclusive("the linux cgocaller keystone: NOT MEASURED on this host");
            return;
        }

        uint before = 0, inside = 0, after = 0;
        int depthBefore = -2, depthInside = -2, depthAfter = -2;
        uintptr result = 0;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                before = runtime_package.GoCurrentGStatus();
                depthBefore = GoNativeCall.TransitionDepth;

                result = syscall_package.GoCgocaller((uintptr)(nuint)(delegate* unmanaged<nuint, nuint>)&ReadTheTransitionFromInside, 41);

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
        Assert.AreEqual((uintptr)42, result, "premise: the probe ran as the call's native target, with its argument");

        Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
        Assert.AreEqual(0, depthBefore, "and outside native code");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), inside, "inside the shim's window it reads _Gsyscall, as Go's cgocall makes it");
        Assert.AreEqual(1, depthInside, "and the seam saw exactly one transition (Q2: one path, entered once)");
        Assert.AreEqual(Grunning, after, "and it leaves _Grunning");
        Assert.AreEqual(0, depthAfter, "with the transition closed");
    }
}

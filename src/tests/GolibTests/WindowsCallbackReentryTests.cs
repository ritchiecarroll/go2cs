using System;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// A syscall.NewCallback callback re-enters Go the way Go's cgocallbackg does (docs/phase4/DESIGN-native-call-seam.md
// §9 F-3, §6.3, §12.1's M7). A native call that calls back into Go code is still a native call from the outside, but
// the Go code inside the callback is RUNNING: cgocallbackg runs exitsyscall on the way in (the goroutine is
// _Grunning, holding a P) and reentersyscall on the way out (back to _Gsyscall). The callback shim
// (syscall_windows_callback_impl.cs) forwarded straight to the Go function, so the callback's Go code ran with the
// goroutine still _Gsyscall, inside an open native transition, and still marked blocked to a returning main.
//
// The native call here is SyscallN at the callback's own code pointer: native code that does nothing but call back.
// The arm reads the goroutine's status and the seam's transition depth from inside the Go callback. The control
// invokes the same callback pointer from managed code with no native call open, where the crossing must be inert
// (a foreign thread, or a pointer called outside any bracketed call, re-enters nothing).
//
// Windows-only by nature: NewCallback and SyscallN are declared only in syscall's windows/ folder.
[TestClass]
public unsafe class WindowsCallbackReentryTests
{
    private const uint Grunning = 2;

    private static (uint Status, int Depth) RunOnGoroutine(Func<(uint, int)> body)
    {
        (uint, int) result = default;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                result = body();
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
        Assert.IsNull(thrown, $"the arm threw on a goroutine: {thrown}");
        return result;
    }

    [TestMethod]
    public void ACallbackFromANativeCallRunsGoCodeAtGrunningOneLevelIn()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("NewCallback and SyscallN are windows': NOT MEASURED on this host");
            return;
        }

        uint statusInside = 0, statusAfter = 0;
        int depthInside = -2, depthAfter = -2;

        Func<uintptr> callback = () =>
        {
            statusInside = runtime_package.GoCurrentGStatus();
            depthInside = GoNativeCall.TransitionDepth;
            return 7;
        };

        uintptr code = syscall_package.NewCallback(callback);

        RunOnGoroutine(() =>
        {
            (uintptr r1, _, _) = syscall_package.SyscallN(code);
            Assert.AreEqual((uintptr)7, r1, "premise: the callback ran as the native call's body");

            statusAfter = runtime_package.GoCurrentGStatus();
            depthAfter = GoNativeCall.TransitionDepth;
            return (statusAfter, depthAfter);
        });

        Assert.AreEqual(Grunning, statusInside, "the callback's Go code runs _Grunning, as cgocallbackg's exitsyscall makes it");
        Assert.AreEqual(2, depthInside, "one level inside the native call: the trampoline's Enter, then the callback's ReenterManaged");
        Assert.AreEqual(Grunning, statusAfter, "the native call returns the goroutine to _Grunning");
        Assert.AreEqual(0, depthAfter, "with every level closed");
    }

    [TestMethod]
    public void ACallbackInvokedOutsideANativeCallReentersNothing()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("NewCallback is windows': NOT MEASURED on this host");
            return;
        }

        uint statusInside = 0;
        int depthInside = -2;

        Func<uintptr> callback = () =>
        {
            statusInside = runtime_package.GoCurrentGStatus();
            depthInside = GoNativeCall.TransitionDepth;
            return 9;
        };

        uintptr code = syscall_package.NewCallback(callback);

        RunOnGoroutine(() =>
        {
            nuint r = ((delegate* unmanaged[Stdcall]<nuint>)(nuint)code)();
            Assert.AreEqual((nuint)9, r, "premise: the callback ran");
            return (runtime_package.GoCurrentGStatus(), GoNativeCall.TransitionDepth);
        });

        Assert.AreEqual(Grunning, statusInside, "with no native call open the goroutine is simply running");
        Assert.AreEqual(0, depthInside, "and the crossing re-entered nothing");
    }
}

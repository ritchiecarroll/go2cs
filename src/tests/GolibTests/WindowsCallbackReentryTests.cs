using System;
using System.Runtime.InteropServices;
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
// The third arm is a FOREIGN thread (S-c2): a thread golib did not create, with no goroutine identity, the shape of a
// callback on a thread a C library created. Go's cgocallback finds no g there and takes an extra M (needAndBindM),
// runs the callback as a goroutine, and drops the M when it returns (dropm). The arm's thread comes from a raw
// CreateThread, and its start routine invokes the callback pointer directly, then stays alive while the test reads
// what the callback left behind: thread-statics die with their thread.
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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateThread(nint attributes, nuint stackSize, delegate* unmanaged<nint, uint> start, nint parameter, uint flags, out uint threadId);

    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(nint handle, uint milliseconds);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(nint handle);

    private sealed class ForeignThreadReading
    {
        public nuint Code;
        public nuint Result;
        public bool IdentityBefore, IdentityAfter;
        public int DepthBefore, DepthAfter, DepthInside;
        public string? NameBefore, NameAfter;
        public long GoidInside;
        public uint StatusInside;
        public WeakReference? Inside;
        public Exception? Thrown;
        public readonly ManualResetEventSlim Read = new();
        public readonly ManualResetEventSlim Release = new();
    }

    private static ForeignThreadReading? s_foreign;

    [UnmanagedCallersOnly]
    private static uint ForeignThreadMain(nint parameter)
    {
        ForeignThreadReading reading = s_foreign!;

        try
        {
            reading.IdentityBefore = Goroutine.Current is not null;
            reading.DepthBefore = GoNativeCall.TransitionDepth;
            reading.NameBefore = Thread.CurrentThread.Name;

            reading.Result = ((delegate* unmanaged[Stdcall]<nuint>)reading.Code)();

            reading.IdentityAfter = Goroutine.Current is not null;
            reading.DepthAfter = GoNativeCall.TransitionDepth;
            reading.NameAfter = Thread.CurrentThread.Name;
        }
        catch (Exception ex)
        {
            reading.Thrown = ex;
        }
        finally
        {
            reading.Read.Set();
        }

        reading.Release.Wait(TimeSpan.FromSeconds(30));
        return 0;
    }

    [TestMethod]
    public void ACallbackOnAForeignThreadRunsAsAGoroutineAndLeavesNothingBehind()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("NewCallback and CreateThread are windows': NOT MEASURED on this host");
            return;
        }

        ForeignThreadReading reading = new();

        Func<uintptr> callback = () =>
        {
            Goroutine? goroutine = Goroutine.Current;
            reading.GoidInside = goroutine?.Id ?? 0;
            reading.Inside = goroutine is null ? null : new WeakReference(goroutine);
            reading.StatusInside = runtime_package.GoCurrentGStatus();
            reading.DepthInside = GoNativeCall.TransitionDepth;
            return 11;
        };

        reading.Code = syscall_package.NewCallback(callback);
        s_foreign = reading;

        nint thread = CreateThread(0, 0, &ForeignThreadMain, 0, 0, out _);
        Assert.AreNotEqual((nint)0, thread, $"premise: CreateThread failed ({Marshal.GetLastPInvokeError()})");

        try
        {
            Assert.IsTrue(reading.Read.Wait(TimeSpan.FromSeconds(30)), "the foreign thread did not finish its reading");
            Assert.IsNull(reading.Thrown, $"the foreign thread threw: {reading.Thrown}");
            Assert.AreEqual((nuint)11, reading.Result, "premise: the callback ran");
            Assert.IsFalse(reading.IdentityBefore, "premise: a thread golib did not create has no goroutine identity");
            Assert.AreEqual(-1, reading.DepthBefore, "premise: and so no transition depth");

            // Inside: Go's cgocallback binds an extra M and runs the callback as a goroutine.
            Assert.AreNotEqual(0L, reading.GoidInside, "the callback runs as a goroutine, as Go's needm makes it");
            Assert.AreEqual(Grunning, reading.StatusInside, "running");
            Assert.AreEqual(0, reading.DepthInside, "outside any native transition: none is open on this thread");

            // After: Go's dropm. Nothing of the callback's goroutine survives on the thread.
            Assert.IsFalse(reading.IdentityAfter, "the identity is released when the callback returns");
            Assert.AreEqual(-1, reading.DepthAfter, "and the thread is back to no transition depth");
            Assert.AreEqual(reading.NameBefore, reading.NameAfter, "and the thread's name is its own again");
            Assert.IsNull(Goroutine.FromId(reading.GoidInside), "the goroutine is retired from the registry");

            // No thread-static residue holds the retired goroutine while its thread is still alive.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsFalse(reading.Inside!.IsAlive, "nothing on the still-living thread holds the retired goroutine");
        }
        finally
        {
            reading.Release.Set();
            WaitForSingleObject(thread, 30_000);
            CloseHandle(thread);
            s_foreign = null;
            GC.KeepAlive(callback);
        }
    }
}

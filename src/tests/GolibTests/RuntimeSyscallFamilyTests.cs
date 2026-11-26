using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// The entersyscall family (src/core/runtime/syscall_managed_impl.cs, COORD ruling 2026-09-22). The
// converted bodies read sys.GetCallerPC / GetCallerSP and hand a P to the scheduler; the runtime row's
// test host died on the first runtime.Entersyscall of TestPreemptionAfterSyscall's goroutines. The
// managed bodies move only the goroutine's status and record no frame; the intrinsics stay throwing.
[TestClass]
public class RuntimeSyscallFamilyTests
{
    private const uint Grunning = 2;

    // Runs body on a golib goroutine (the shape TestPreemptionAfterSyscall's fakeSyscall has) and
    // returns what it threw, if anything.
    private static Exception? OnGoroutine(Action body)
    {
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try { body(); }
            catch (Exception e) { thrown = e; }
            finally { done.Set(); }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not finish");
        return thrown;
    }

    [TestMethod]
    public void EntersyscallAndExitsyscallMoveOnlyTheStatus()
    {
        uint before = 0, during = 0, after = 0;
        (nuint pc, nuint sp, nuint bp) frame = default;

        Exception? thrown = OnGoroutine(() =>
        {
            before = runtime_package.GoCurrentGStatus();
            runtime_package.GoEntersyscall();
            during = runtime_package.GoCurrentGStatus();
            frame = runtime_package.GoCurrentSyscallFrame();
            runtime_package.GoExitsyscall();
            after = runtime_package.GoCurrentGStatus();
        });

        Assert.IsNull(thrown, $"the family threw on a goroutine: {thrown}");
        Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), during, "inside the call it reads _Gsyscall");
        Assert.AreEqual(((nuint)0, (nuint)0, (nuint)0), frame, "no Go PC/SP/FP is recorded for a managed syscall");
        Assert.AreEqual(Grunning, after, "and it leaves _Grunning");
    }

    [TestMethod]
    public void EntersyscallblockPairsWithExitsyscall()
    {
        uint during = 0, after = 0;

        Exception? thrown = OnGoroutine(() =>
        {
            runtime_package.GoEntersyscallblock();
            during = runtime_package.GoCurrentGStatus();
            runtime_package.GoExitsyscall();
            after = runtime_package.GoCurrentGStatus();
        });

        Assert.IsNull(thrown, $"the family threw on a goroutine: {thrown}");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), during);
        Assert.AreEqual(Grunning, after);
    }

    [TestMethod]
    public void AnExitWithoutAnEntryIsRefusedByName()
    {
        Exception? thrown = OnGoroutine(runtime_package.GoExitsyscall);

        Assert.IsInstanceOfType(thrown, typeof(PanicException), $"got {thrown?.GetType().Name}: {thrown?.Message}");
        StringAssert.Contains(thrown!.Message, "exitsyscall");
    }

    // An entry whose seam half throws leaves both halves as it found them. GoNativeCall.Enter refuses a second entry
    // ("already inside a native call"); before the rollback the status had already moved, so the goroutine stayed
    // _Gsyscall with no seam entered: the next exitsyscall would pass the status check and fail in the seam, and a
    // returning main would read the goroutine as blocked. The test opens the seam itself to force the refusal.
    [TestMethod]
    public void AnEntryWhoseSeamThrowsLeavesTheStatusRunning()
    {
        uint afterRefusal = 0, during = 0, after = 0;
        int depthAfterRefusal = -2, depthAfter = -2;
        Exception? refused = null;

        Exception? thrown = OnGoroutine(() =>
        {
            GoNativeCall.Enter();

            try
            {
                runtime_package.GoEntersyscall();
            }
            catch (Exception e)
            {
                refused = e;
            }

            afterRefusal = runtime_package.GoCurrentGStatus();
            depthAfterRefusal = GoNativeCall.TransitionDepth;
            GoNativeCall.Exit();
            depthAfter = GoNativeCall.TransitionDepth;

            // The goroutine is usable again: a normal pair moves the status and the seam as before.
            runtime_package.GoEntersyscall();
            during = runtime_package.GoCurrentGStatus();
            runtime_package.GoExitsyscall();
            after = runtime_package.GoCurrentGStatus();
        });

        Assert.IsNull(thrown, $"the goroutine threw outside the forced refusal: {thrown}");
        Assert.IsInstanceOfType(refused, typeof(PanicException), $"got {refused?.GetType().Name}: {refused?.Message}");
        StringAssert.Contains(refused!.Message, "already inside a native call");
        Assert.AreEqual(Grunning, afterRefusal, "a refused entry rolls the status back to _Grunning");
        Assert.AreEqual(1, depthAfterRefusal, "and enters no seam: only the test's own entry is open");
        Assert.AreEqual(0, depthAfter, "the seam depth is 0 once the test closes its own entry");
        Assert.AreEqual(runtime_package.GoGSyscallStatus(), during);
        Assert.AreEqual(Grunning, after);
    }

    [TestMethod]
    public void GetCallerPCStaysThrowingByName()
    {
        // The ruling's other half: the intrinsic's contract cannot be met, so it is NOT given a value.
        // internal/runtime/sys's hand-owned companion refuses it as a Go panic naming the intrinsic,
        // where the bodyless partial's stub used to throw NotImplementedException.
        Exception? thrown = OnGoroutine(runtime_package.GoGetCallerPC);

        Assert.IsInstanceOfType(thrown, typeof(PanicException), $"got {thrown?.GetType().Name}: {thrown?.Message}");
        StringAssert.Contains(thrown!.Message, "GetCallerPC");
    }
}

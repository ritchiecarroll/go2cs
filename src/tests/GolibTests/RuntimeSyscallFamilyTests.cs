using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

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

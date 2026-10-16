using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using sync = go.sync_package;

namespace GolibTests;

[TestClass]
public class OnceForeignExceptionTests
{
    // The defect these guards pin closed (exec-wall design OQ-6, ratified 2026-08-22): when the
    // function inside sync.OnceFunc/OnceValue throws a FOREIGN (.NET) exception rather than a Go
    // panic, the converted guard's deferred `p = recover(); if !valid { panic(p) }` sees recover()
    // return nil — there is no Go panic in flight — and its `panic(nil)` both masks the original
    // exception's identity and parks a nil-valued PanicException in the thread's captured-panic
    // slot with the oncefunc stack attached. Every R2 exec-wall residual behind a OnceValue-guarded
    // probe (testenv, os/user) reported as `panic: nil` instead of naming the
    // NotImplementedException underneath — a small honesty defect that poisoned a whole census
    // column's readability.
    //
    // The ratified semantics: a re-panic of the ORIGINAL is Go's own OnceValue contract. During a
    // foreign unwind, the deferred nil re-panic must surface the preserved original instead.

    [TestMethod]
    public void OnceValueForeignExceptionSurvivesWithItsIdentity()
    {
        var probe = sync.OnceValue<int>(() => throw new NotImplementedException("exec-wall-marker"));

        Exception? first = Capture(() => probe());

        Assert.IsNotNull(first, "the foreign exception vanished entirely on first call");
        Assert.IsTrue(
            first is NotImplementedException || first.ToString().Contains("exec-wall-marker"),
            $"first call surfaced '{first.GetType().Name}: {first.Message}' — the original " +
            "NotImplementedException identity was masked (the panic-nil shape this guard pins closed).");
    }

    [TestMethod]
    public void OnceValueForeignExceptionDoesNotPoisonLaterPanics()
    {
        var probe = sync.OnceValue<int>(() => throw new NotImplementedException("poison-marker"));
        Capture(() => probe());

        // A later, UNRELATED Go panic on the same thread must report its own value — not the
        // stale nil parked by the once guard, and not the once guard's foreign exception.
        Exception? later = Capture(() => throw go.builtin.panic("unrelated-panic"));

        Assert.IsNotNull(later);
        Assert.IsTrue(later.Message.Contains("unrelated-panic"),
            $"a later panic reported '{later.Message}' — state leaked from the once guard's unwind.");
    }

    // --- the REPLAY half (JOB-024, 2026-08-26) -------------------------------------------------
    //
    // The frame-level correction above fixed the FIRST unwind: the original exception leaves
    // Once.Do with its identity intact. But Go's OnceX contract replays the panic VALUE on every
    // LATER call — `if !valid { panic(p) }` in the returned closure — and during a foreign unwind
    // recover() stored nil into p, so every later caller gets `panic: nil` with no trace of the
    // original. That is exactly what JOB-024 measured killing the os/exec host: 1.23.12's
    // TestConcurrentExec fans callers out on goroutines, ONE gets the preserved original, every
    // other goroutine gets the nil replay, and a goroutine panic is process-fatal. These guards
    // pin the replay: later calls must surface the SAME foreign exception, identity intact.

    [TestMethod]
    public void OnceValueForeignExceptionReplaysWithItsIdentityOnLaterCalls()
    {
        var probe = sync.OnceValue<int>(() => throw new NotImplementedException("replay-marker"));

        Capture(() => probe());                     // first call: consumed by the frame correction
        Exception? second = Capture(() => probe()); // the replay path — oncefunc.cs's `panic(p)`

        Assert.IsNotNull(second, "the replay vanished entirely");
        Assert.IsTrue(
            second is NotImplementedException || second.ToString().Contains("replay-marker"),
            $"the SECOND call surfaced '{second.GetType().Name}: {second.Message}' — the foreign " +
            "exception's identity was masked on replay (the panic-nil shape that killed os/exec's " +
            "TestConcurrentExec goroutines).");
    }

    [TestMethod]
    public void OnceFuncForeignExceptionReplaysWithItsIdentityOnLaterCalls()
    {
        var probe = sync.OnceFunc(() => throw new NotImplementedException("replay-marker-fn"));

        Capture(() => probe());
        Exception? second = Capture(() => probe());

        Assert.IsNotNull(second, "the replay vanished entirely");
        Assert.IsTrue(
            second is NotImplementedException || second.ToString().Contains("replay-marker-fn"),
            $"OnceFunc's SECOND call surfaced '{second.GetType().Name}: {second.Message}' — " +
            "identity masked on replay.");
    }

    [TestMethod]
    public void OnceValuesForeignExceptionReplaysWithItsIdentityOnLaterCalls()
    {
        var probe = sync.OnceValues<int, int>(() => throw new NotImplementedException("replay-marker-2"));

        Capture(() => probe());
        Exception? second = Capture(() => probe());

        Assert.IsNotNull(second, "the replay vanished entirely");
        Assert.IsTrue(
            second is NotImplementedException || second.ToString().Contains("replay-marker-2"),
            $"OnceValues' SECOND call surfaced '{second.GetType().Name}: {second.Message}' — " +
            "identity masked on replay.");
    }

    [TestMethod]
    public void OnceValueGoPanicStillReplaysItsValueOnEveryCall()
    {
        // The CONTROL: Go's own contract — "If f panics, the returned function will panic with
        // the same value on every call" — must hold unchanged for a REAL Go panic. Green before
        // and after the replay fix; a fix that breaks this has overreached.
        var probe = sync.OnceValue<int>(() => throw go.builtin.panic("go-panic-value"));

        Exception? first = Capture(() => probe());
        Exception? second = Capture(() => probe());

        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.IsTrue(first.Message.Contains("go-panic-value"),
            $"first call lost the Go panic value: '{first.Message}'");
        Assert.IsTrue(second.Message.Contains("go-panic-value"),
            $"replay lost the Go panic value: '{second.Message}'");
    }

    // --- the DROP half (Go 1.26 TestOnceXGC/*_panic, 2026-10-16) -------------------------------
    //
    // Go's OnceX bodies clear the wrapped function in the DEFER (`d.f = nil // Do not keep f alive
    // after invoking it`), so f is released after the first call whether it returned or panicked.
    // The hand-own cleared it only after f() RETURNED, so a panicking f stayed reachable for the
    // life of the returned func. Each guard builds the once and calls it in NoInlining helpers, so
    // no frame of this method can hold the captured object; only the WeakReference sees it.

    [TestMethod]
    public void OnceFuncDropsAPanickingFunctionAfterTheFirstCall() =>
        AssertDroppedAfterFirstCall(BuildOnceFunc(panics: true), "OnceFunc, f panics");

    [TestMethod]
    public void OnceValueDropsAPanickingFunctionAfterTheFirstCall() =>
        AssertDroppedAfterFirstCall(BuildOnceValue(panics: true), "OnceValue, f panics");

    [TestMethod]
    public void OnceValuesDropsAPanickingFunctionAfterTheFirstCall() =>
        AssertDroppedAfterFirstCall(BuildOnceValues(panics: true), "OnceValues, f panics");

    // The CONTROLS: the returning path already dropped f before the fix. Green on both sides; a
    // control that goes red means the harness, not the hand-own, keeps the object alive.
    [TestMethod]
    public void OnceFuncDropsAReturningFunctionAfterTheFirstCall() =>
        AssertDroppedAfterFirstCall(BuildOnceFunc(panics: false), "OnceFunc, f returns");

    [TestMethod]
    public void OnceValueDropsAReturningFunctionAfterTheFirstCall() =>
        AssertDroppedAfterFirstCall(BuildOnceValue(panics: false), "OnceValue, f returns");

    private sealed class Captured { }

    // OnceFunc's own Action is returned as is: there is no adapter to share a closure with.

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (Action call, WeakReference captured) BuildOnceFunc(bool panics)
    {
        var captured = new Captured();
        var probe = sync.OnceFunc(() => { GC.KeepAlive(captured); if (panics) throw go.builtin.panic("once-drop"); });
        return (probe, new WeakReference(captured));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (Action call, WeakReference captured) BuildOnceValue(bool panics)
    {
        var captured = new Captured();
        var probe = sync.OnceValue<int>(() => { GC.KeepAlive(captured); if (panics) throw go.builtin.panic("once-drop"); return 1; });
        return (Adapt(probe), new WeakReference(captured));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (Action call, WeakReference captured) BuildOnceValues(bool panics)
    {
        var captured = new Captured();
        var probe = sync.OnceValues<int, int>(() => { GC.KeepAlive(captured); if (panics) throw go.builtin.panic("once-drop"); return (1, 2); });
        return (Adapt(probe), new WeakReference(captured));
    }

    // The adapters are their OWN methods on purpose. A lambda written beside the one that captures
    // `captured` would share its closure object, and would keep `captured` alive by itself.
    private static Action Adapt<T>(Func<T> f) => () => f();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void CallOnce(Action call) => Capture(call);

    private static void AssertDroppedAfterFirstCall((Action call, WeakReference captured) once, string what)
    {
        Collect();
        Assert.IsTrue(once.captured.IsAlive, $"{what}: the captured object died BEFORE the first call (the once must still hold f)");

        CallOnce(once.call);
        Collect();
        Assert.IsFalse(once.captured.IsAlive,
            $"{what}: the wrapped function is still reachable after the first call. Go clears it in the defer, on " +
            "both the returning and the panicking path (`d.f = nil // Do not keep f alive after invoking it`).");
        GC.KeepAlive(once.call);
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

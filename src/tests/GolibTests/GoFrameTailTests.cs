using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// The frame's TAIL, the catch that adopts a panic and parks it where recover() reads it, in its two emitted
/// forms: today's static pair, <c>catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) {
/// GoFrame.Capture(ᒐp); }</c>, and the frame-local pair the converter emits now, <c>catch (Exception ᒐe) when
/// (ᒐ.Is(ᒐe)) { ᒐ.Capture(); }</c>, where the filter stores the panic in the frame and the catch body parks it.
/// Every scenario runs under both and must give Go's answer under both.
/// </summary>
/// <remarks>
/// The ordering both forms keep is the load-bearing part: the filter runs in the FIRST pass of exception handling,
/// before any finally between the throw and the frame, and parking the panic is what that finally then observes.
/// A MERGED tail that parks in the filter (<see cref="MergedTail"/>) passes every plain scenario and loses a panic in
/// the nested one, so it is kept here as the negative control that shows the nested scenario can tell them apart
/// (docs/phase4/SCOUT-defer-emission-parity.md, 10.4).
/// </remarks>
[TestClass]
public class GoFrameTailTests
{
    // --- the nested scenario: P1 caught by an OUTER frame, raised under a plain try/finally whose finally calls a
    // function that panics P2 and recovers it. Go recovers P1 in the outer frame and loses nothing.

    [ThreadStatic] private static object? t_outerRecovered;

    private static readonly Action s_outerDeferred = static () => t_outerRecovered = recover();

    private static readonly Action s_innerDeferred = static () => recover();

    // B defers a recovering call and panics P2, which B's own frame recovers; one per tail, so each arm is a
    // program emitted entirely in that tail.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PanicsAndRecoversItsOwnToday() { GoFrame ᒐ = default; try {
        defer(s_innerDeferred, ref ᒐ, recovers: true);
        throw panic((@string)"P2");
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PanicsAndRecoversItsOwnFrameLocal() { GoFrame ᒐ = default; try {
        defer(s_innerDeferred, ref ᒐ, recovers: true);
        throw panic((@string)"P2");
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }

    // A has no frame; its finally calls B, as golib's own finally blocks call deferred functions.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PanicsUnderAFinally(Action inner)
    {
        try { throw panic((@string)"P1"); }
        finally { inner(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NestedToday() { GoFrame ᒐ = default; try {
        defer(s_outerDeferred, ref ᒐ, recovers: true);
        PanicsUnderAFinally(PanicsAndRecoversItsOwnToday);
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NestedFrameLocal() { GoFrame ᒐ = default; try {
        defer(s_outerDeferred, ref ᒐ, recovers: true);
        PanicsUnderAFinally(PanicsAndRecoversItsOwnFrameLocal);
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }

    // The rejected shape: the filter adopts AND parks the panic, and the catch body is empty.
    private static bool MergedTail(Exception e)
    {
        if (!GoFrame.IsPanic(e, out PanicException? p))
            return false;

        GoFrame.Capture(p);
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NestedMerged() { GoFrame ᒐ = default; try {
        defer(s_outerDeferred, ref ᒐ, recovers: true);
        PanicsUnderAFinally(PanicsAndRecoversItsOwnToday);
    } catch (Exception ᒐe) when (MergedTail(ᒐe)) { } finally { ᒐ.Run(); } }

    private static (object? recovered, Exception? escaped) RunNested(Action frame)
    {
        t_outerRecovered = "unset";
        Exception? escaped = null;

        try { frame(); }
        catch (Exception ex) { escaped = ex; GoFuncRoot.CapturedPanicValue = null; }

        return (t_outerRecovered, escaped);
    }

    [TestMethod]
    public void TheNestedPanicIsRecoveredByTheOuterFrameUnderBothTails()
    {
        foreach ((string tail, Action frame) in new (string, Action)[] { ("today", NestedToday), ("frame-local", NestedFrameLocal) })
        {
            (object? recovered, Exception? escaped) = RunNested(frame);

            Assert.IsNull(escaped, $"{tail}: nothing escapes the outer frame, which recovered");
            Assert.AreEqual("P1", recovered?.ToString(), $"{tail}: the outer frame recovers P1, the panic it caught");
        }
    }

    [TestMethod]
    public void TheMergedTailLosesTheNestedPanicTheNegativeControl()
    {
        // Proves the scenario above can tell the tails apart: the merged filter parks P1 in the first pass, the
        // intervening finally's frame parks and recovers P2 over it, and the outer frame then reads nil.
        (object? recovered, Exception? escaped) = RunNested(NestedMerged);

        Assert.IsNull(escaped, "the merged tail loses the panic silently: nothing escapes");
        Assert.IsNull(recovered, "the merged tail's outer frame recovers nil, where Go recovers P1");
    }

    // --- a foreign exception passes through the frame unchanged, after the deferred calls ran, and recover() in
    // them reads nil (a .NET exception is not a Go panic).

    private sealed class Foreign : Exception;

    [ThreadStatic] private static object? t_foreignRecovered;

    private static readonly Action s_recordRecover = static () => t_foreignRecovered = recover();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignToday(Exception thrown) { GoFrame ᒐ = default; try {
        defer(s_recordRecover, ref ᒐ, recovers: true);
        throw thrown;
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignFrameLocal(Exception thrown) { GoFrame ᒐ = default; try {
        defer(s_recordRecover, ref ᒐ, recovers: true);
        throw thrown;
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }

    [TestMethod]
    public void AForeignExceptionPassesThroughUnchangedUnderBothTails()
    {
        foreach ((string tail, Action<Exception> frame) in new (string, Action<Exception>)[] { ("today", ForeignToday), ("frame-local", ForeignFrameLocal) })
        {
            Foreign thrown = new();
            t_foreignRecovered = "unset";

            Exception? escaped = null;

            try { frame(thrown); }
            catch (Exception ex) { escaped = ex; }

            GoFuncRoot.InFlightForeignException = null;

            Assert.AreSame(thrown, escaped, $"{tail}: the same exception instance leaves the frame");
            Assert.IsNull(t_foreignRecovered, $"{tail}: the deferred call ran, and recover() read nil");
        }
    }

    // --- Goexit fails both filters, so it unwinds through the frame while the finally runs the deferred calls.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GoexitToday(Action deferred) { GoFrame ᒐ = default; try {
        defer(deferred, ref ᒐ);
        throw new GoexitException();
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GoexitFrameLocal(Action deferred) { GoFrame ᒐ = default; try {
        defer(deferred, ref ᒐ);
        throw new GoexitException();
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }

    [TestMethod]
    public void GoexitUnwindsThroughTheFrameRunningTheDeferredCallsUnderBothTails()
    {
        foreach ((string tail, Action<Action> frame) in new (string, Action<Action>)[] { ("today", GoexitToday), ("frame-local", GoexitFrameLocal) })
        {
            bool ran = false;
            Exception? escaped = null;

            // Its own thread: a Goexit marks its thread's goroutine as exiting.
            Thread thread = new(() =>
            {
                try { frame(() => ran = true); }
                catch (Exception ex) { escaped = ex; }
            });

            thread.Start();
            thread.Join();

            Assert.IsInstanceOfType(escaped, typeof(GoexitException), $"{tail}: the Goexit leaves the frame");
            Assert.IsTrue(ran, $"{tail}: the frame's deferred call ran");
        }
    }

    // --- a deferred call that panics while the frame runs P1's deferred calls: Go replaces P1 with P2, and an
    // earlier-deferred recover() returns P2.

    [ThreadStatic] private static object? t_rePanicRecovered;

    private static readonly Action s_recoverLatest = static () => t_rePanicRecovered = recover();

    private static readonly Action s_panicP2 = static () => throw panic((@string)"P2");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RePanicToday() { GoFrame ᒐ = default; try {
        defer(s_recoverLatest, ref ᒐ, recovers: true);
        defer(s_panicP2, ref ᒐ);
        throw panic((@string)"P1");
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RePanicFrameLocal() { GoFrame ᒐ = default; try {
        defer(s_recoverLatest, ref ᒐ, recovers: true);
        defer(s_panicP2, ref ᒐ);
        throw panic((@string)"P1");
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }

    [TestMethod]
    public void ARePanicInADeferredCallIsWhatTheEarlierDeferRecoversUnderBothTails()
    {
        foreach ((string tail, Action frame) in new (string, Action)[] { ("today", RePanicToday), ("frame-local", RePanicFrameLocal) })
        {
            t_rePanicRecovered = "unset";
            Exception? escaped = null;

            try { frame(); }
            catch (Exception ex) { escaped = ex; GoFuncRoot.CapturedPanicValue = null; }

            Assert.IsNull(escaped, $"{tail}: the earlier-deferred call recovered, so nothing escapes");
            Assert.AreEqual("P2", t_rePanicRecovered?.ToString(), $"{tail}: recover() returns the re-panic, P2");
        }
    }

    // --- recover() in a deferred call on a normal return reads nil: no panic is running.

    [ThreadStatic] private static object? t_normalRecovered;

    private static readonly Action s_recoverOnReturn = static () => t_normalRecovered = recover();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int NormalToday() { GoFrame ᒐ = default; try {
        defer(s_recoverOnReturn, ref ᒐ, recovers: true);
        return 7;
    } catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; } finally { ᒐ.Run(); } }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int NormalFrameLocal() { GoFrame ᒐ = default; try {
        defer(s_recoverOnReturn, ref ᒐ, recovers: true);
        return 7;
    } catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); return default; } finally { ᒐ.Run(); } }

    [TestMethod]
    public void RecoverReadsNilOutsideAPanicUnderBothTails()
    {
        foreach ((string tail, Func<int> frame) in new (string, Func<int>)[] { ("today", NormalToday), ("frame-local", NormalFrameLocal) })
        {
            t_normalRecovered = "unset";

            Assert.AreEqual(7, frame(), $"{tail}: the function returns its own result");
            Assert.IsNull(t_normalRecovered, $"{tail}: recover() on a normal return reads nil");
        }
    }
}

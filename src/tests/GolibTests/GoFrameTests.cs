using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class GoFrameTests
{
    // GoFrame is the ref-struct replacement for the func((defer, recover) => …) execution context:
    // the converted body moves INLINE into try/catch/finally and the frame holds nothing but this
    // call's defer list (DESIGN-closure-emission.md §4). The behaviour it must reproduce —
    // LIFO drain on every exit path, the HandledPanic save/restore that keeps a traceback honest
    // through a deferred sequence, the re-panic origin inheritance, and the re-throw of a panic no
    // defer recovered — is GoFunc.HandleFinally's, and it was adversarially reviewed there.
    //
    // So these tests are written as an A/B wherever the two forms can express the same scenario:
    // the OLD `func(…)` machinery and the NEW frame run the same body and must agree observably.
    // A test that only exercised the new type would pin whatever it happens to do; pinning it
    // against the shipped machinery is what makes "it moved, it did not change" checkable.

    // --- helpers -------------------------------------------------------------------------------

    // Runs `body` inside a frame exactly as the converter emits it, and returns the panic that
    // escaped, or null. The frame is declared here rather than passed in because a ref struct
    // cannot be captured — which is the whole reason the body is emitted inline.
    private static PanicException? RunInFrame(FrameBody body)
    {
        try
        {
            GoFrame frame = default;

            try
            {
                body(ref frame);
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        }
        catch (PanicException escaped)
        {
            return Completed(escaped);
        }

        return null;
    }

    private delegate void FrameBody(ref GoFrame frame);

    // A panic that no defer recovered is re-thrown with the captured-panic slot still set — both
    // forms do that, and in a real program the slot is either re-captured by the next enclosing
    // frame or the process is on its way out. A test that swallows the escape has to stand in for
    // that completion, or the next test on this thread inherits a panic it never raised.
    private static PanicException Completed(PanicException escaped)
    {
        // Cleared directly: recover() outside a deferred sequence stops nothing (Go's direct-call
        // rule, DESIGN-recover-model.md), so it can no longer serve as this cleanup.
        GoFuncRoot.CapturedPanicValue = null;
        return escaped;
    }

    // Runs a nested frame and lets an unrecovered panic keep going, which is what a converted
    // callee does: RunInFrame swallows the escape so a test can inspect it, so a test that needs
    // the panic to reach an OUTER frame re-raises it here.
    private static void RunInNestedFrame(FrameBody body)
    {
        PanicException? escaped = RunInFrame(body);

        if (escaped is not null)
            throw escaped;
    }

    // --- ordering ------------------------------------------------------------------------------

    [TestMethod]
    public void DefersRunLastInFirstOut()
    {
        List<int> frameOrder = [];

        RunInFrame((ref GoFrame frame) =>
        {
            for (int i = 0; i < 3; i++)
            {
                int captured = i;
                frame.Push(() => frameOrder.Add(captured));
            }
        });

        CollectionAssert.AreEqual(new[] { 2, 1, 0 }, frameOrder, "frame drained out of order");
    }

    [TestMethod]
    public void DefersPastTheInlineAritySpillAndStayOrdered()
    {
        // Ten registrations exceed the frame's inline slots, so the overflow list is exercised —
        // including the interleaving of the last inline slot with the first overflowed one, which
        // is where a LIFO drain across two storage kinds gets it wrong if it gets it wrong.
        List<int> order = [];

        RunInFrame((ref GoFrame frame) =>
        {
            for (int i = 0; i < 10; i++)
            {
                int captured = i;
                frame.Push(() => order.Add(captured));
            }
        });

        CollectionAssert.AreEqual(new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }, order);
    }

    [TestMethod]
    public void ANullRegistrationIsIgnored()
    {
        List<int> order = [];

        RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(null);
            frame.Push(() => order.Add(1));
            frame.Push(null);
        });

        CollectionAssert.AreEqual(new[] { 1 }, order);
    }

    // --- exit paths ----------------------------------------------------------------------------

    [TestMethod]
    public void DefersRunOnPanicAndTheUnrecoveredPanicIsRethrown()
    {
        bool frameRan = false;

        PanicException? fromFrame = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => frameRan = true);
            throw panic("boom");
        });

        Assert.IsTrue(frameRan, "the frame's defer did not run on the panic path");
        Assert.IsNotNull(fromFrame, "an unrecovered panic must escape the frame");
        Assert.AreEqual((@string)"boom", fromFrame.State);
    }

    [TestMethod]
    public void RecoverStopsThePanic()
    {
        object? frameRecovered = null;

        PanicException? fromFrame = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => frameRecovered = recover());
            throw panic("boom");
        });

        Assert.IsNull(fromFrame, "a recovered panic must not escape the frame");
        Assert.AreEqual((@string)"boom", frameRecovered);
    }

    [TestMethod]
    public void RecoverReturnsNullWhenNothingPanicked()
    {
        object? recovered = "sentinel";

        RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => recovered = recover());
        });

        Assert.IsNull(recovered);
    }

    [TestMethod]
    public void ARuntimeFaultIsAdoptedAsAPanicAndIsRecoverable()
    {
        // A .NET exception that MAPS to a Go runtime panic must be adopted by the frame's filter
        // exactly as GoFunc.Execute adopts it — otherwise `recover()` cannot see a nil dereference
        // or a divide by zero, which real converted code relies on.
        object? recovered = null;

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => recovered = recover());
            int zero = 0;
            _ = 1 / zero;
        });

        Assert.IsNull(escaped);
        Assert.IsNotNull(recovered);
        StringAssert.Contains(recovered.ToString(), "divide by zero");
    }

    [TestMethod]
    public void ANonPanicExceptionPropagatesAndStillRunsTheDefers()
    {
        bool ran = false;

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            GoFrame frame = default;

            try
            {
                frame.Push(() => ran = true);
                throw new InvalidOperationException("not a panic");
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        });

        Assert.IsTrue(ran, "a finally must drain the defers whatever the exception was");
    }

    [TestMethod]
    public void GoexitUnwindsThroughTheFrameAndTheDefersStillRun()
    {
        // runtime.Goexit is Go's "unwind this goroutine, running its defers". GoexitException is
        // deliberately NOT a panic — it fails the frame's filter — so it propagates while the
        // finally still drains, which IS Go's defers-run-on-Goexit semantics.
        bool ran = false;

        Assert.ThrowsException<GoexitException>(() =>
        {
            GoFrame frame = default;

            try
            {
                frame.Push(() => ran = true);
                throw new GoexitException();
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        });

        Assert.IsTrue(ran, "Goexit must still run the frame's defers");
    }

    // --- panic bookkeeping ---------------------------------------------------------------------

    [TestMethod]
    public void TheHandledPanicIsObservableThroughTheDeferredSequenceAndRestoredAfter()
    {
        // Go keeps a panicking goroutine's frames on the stack until the panic completes, so a
        // traceback taken from a deferred call still shows the panic site. InFlightPanic is what
        // reproduces that, and it must be scoped strictly to the sequence — the save/restore is
        // the part that cannot go stale.
        PanicException? seenInsideBeforeRecover = null;
        PanicException? seenInsideAfterRecover = null;

        RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() =>
            {
                seenInsideBeforeRecover = GoFuncRoot.InFlightPanic;
                _ = recover();
                seenInsideAfterRecover = GoFuncRoot.InFlightPanic;
            });

            throw panic("boom");
        });

        Assert.IsNotNull(seenInsideBeforeRecover, "the handled panic must be visible in the sequence");
        Assert.IsNotNull(seenInsideAfterRecover, "recover() clears the slot but not the traceback view");
        Assert.IsNull(GoFuncRoot.InFlightPanic, "the handled panic outlived its sequence");
    }

    [TestMethod]
    public void ARepanicFromADeferredCallInheritsTheOriginalOrigin()
    {
        // sync.OnceFunc's replay idiom: `defer func(){ panic(recover()) }()`. Go's traceback still
        // shows the ORIGINAL panic's frames, so the new panic adopts the origin rather than
        // starting a fresh, shallower one. Both forms must agree on the frames they report.
        PanicException? fromFrame = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => throw panic(recover()!));
            raiseOriginPanic();
        });

        Assert.IsNotNull(fromFrame);
        StringAssert.Contains(fromFrame.StackTrace, nameof(raiseOriginPanic));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void raiseOriginPanic()
    {
        throw panic("origin");
    }

    [TestMethod]
    public void ANestedFrameRecoversWithoutDisturbingTheOuterOne()
    {
        List<string> log = [];

        PanicException? escaped = RunInFrame((ref GoFrame outer) =>
        {
            outer.Push(() => log.Add("outer defer"));

            RunInNestedFrame((ref GoFrame inner) =>
            {
                inner.Push(() =>
                {
                    log.Add("inner recovered " + recover());
                });

                throw panic("inner boom");
            });

            log.Add("outer continued");
        });

        Assert.IsNull(escaped, "the inner recover must not let the panic reach the outer frame");
        CollectionAssert.AreEqual(new[] { "inner recovered inner boom", "outer continued", "outer defer" }, log);
    }

    [TestMethod]
    public void AnUnrecoveredNestedPanicRunsBothFramesDefersThenEscapes()
    {
        List<string> log = [];

        PanicException? escaped = RunInFrame((ref GoFrame outer) =>
        {
            outer.Push(() => log.Add("outer defer"));

            RunInNestedFrame((ref GoFrame inner) =>
            {
                inner.Push(() => log.Add("inner defer"));
                throw panic("boom");
            });

            log.Add("unreachable");
        });

        Assert.IsNotNull(escaped);
        CollectionAssert.AreEqual(new[] { "inner defer", "outer defer" }, log);
    }

    [TestMethod]
    public void AFrameCalledFromADeferredCallDoesNotReRaiseTheOuterPanic()
    {
        // The re-raise belongs to the frame that CAUGHT the panic, not to the thread. A callee
        // invoked from a deferred call caught nothing — it exits through the same finally, but the
        // panic parked there is somebody else's, and re-raising it aborts the caller's cleanup at
        // whatever statement happens to follow the call.
        //
        // Measured on database/sql's Conn.Raw: the deferred release reaches Conn.close through
        // withLock, a helper holding one defer and panicking nothing, whose Run threw the parked
        // panic on the way out — so `c.dc = nil` never ran, the connection stayed open, and
        // TestConnRaw blocked on a poll that could never succeed for the package's whole deadline.
        List<string> log = [];

        PanicException? escaped = RunInFrame((ref GoFrame outer) =>
        {
            outer.Push(() =>
            {
                log.Add("cleanup begin");
                RunInNestedFrame((ref GoFrame callee) => callee.Push(() => log.Add("callee defer")));
                log.Add("cleanup end");
            });

            throw panic("boom");
        });

        CollectionAssert.AreEqual(new[] { "cleanup begin", "callee defer", "cleanup end" }, log,
            "a callee's frame re-raised the caller's panic and truncated the deferred cleanup");
        Assert.IsNotNull(escaped, "the panic must still escape once the OWNING frame's defers have run");
        Assert.AreEqual((@string)"boom", escaped.State);
    }

    [TestMethod]
    public void AFrameWithNoDefersCalledDuringAPanicDoesNotReRaiseItEither()
    {
        // Same rule, the m_count == 0 path: a frame whose defers were all conditional still runs
        // Run() from its finally, and it must be as silent as one that registered none.
        List<string> log = [];

        RunInFrame((ref GoFrame outer) =>
        {
            outer.Push(() =>
            {
                RunInNestedFrame((ref GoFrame callee) => log.Add("callee body"));
                log.Add("cleanup end");
            });

            throw panic("boom");
        });

        CollectionAssert.AreEqual(new[] { "callee body", "cleanup end" }, log);
    }

    [TestMethod]
    public void APanicRaisedByADeferredCallStillEscapesAFrameThatCaughtNothing()
    {
        // The negative that keeps the ownership rule from becoming a swallow: this frame caught no
        // panic, so it claims none — but a panic raised by its OWN deferred call is still its to
        // continue, and must reach the caller.
        List<string> log = [];

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() =>
            {
                log.Add("defer");
                throw panic("from defer");
            });

            log.Add("body");
        });

        Assert.IsNotNull(escaped, "a panic raised by a deferred call must escape its own frame");
        Assert.AreEqual((@string)"from defer", escaped.State);
        CollectionAssert.AreEqual(new[] { "body", "defer" }, log);
    }

    [TestMethod]
    public void APanicRaisedInsideADeferredCleanupReplacesTheOneUnwinding()
    {
        // Both rules at once, and the shape that would break if ownership were decided by depth
        // rather than by who caught what: an outer panic is unwinding, its cleanup calls a callee,
        // and THAT callee's deferred call panics. Go lets the newer panic replace the one in
        // flight, so it is what a recover() upstream answers — and the statement after the callee
        // is correctly skipped, because this time a panic really did pass through.
        List<string> log = [];

        PanicException? escaped = RunInFrame((ref GoFrame outer) =>
        {
            outer.Push(() =>
            {
                log.Add("cleanup begin");

                RunInNestedFrame((ref GoFrame callee) =>
                {
                    callee.Push(() => throw panic("callee boom"));
                    log.Add("callee body");
                });

                log.Add("unreachable");
            });

            throw panic("outer boom");
        });

        Assert.IsNotNull(escaped);
        Assert.AreEqual((@string)"callee boom", escaped.State);
        CollectionAssert.AreEqual(new[] { "cleanup begin", "callee body" }, log);
    }

    // --- allocation ----------------------------------------------------------------------------

    [TestMethod]
    public void TheFrameItselfAllocatesNothing()
    {
        // The point of the design. A frame that registers a defer allocates only what the DEFERRED
        // CLOSURE costs; the execution context, its body delegate and its handler delegates are
        // gone, and the defer list is inline storage rather than a Stack<Action>.
        //
        // Measured against the shipped GoFunc form running the identical scenario, which is the
        // control that makes the number mean something.
        long frameBytes = Allocated(static () =>
        {
            GoFrame frame = default;

            try
            {
                defer(Nothing, ref frame);
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        });

        // ZERO, not merely less. The execution context this replaced measured 248 B for the same
        // scenario (a GoFunc object, a body display class, a body delegate, two handler delegates
        // and a Stack<Action>); the frame is inline storage and a cached static method group, so
        // there is nothing left to charge. A capturing deferred closure still pays for ITS closure,
        // which is what open-coding (DESIGN-closure-emission.md §4.5) would address.
        Assert.AreEqual(0, frameBytes,
            $"the frame machinery itself must be free; measured {frameBytes} B for one static-method defer");
    }

    [TestMethod]
    public void TheAllocationProbeItselfDetectsAnAllocation()
    {
        // The neutered control for the assertion above: the same instrument, pointed at a body
        // that provably allocates, must report it. Without this a broken probe reads as a win.
        //
        // The array is handed to AllocationProbe.Escape rather than discarded, and that is
        // load-bearing at the configuration validation runs under: Release with tiering OFF is
        // fully optimized from the first call, and .NET's escape analysis stack-allocates a
        // discarded `new object[4]` outright — 0 B/run measured at TC0 against 56 B/run under
        // default tiering. A control that measures zero because the allocation was elided reads as
        // "the probe is blind", and every want-zero assertion resting on this probe would pass
        // vacuously beside it. See AllocationProbe for the measurement and why an escape rather
        // than a skip or a tiering pin.
        long bytes = Allocated(static () => AllocationProbe.Escape(new object[4]));
        Assert.IsTrue(bytes > 0, "the allocation probe reported zero for a body that allocates");
    }

    // A do-nothing STATIC target: a static method group converts to a delegate that C# caches, so
    // the measurement is of the defer machinery rather than of the callee's delegate.
    private static void Nothing()
    {
    }

    private static long Allocated(Action body)
    {
        // Warm the JIT and any first-use statics so the measurement is of steady state, then
        // average over enough iterations that a single-call rounding cannot dominate.
        for (int i = 0; i < 200; i++)
            body();

        const int Runs = 1000;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Runs; i++)
            body();

        return (GC.GetAllocatedBytesForCurrentThread() - before) / Runs;
    }
}

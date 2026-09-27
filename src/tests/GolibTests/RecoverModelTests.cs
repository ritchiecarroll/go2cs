using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Go's recover() rules, driven through GoFrame in the exact shape the converter emits
/// (docs/phase4/DESIGN-recover-model.md). Each arm mirrors a runtime defer_test.go case:
/// <list type="bullet">
/// <item>DIRECT eligibility — only a deferred call the panic sequence itself runs can recover; a
/// recover in the defers of a deferred function's OWN normal return reads nil (TestRecoverMatching).</item>
/// <item>The panic CHAIN — a nested panic recovered inside a deferred call leaves the outer panic in
/// flight and recoverable (TestIssue43920, TestIssue43921), and a panic replaced within the SAME
/// sequence is aborted (TestAbortedPanic, the arm that was already green and must stay so).</item>
/// </list>
/// </summary>
[TestClass]
public class RecoverModelTests
{
    private delegate void FrameBody(ref GoFrame frame);

    // One emitted frame; returns the panic that escaped it, or null. The escaped panic's captured
    // slot is cleared here, standing in for the enclosing frame that would re-capture it.
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
            GoFuncRoot.CapturedPanicValue = null;
            return escaped;
        }

        return null;
    }

    // A nested converted callee: an unrecovered panic keeps going to the caller.
    private static void Call(FrameBody body)
    {
        PanicException? escaped = RunInFrame(body);

        if (escaped is not null)
            throw escaped;
    }

    [TestCleanup]
    public void NoPanicOutlivesAnArm()
    {
        Assert.IsNull(GoFuncRoot.InFlightPanic, "a panic outlived its arm");
        Assert.IsNull(recover(), "a recoverable panic outlived its sequence");
    }

    [TestMethod]
    public void ARecoverInADeferredFunctionsOwnDefersReadsNil()
    {
        // TestRecoverMatching
        object? outer = "unset", nested = "unset";

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => outer = recover());
            frame.Push(() => Call((ref GoFrame d2) =>
            {
                d2.Push(() => nested = recover());
            }));

            throw panic("panic1");
        });

        Assert.IsNull(nested, "a recover in the defers of a deferred function's normal return must read nil");
        Assert.AreEqual((@string)"panic1", outer, "the outer deferred call recovers the panic");
        Assert.IsNull(escaped);
    }

    [TestMethod]
    public void ANestedPanicRecoveredInADeferredCallLeavesTheOuterPanicInFlight()
    {
        // TestIssue43921
        object? r1 = "unset", r4 = "unset";

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => r1 = recover());

            Call((ref GoFrame inner) =>
            {
                inner.Push(() => { });
                inner.Push(() => Call((ref GoFrame dx) =>
                {
                    dx.Push(() => r4 = recover());
                    throw panic(4);
                }));

                throw panic(1);
            });
        });

        Assert.AreEqual(4, r4);
        Assert.AreEqual(1, r1, "the outer panic must survive the nested one's recovery");
        Assert.IsNull(escaped);
    }

    [TestMethod]
    public void TheIssue43920ChainRecoversFiveThreeAndOne()
    {
        // TestIssue43920
        object? r1 = "unset", r3 = "unset", r5 = "unset";
        int steps = 0;

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => r1 = recover());
            frame.Push(() => Call((ref GoFrame d2) =>
            {
                d2.Push(() => Call((ref GoFrame d2a) =>
                {
                    d2a.Push(() => r5 = recover());
                    d2a.Push(() => throw panic(5));
                    Call((ref GoFrame _) => throw panic(4));
                }));
                d2.Push(() => r3 = recover());
                d2.Push(() => throw panic(3));
            }));

            Call((ref GoFrame f) =>
            {
                f.Push(() => steps++);
                throw panic(1);
            });
        });

        Assert.AreEqual(1, steps);
        Assert.AreEqual(3, r3);
        Assert.AreEqual(5, r5);
        Assert.AreEqual(1, r1, "panic 1 must still be recoverable after 3 and 5 were recovered");
        Assert.IsNull(escaped);
    }

    [TestMethod]
    public void APanicReplacedInTheSameSequenceIsAborted()
    {
        // TestAbortedPanic -- green before the change; guards the abort half of the chain rule.
        object? first = "unset", second = "unset";

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => first = recover());
            frame.Push(() => second = recover());
            frame.Push(() => throw panic("panic2"));

            throw panic("panic1");
        });

        Assert.AreEqual((@string)"panic2", second);
        Assert.IsNull(first, "panic1 was aborted when panic2 replaced it and was recovered");
        Assert.IsNull(escaped);
    }

    [TestMethod]
    public void AnUnrecoveredOuterPanicStillEscapesPastARecoveredNestedOne()
    {
        // The swallow shape underneath TestIssue43921, with no outer recover: the outer panic must
        // leave the frame instead of the frame returning normally.
        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() => Call((ref GoFrame d) =>
            {
                d.Push(() => _ = recover());
                throw panic("inner");
            }));

            throw panic("outer");
        });

        Assert.IsNotNull(escaped, "the outer panic was swallowed");
        Assert.AreEqual((@string)"outer", escaped.State);
    }

    [TestMethod]
    public void RecoverOutsideAnyDeferredSequenceReadsNil()
    {
        Assert.IsNull(recover());
    }
}

using System;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The contract of the native-call transition seam, <see cref="GoNativeCall"/>
/// (docs/phase4/DESIGN-native-call-seam.md §4 and §12.3).
/// </summary>
/// <remarks>
/// <para>
/// The seam counts a goroutine's transitions in one per-goroutine number, even while it runs managed code and odd
/// while it is inside a native call. <c>Enter</c> and <c>ReenterManaged</c> add one, <c>Exit</c> and
/// <c>ReturnToNative</c> subtract one, and each refuses the parity it does not expect. That parity check is what
/// enforces "exactly once per transition" (COORD's Q2 ruling): a native call entered twice, by a funnel and by the
/// runtime family both, fails by name instead of double-counting.
/// </para>
/// <para>
/// The number lives on the goroutine object, never in a thread-static (G's §4.1 rule). A thread with no goroutine
/// identity is inert: a runtime-internal thread, a foreign thread before it attaches.
/// </para>
/// <para>
/// Every arm runs on a thread it creates, because the test host's own thread may already be the main goroutine,
/// and a fresh thread is the only one whose identity the arm controls.
/// </para>
/// </remarks>
[TestClass]
public class GoNativeCallTests
{
    private static void OnFreshThread(Action body, bool withIdentity = true)
    {
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                if (withIdentity)
                {
                    using (Goroutine.Enter())
                        body();
                }
                else
                {
                    body();
                }
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        if (failure is not null)
            throw new AssertFailedException($"the arm failed on its thread: {failure}", failure);
    }

    private static PanicException Refused(Action action)
    {
        try
        {
            action();
        }
        catch (PanicException panic)
        {
            return panic;
        }

        throw new AssertFailedException("the out-of-order transition was not refused");
    }

    [TestMethod]
    public void AThreadWithNoGoroutineIdentityIsInert()
    {
        OnFreshThread(() =>
        {
            Assert.IsNull(Goroutine.Current, "premise: a thread golib never adopted has no goroutine");

            // Inert means no state and no refusal, even for a sequence that would be out of order on a goroutine.
            GoNativeCall.Exit();
            GoNativeCall.Enter();
            GoNativeCall.Enter();
            GoNativeCall.ReturnToNative();

            Assert.AreEqual(-1, GoNativeCall.TransitionDepth);
        }, withIdentity: false);
    }

    [TestMethod]
    public void AnEnterAndItsExitBalance()
    {
        OnFreshThread(() =>
        {
            Assert.AreEqual(0, GoNativeCall.TransitionDepth);

            GoNativeCall.Enter();
            Assert.AreEqual(1, GoNativeCall.TransitionDepth);

            GoNativeCall.Exit();
            Assert.AreEqual(0, GoNativeCall.TransitionDepth);

            GoNativeCall.Enter(NativeCallHint.Blocks);
            GoNativeCall.Exit();
            Assert.AreEqual(0, GoNativeCall.TransitionDepth);
        });
    }

    [TestMethod]
    public void ASecondEnterForOneTransitionIsRefusedByName()
    {
        OnFreshThread(() =>
        {
            GoNativeCall.Enter();

            PanicException refusal = Refused(() => GoNativeCall.Enter());
            StringAssert.Contains(refusal.Message, "GoNativeCall.Enter");
            StringAssert.Contains(refusal.Message, "already inside a native call");

            // The refusal changed nothing: the one open transition still closes.
            Assert.AreEqual(1, GoNativeCall.TransitionDepth);
            GoNativeCall.Exit();
        });
    }

    [TestMethod]
    public void AnExitOutsideANativeCallIsRefusedByName()
    {
        OnFreshThread(() =>
        {
            PanicException refusal = Refused(GoNativeCall.Exit);
            StringAssert.Contains(refusal.Message, "GoNativeCall.Exit");
            StringAssert.Contains(refusal.Message, "not inside a native call");
            Assert.AreEqual(0, GoNativeCall.TransitionDepth);
        });
    }

    [TestMethod]
    public void ACallbackNestsAndAFunnelInsideItBracketsAgain()
    {
        OnFreshThread(() =>
        {
            int[] depths = new int[6];

            GoNativeCall.Enter();                   // a funnel enters native
            depths[0] = GoNativeCall.TransitionDepth;
            GoNativeCall.ReenterManaged();          // native calls back into Go
            depths[1] = GoNativeCall.TransitionDepth;
            GoNativeCall.Enter();                   // the callback makes a native call of its own
            depths[2] = GoNativeCall.TransitionDepth;
            GoNativeCall.Exit();
            depths[3] = GoNativeCall.TransitionDepth;
            GoNativeCall.ReturnToNative();          // the callback returns to native
            depths[4] = GoNativeCall.TransitionDepth;
            GoNativeCall.Exit();                    // the outer native call returns
            depths[5] = GoNativeCall.TransitionDepth;

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 2, 1, 0 }, depths);
        });
    }

    [TestMethod]
    public void AReturnToNativeWithoutAReentryIsRefusedByName()
    {
        OnFreshThread(() =>
        {
            PanicException outside = Refused(GoNativeCall.ReturnToNative);
            StringAssert.Contains(outside.Message, "GoNativeCall.ReturnToNative");

            GoNativeCall.Enter();

            PanicException inside = Refused(GoNativeCall.ReturnToNative);
            StringAssert.Contains(inside.Message, "GoNativeCall.ReturnToNative");
            Assert.AreEqual(1, GoNativeCall.TransitionDepth);

            GoNativeCall.Exit();
        });
    }

    [TestMethod]
    public void AReentryOutsideANativeCallIsRefusedByName()
    {
        OnFreshThread(() =>
        {
            PanicException refusal = Refused(GoNativeCall.ReenterManaged);
            StringAssert.Contains(refusal.Message, "GoNativeCall.ReenterManaged");
            Assert.AreEqual(0, GoNativeCall.TransitionDepth);
        });
    }

    [TestMethod]
    public void TheDepthIsTheGoroutinesNotTheThreads()
    {
        // Two goroutines adopted in turn by ONE thread: the second must not inherit the first's open transition.
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                using (Goroutine.Enter())
                {
                    GoNativeCall.Enter();
                    Assert.AreEqual(1, GoNativeCall.TransitionDepth);
                    GoNativeCall.Exit();
                    GoNativeCall.Enter();
                }

                Assert.AreEqual(-1, GoNativeCall.TransitionDepth, "a released identity leaves the thread inert");

                using (Goroutine.Enter())
                    Assert.AreEqual(0, GoNativeCall.TransitionDepth, "a new goroutine starts outside native code");
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        if (failure is not null)
            throw new AssertFailedException($"the arm failed on its thread: {failure}", failure);
    }
}

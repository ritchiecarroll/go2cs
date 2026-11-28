using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class ConcurrentSubtestRunTests
{
    // WHY THIS EXISTS.
    //
    // Go's testing places NO goroutine restriction on t.Run. It restricts FailNow and Fatal to the
    // test's own goroutine -- those unwind the frame -- and this host keeps that. But Run is callable
    // from anywhere, and Go pins it with TWO of its own tests, both regression tests for
    // go.dev/issue/64402:
    //
    //   TestConcurrentRun  starts two goroutines that each call t.Run on the SAME parent
    //   TestParentRun      calls t1.Run from inside t2's body (t1.Run, not t2.Run)
    //
    // This host refused both until 2026-09-04, via an owner-thread check on Run, and the two
    // refusals failed DIFFERENTLY -- which is why only one of them was visible. TestParentRun merely
    // reported an infrastructure error and silently lost its inner subtest. TestConcurrentRun
    // DEADLOCKED: a refused Run never runs the body, so the body's ready.Done() never happens, the
    // parent's ready.Wait() blocks forever, and the package deadline kills the entire run. That is
    // what made `testing`'s own row unmeasurable -- one defect, 49 verdicts unreachable behind it.
    //
    // The host has no _test.go and no behavioral test can reach it, so this takes the route
    // TestExecutionOutputCapTests documents: an MSTest guard binding the converted package directly.
    //
    // NEGATIVE CONTROL, measured 2026-09-04: with the owner check restored on Run, ConcurrentRun
    // fails on `both subtest bodies must have run` (the bodies never execute) rather than hanging --
    // the latch's timeout is what converts the production deadlock into a reportable failure here, so
    // the guard states the defect instead of reproducing it as a hung suite. ParentRun fails on
    // `the inner subtest body must have run`.

    private static TestRunner NewRunner() =>
        new(new TestRegistry("guard", []), new TestOptions(),
            new TestReporter("guard", json: false), ".", ".");

    // Long enough that a healthy run never reaches it, short enough that a REGRESSION reports a
    // failure instead of hanging the suite. A deadlock is the failure mode being guarded against, so
    // the guard must not be able to reproduce it.
    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(30.0D);

    [TestMethod]
    public void ConcurrentRunOnOneParentRunsBothBodiesAndTheParentWaitsForBoth()
    {
        TestExecution parent = new(NewRunner(), "TestConcurrentRunGuard", null, "guard.go", 1);

        int bodiesRan = 0;
        using CountdownEvent ready = new(2);
        using ManualResetEventSlim block = new(false);
        int completedBeforeParentReturned = 0;

        // Go's TestConcurrentRun shape: two goroutines, one parent, each body signalling that it
        // started and then parking until both have. If Run refuses -- or serialises -- the second
        // body never starts and `ready` never counts down.
        Task[] callers = new Task[2];

        for (int i = 0; i < 2; i++)
        {
            callers[i] = Task.Run(() => parent.Run("", _ =>
            {
                Interlocked.Increment(ref bodiesRan);
                ready.Signal();
                block.Wait(Budget);
                Interlocked.Increment(ref completedBeforeParentReturned);
            }));
        }

        bool bothStarted = ready.Wait(Budget);
        block.Set();

        bool callersReturned = Task.WaitAll(callers, Budget);

        Assert.IsTrue(bothStarted,
            "both subtest bodies must have run: Go permits concurrent t.Run on one parent (go.dev/issue/64402)");
        Assert.IsTrue(callersReturned, "both t.Run calls must return");
        Assert.AreEqual(2, Volatile.Read(ref bodiesRan), "exactly two subtest bodies must have run");

        // The parent waited for both: each Run returns only once its child completed, so by the time
        // both callers have returned both bodies must have finished. This is the half a refusal
        // would satisfy vacuously (no body, nothing to wait for), which is why it is asserted
        // BESIDE the count rather than instead of it.
        Assert.AreEqual(2, Volatile.Read(ref completedBeforeParentReturned),
            "the parent must have waited for both children before its Run calls returned");
    }

    [TestMethod]
    public void RunOnTheParentFromInsideAChildRunsTheInnerBody()
    {
        // Go's TestParentRun shape: the inner call is on the PARENT, made from the child's
        // goroutine. It never deadlocked -- it silently lost the inner subtest -- so it is the arm
        // that would have gone on failing quietly after a fix aimed only at the deadlock.
        TestExecution parent = new(NewRunner(), "TestParentRunGuard", null, "guard.go", 1);

        int innerRan = 0;

        parent.Run("outer", _ =>
        {
            parent.Run("not_inner", _ => Interlocked.Increment(ref innerRan));
        });

        Assert.AreEqual(1, Volatile.Read(ref innerRan),
            "the inner subtest body must have run: t1.Run from inside t2's body is what Go's TestParentRun does");
    }
}

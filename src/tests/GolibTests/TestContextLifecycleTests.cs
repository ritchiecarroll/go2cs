using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TestContextLifecycleTests
{
    // WHY THIS EXISTS.
    //
    // Go 1.24 added TB.Context, and the first implementation of it here had TWO defects, both from
    // reasoning about the member instead of reading it:
    //
    //   1. It called TryEnsureOwner, copied from the neighbouring Setenv. Go's Context is
    //      `checkFuzzFn; return c.ctx` -- NO goroutine restriction of any kind -- and calling
    //      t.Context() from a spawned goroutine is most of what a test context is FOR. TryEnsureOwner
    //      does not merely refuse: it sets InfrastructureFailed and FAILS the test, so an ordinary
    //      Go pattern became an infrastructure failure.
    //   2. Its refusal path returned context.Background() -- a DIFFERENT context, one that never
    //      cancels -- so anything past the guard got a Done() that never fires. That is the quieter
    //      half and the half that would have been diagnosed as something else.
    //
    // Neither was reachable by any standing gate: nothing at go1.23.12 calls Context, so the member
    // is inert to the corpus and would have arrived live at the hop. These arms are what stand in
    // for the coverage the hop's own `testing` row will eventually provide.

    private static TestExecution NewExecution(string name)
    {
        TestReporter reporter = new("guard", json: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return new TestExecution(runner, name, null, "guard.go", 1);
    }

    // Go: "Successive calls return the same context." A per-call context would give each caller its
    // own cancellation and silently break any code that stores one and selects on it elsewhere.
    [TestMethod]
    public void ContextIsStableAcrossCalls()
    {
        TestExecution execution = NewExecution("TestContextStable");

        go.context_package.Context first = execution.Context();
        go.context_package.Context second = execution.Context();

        Assert.AreSame(first, second, "successive Context() calls must return the SAME context");
    }

    // The live context must not already be cancelled -- without this the cancellation arm below is
    // vacuously true, because a context that was never live is trivially "cancelled by cleanup".
    [TestMethod]
    public void ContextIsLiveBeforeTheCleanupPhase()
    {
        TestExecution execution = NewExecution("TestContextLive");

        Assert.IsNull(execution.Context().Err(),
            "PRECONDITION: a running test's context must be live, or the cancellation arm proves nothing");
    }

    // THE ORDER IS THE CONTRACT (testing.go:1429). Context's documented purpose is to let a cleanup
    // wait on resources that shut down on Done(); cancel AFTER the cleanup phase and every such
    // cleanup blocks to its own deadline instead. Asserted from inside a cleanup, which is the only
    // place the ordering is observable.
    [TestMethod]
    public void ContextIsCancelledBeforeCleanupsRun()
    {
        TestExecution parent = NewExecution("TestContextCancelOrder");
        go.error? seenInCleanup = null;
        bool cleanupRan = false;

        parent.Run("child", t =>
        {
            TestExecution child = t.Value.Execution;
            go.context_package.Context ctx = child.Context();

            Assert.IsNull(ctx.Err(), "the context must be live while the test body runs");

            child.Cleanup(() =>
            {
                cleanupRan = true;
                seenInCleanup = ctx.Err();
            });
        });

        Assert.IsTrue(cleanupRan, "the cleanup must have run, or this arm asserts nothing");
        Assert.IsNotNull(seenInCleanup,
            "the context must ALREADY be cancelled when a cleanup observes it -- Go cancels just before the cleanup phase");
    }

    // THE REGRESSION GUARD FOR DEFECT 1. A non-owner goroutine must get the test's own context and
    // must NOT fail the test. Before the fix this marked InfrastructureFailed and returned a
    // different, never-cancelling context.
    [TestMethod]
    public void ContextFromANonOwnerGoroutineIsTheTestsOwnContextAndDoesNotFailTheTest()
    {
        TestExecution execution = NewExecution("TestContextOffThread");

        go.context_package.Context onOwner = execution.Context();
        go.context_package.Context? offOwner = null;

        Task worker = Task.Run(() => offOwner = execution.Context());
        Assert.IsTrue(worker.Wait(TimeSpan.FromSeconds(30)), "the off-thread Context() call must complete");

        Assert.IsNotNull(offOwner, "a goroutine must be able to obtain the test's context");
        Assert.AreSame(onOwner, offOwner,
            "a non-owner goroutine must get the TEST'S context, not a fresh Background() that never cancels");
        Assert.IsFalse(execution.InfrastructureFailed,
            "calling Context() off the test goroutine must not fail the test -- Go imposes no such restriction");
    }
}

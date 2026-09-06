using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class FailAfterCompleteTests
{
    // WHY THIS EXISTS, and why the ORDER is the arm that matters.
    //
    // Go's `common.Fail` propagates to the ancestors BEFORE it checks its own `done`:
    //
    //   func (c *common) Fail() {
    //       if c.parent != nil { c.parent.Fail() }   // FIRST, unconditionally
    //       c.mu.Lock(); defer c.mu.Unlock()
    //       if c.done { panic("Fail in goroutine after " + c.name + " has completed") }
    //       c.failed = true
    //   }
    //
    // Until 2026-09-06 this host checked m_finished FIRST, threw, and never propagated -- so a
    // goroutine failing after its test completed failed NOBODY. In Go the parent test is failed; here
    // it was not. That is a VERDICT-visible difference rather than a reporting one, and it is why the
    // coordinator ruled the order the defect to fix and the kind the easy half.
    //
    // The KIND is the same divergence Log carried: Go panics, so a converted recover() can see it and
    // golib's backstop reports it Go-style; a .NET InvalidOperationException is invisible to recover()
    // and lands in the host's INFRASTRUCTURE bucket, which by TestRunner's own definition means "the
    // host could not run the test".
    //
    // WHAT IS DELIBERATELY NOT GUARDED: FailFromChild has no done check. Go's recursion runs through
    // Fail() so every ancestor could panic; this walk cannot. The branch would be unreachable --
    // runTests makes every top-level test a t.Run on a root T, so mid-run there is always a live
    // ancestor -- and an unexercisable branch is the thing this host least needs. The reason lives at
    // FailFromChild rather than in an arm here, because there is nothing to assert.
    //
    // DISCRIMINATING ARMS: two. ALateFailStillFailsTheLiveParent isolates the ORDER by catching
    // whatever is thrown and asserting only the parent's state -- so it goes red pre-fix on the order
    // assertion BY NAME rather than on the exception type, which is the other arm's business.
    // ALateFailPanicsWithGosText isolates the KIND and the text. The third arm is MUST-NOT-REGRESS: a
    // LIVE test's Fail still marks itself and its parent, and still does not throw. It passes either
    // way, and is here so a fix that made every Fail panic could not read green.

    private static (TestRunner Runner, TestReporter Reporter) NewRunner()
    {
        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return (runner, reporter);
    }

    // A child that RAN AND PASSED, through the host's real subtest path rather than by poking
    // internals. That matters for this guard specifically: RecordGoroutinePanic -- the other way to
    // mark an execution finished -- itself calls FailFromChild, which would leave the parent already
    // failed and make the order assertion below VACUOUSLY true.
    private static TestExecution RunPassingChild(TestExecution parent, string name)
    {
        TestExecution? child = null;

        parent.Run(name, t => child = t.Value.Execution);

        Assert.IsNotNull(child, "the subtest body must have run and exposed its execution");

        return child!;
    }

    // THE ORDER, isolated: catch whatever is thrown -- the kind is the next arm's subject -- and
    // assert only that the ancestor chain was marked failed, which is what Go guarantees and what
    // this host did not do.
    [TestMethod]
    public void ALateFailStillFailsTheLiveParent()
    {
        (TestRunner runner, _) = NewRunner();

        TestExecution parent = new(runner, "TestLateFailParent", null, "guard.go", 1);
        TestExecution child = RunPassingChild(parent, "child");

        Assert.IsFalse(parent.Failed,
            "PRECONDITION: a passing child must leave its parent unfailed, or the assertion below is vacuously true");

        try
        {
            child.Fail();
            Assert.Fail("a Fail arriving after the test completed must not return normally -- Go panics there");
        }
        catch (AssertFailedException)
        {
            throw;
        }
        catch (Exception)
        {
            // Whatever it threw is this arm's business only insofar as it threw. The KIND is asserted
            // by ALateFailPanicsWithGosText; isolating them is what lets this arm go red on the ORDER
            // rather than on the exception type.
        }

        Assert.IsTrue(parent.Failed,
            "Go's Fail propagates to the ancestors BEFORE it checks its own done, so a late failure still fails the parent");
    }

    // THE KIND and the text, composed as testing.go composes it. A .NET exception here is invisible to
    // a converted recover() and is classified as an infrastructure failure by the host itself.
    [TestMethod]
    public void ALateFailPanicsWithGosText()
    {
        (TestRunner runner, _) = NewRunner();

        TestExecution parent = new(runner, "TestLateFailKindParent", null, "guard.go", 1);
        TestExecution child = RunPassingChild(parent, "child");

        PanicException panic = Assert.ThrowsException<PanicException>(() => child.Fail(),
            "Go panics on a late Fail, so this host must panic too rather than throw a .NET exception recover() cannot see");

        Assert.IsInstanceOfType(panic.State, typeof(@string),
            "the panicked value must be a Go string, or a converted recover().(string) cannot assert on it");

        Assert.AreEqual($"Fail in goroutine after {child.Name} has completed", panic.State!.ToString(),
            "the panic must carry Go's own text, with no record appended -- unlike logDepth's");
    }

    // MUST-NOT-REGRESS: the live path is unchanged. Passes with the pre-fix body too, and is here so a
    // fix that made every Fail panic, or that stopped propagating, could not read green.
    [TestMethod]
    public void ALiveFailMarksItselfAndItsParentWithoutThrowing()
    {
        (TestRunner runner, _) = NewRunner();

        TestExecution parent = new(runner, "TestLiveFailParent", null, "guard.go", 1);
        TestExecution child = new(runner, "TestLiveFailParent/child", parent, "guard.go", 2);

        child.Fail();

        Assert.IsTrue(child.Failed, "a live test's own Fail must mark it failed");
        Assert.IsTrue(parent.Failed, "and must propagate to its parent, exactly as Go's does");
    }
}

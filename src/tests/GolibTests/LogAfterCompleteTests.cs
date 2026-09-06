using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class LogAfterCompleteTests
{
    // WHY THIS EXISTS.
    //
    // A log record arriving AFTER its test has finished is not an error in Go. `common.logDepth`
    // (testing.go:1015-1032) walks the parent chain and appends the record at the first ancestor that
    // is still live, returning normally; it panics only when NO live ancestor exists. And `runTests`
    // runs every top-level test as `t.Run` on a root `T` (testing.go:2155-2169, the child's parent set
    // at :1724), so mid-run there is ALWAYS a live ancestor -- Go's panic there is effectively
    // unreachable until the whole run ends.
    //
    // This host refused every late record until 2026-09-06, and diverged in TWO ways at once: it took
    // a path Go structurally never takes, and it took it by throwing a .NET InvalidOperationException
    // rather than a Go panic -- invisible to a converted `recover()` and landing in the host's own
    // INFRASTRUCTURE bucket, which by TestRunner's definition means "the host could not run the test".
    //
    // The path is REACHED, not theoretical: `internal/testenv.Command` installs a `cmd.Cancel` closure
    // that calls `t.Logf` (exec.go:186,199), invoked from `os/exec`'s `watchCtx` goroutine, which can
    // outlive the test that started it.
    //
    // The host has no _test.go and no behavioral test can reach it, so this takes the route
    // TestExecutionOutputCapTests and ConcurrentSubtestRunTests document: an MSTest guard binding the
    // converted package directly, constructing executions and finishing them deterministically rather
    // than racing a goroutine against a test's completion.
    //
    // WHY NOT THE ROW THAT FOUND IT. `runtime`'s TestCrashWhileTracing is where this surfaced, and it
    // is the WRONG instrument: measured SOLO at master on 2026-09-06 the row fails cleanly in 2.33 s
    // and the host lives, because a solo run's cleanup fires while the test is still live. A solo run
    // is the one context in which the late log CANNOT happen, so a run gated to that row could never
    // produce the string it was being read for. The divergence is deterministic at this tier; the row
    // was never able to decide it.
    //
    // DISCRIMINATING ARMS: three. The parent-walk arms (LandsOnTheLiveParent, LandsOnTheNearestLiveAncestor)
    // and the no-ancestor arm (PanicsWithGosTextWhenNoAncestorIsLive) each fail with the pre-fix body --
    // the first two by throwing at all, the third by throwing the wrong type with the wrong text. The
    // fourth arm (a live test's own record) is a MUST-NOT-REGRESS arm: it passes either way, and is here
    // so a fix that routed everything to the parent would not read green.

    private static (TestRunner Runner, TestReporter Reporter) NewRunner()
    {
        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return (runner, reporter);
    }

    private static TestExecution NewExecution(TestRunner runner, string name, TestExecution? parent = null) =>
        new(runner, name, parent, "guard.go", 1);

    // The host's own way of finishing an execution outside Execute: RecordGoroutinePanic claims the
    // terminal event and marks the execution done. Deterministic -- no threads, no timing, and no
    // dependence on the run loop the guarded defect would break.
    private static void Finish(TestExecution execution, string report) =>
        execution.RecordGoroutinePanic(report);

    private static string TerminalOutput(TestReporter reporter, string name)
    {
        TestEvent terminal = reporter.Events.Single(e => e.Test == name && e.Action == "fail");

        Assert.IsNotNull(terminal.Output, $"{name} reported no output at all");

        return terminal.Output!;
    }

    private const string LateRecord = "LATE-RECORD-FROM-A-FINISHED-TEST";

    // Go's case, and the one os/exec's watcher goroutine actually takes: the test is done, its parent
    // is not, so the record lands on the parent and the call returns normally.
    [TestMethod]
    public void ALateRecordLandsOnTheLiveParent()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();

        TestExecution parent = NewExecution(runner, "TestLateLogParent");
        TestExecution child = NewExecution(runner, "TestLateLogParent/child", parent);

        Finish(child, "child terminal");

        // The whole defect: this threw before 2026-09-06.
        child.Log(LateRecord);

        Finish(parent, "parent terminal");

        StringAssert.Contains(TerminalOutput(reporter, parent.Name), LateRecord,
            "a record logged after the child finished must land on the still-live parent, as Go's logDepth appends it to the first non-done ancestor");
    }

    // What separates a WALK from a one-step parent check: the immediate parent is done too, so the
    // record must travel to the grandparent rather than being refused at the first closed door.
    [TestMethod]
    public void ALateRecordLandsOnTheNearestLiveAncestor()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();

        TestExecution grandparent = NewExecution(runner, "TestLateLogRoot");
        TestExecution parent = NewExecution(runner, "TestLateLogRoot/mid", grandparent);
        TestExecution child = NewExecution(runner, "TestLateLogRoot/mid/leaf", parent);

        Finish(child, "leaf terminal");
        Finish(parent, "mid terminal");

        child.Log(LateRecord);

        Finish(grandparent, "root terminal");

        StringAssert.Contains(TerminalOutput(reporter, grandparent.Name), LateRecord,
            "with the immediate parent also finished the record must travel to the nearest LIVE ancestor, not stop at the first done one");

        Assert.IsFalse(TerminalOutput(reporter, parent.Name).Contains(LateRecord),
            "the record must not land on an ancestor that had already finished");
    }

    // Go's remaining case, and the only one that panics: nothing above is live. The text is Go's own,
    // composed as testing.go:1029 composes it, and the KIND is a Go panic rather than a .NET exception
    // so a converted recover() can see it and golib's backstop reports it Go-style.
    [TestMethod]
    public void ALateRecordPanicsWithGosTextWhenNoAncestorIsLive()
    {
        (TestRunner runner, _) = NewRunner();

        TestExecution solo = NewExecution(runner, "TestLateLogSolo");

        Finish(solo, "solo terminal");

        PanicException panic = Assert.ThrowsException<PanicException>(() => solo.Log(LateRecord),
            "with no live ancestor Go panics, so this host must panic too -- a .NET exception is invisible to a converted recover() and is classified as an infrastructure failure");

        // The DYNAMIC TYPE is observable and is asserted first: `builtin.panic` normalises a C# string
        // to `@string` at its boxing boundary precisely so a converted `recover().(string)` type
        // assertion succeeds -- boxed as System.String it would match nothing on the recover side. This
        // arm's first draft compared against `panic.State as string`, read null, and was the guard
        // catching the guard.
        Assert.IsInstanceOfType(panic.State, typeof(@string),
            "the panicked value must be a Go string, or a converted recover().(string) cannot assert on it");

        Assert.AreEqual($"Log in goroutine after {solo.Name} has completed: {LateRecord}", panic.State!.ToString(),
            "the panic must carry Go's own text, composed exactly as testing.go:1029 composes it");
    }

    // MUST-NOT-REGRESS: a live test's own record still goes to the live test. This arm passes with the
    // pre-fix body too, and is here so a fix that sent everything up the chain could not read green.
    [TestMethod]
    public void ALiveTestsOwnRecordStillLandsOnItself()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();

        TestExecution parent = NewExecution(runner, "TestLiveLogParent");
        TestExecution child = NewExecution(runner, "TestLiveLogParent/child", parent);

        child.Log(LateRecord);

        Finish(child, "child terminal");
        Finish(parent, "parent terminal");

        StringAssert.Contains(TerminalOutput(reporter, child.Name), LateRecord,
            "a record logged while the test is live belongs to that test");

        Assert.IsFalse(TerminalOutput(reporter, parent.Name).Contains(LateRecord),
            "a live test's own record must not be forwarded to its parent");
    }
}

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TestHostWatchdogTests
{
    // WHY THIS EXISTS. Go has no per-test deadline: a test that never returns holds a Go test binary until
    // -timeout panics it, and every test after it goes unmeasured. The 1.26 hop lost three whole rows that
    // way (synctest once, runtime twice; runtime's TestProfBufWakeup took the full 2 h deadline and left 206
    // rows empty). The host's watchdog (TestRunner.WaitBounded, ruled as DESIGN-object-lifetime-disclosure.md
    // OQ-L3(b)) reports a top-level test still running at the per-test deadline as FAILED and ABANDONED, and
    // the run goes on. These arms drive the real run loop with a host-side registration, as GoTestOrderTests
    // does, and finish executions deterministically where the order of two terminal reports is the question.

    // The deadline arrives the way the pipeline sends it: in the environment, read once and cleared.
    private static TestOptions OptionsWithDeadline(string deadline)
    {
        Environment.SetEnvironmentVariable("GO2CS_TEST_DEADLINE", deadline);
        return TestOptions.Parse([]);
    }

    // A test that blocks forever, beside one registered after it that passes. Before the watchdog the run
    // never returned; now the blocked test fails at the deadline and the next one runs.
    [TestMethod]
    public void AHungTestFailsAtTheDeadlineAndTheRunContinues()
    {
        using ManualResetEventSlim release = new(false);

        TestRegistry registry = new("watchdog", []);
        registry.Add("TestAHangs", _ => release.Wait(), "a_test.go", 1);
        registry.Add("TestBPasses", _ => { }, "a_test.go", 2);

        TestReporter reporter = new("watchdog", json: false, verbose: false);
        TestRunner runner = new(registry, OptionsWithDeadline("300ms"), reporter, ".", ".");

        Task<nint> run = Task.Run(runner.RunAll);
        bool returned = run.Wait(TimeSpan.FromSeconds(30));

        // Let the abandoned test's thread end, so it does not outlive this test.
        release.Set();

        Assert.IsTrue(returned, "RunAll did not return within 30 s: a hung test held the whole run");
        Assert.AreEqual((nint)1, run.Result, "a run with an abandoned test exits as a failing run");
        Assert.IsTrue(reporter.Events.Any(e => e.Test == "TestAHangs" && e.Action == "fail"), "the hung test has no fail verdict");
        Assert.IsTrue(reporter.Events.Any(e => e.Test == "TestBPasses" && e.Action == "pass"), "the test after the hung one did not run");
    }

    // The cascade tag and the watchdog's own mark: the abandoned verdict says it is the watchdog's, and the test
    // after it says which abandoned test's goroutines it ran beside.
    [TestMethod]
    public void TheAbandonedVerdictIsMarkedAndLaterEventsCarryTheCascadeTag()
    {
        using ManualResetEventSlim release = new(false);

        TestRegistry registry = new("watchdog", []);
        registry.Add("TestAHangs", _ => release.Wait(), "a_test.go", 1);
        registry.Add("TestBPasses", _ => { }, "a_test.go", 2);

        TestReporter reporter = new("watchdog", json: false, verbose: false);
        TestRunner runner = new(registry, OptionsWithDeadline("300ms"), reporter, ".", ".");

        Task<nint> run = Task.Run(runner.RunAll);
        bool returned = run.Wait(TimeSpan.FromSeconds(30));
        release.Set();

        Assert.IsTrue(returned, "RunAll did not return within 30 s");

        TestEvent hung = reporter.Events.Single(e => e.Test == "TestAHangs" && e.Action == "fail");
        Assert.IsTrue(hung.Abandoned, "the hung test's fail must carry the watchdog's Abandoned mark");
        StringAssert.Contains(hung.Output, "did not finish within the per-test deadline");
        Assert.IsNull(hung.AfterAbandoned, "the abandoned verdict is not itself 'after' an abandoned test");

        // The converter reads the mark by its JSON name (normalizedTestEvent.Abandoned, json:"abandoned").
        StringAssert.Contains(JsonSerializer.Serialize(hung, TestReporter.JsonOptions), "\"abandoned\":true");

        TestEvent after = reporter.Events.Single(e => e.Test == "TestBPasses" && e.Action == "pass");
        Assert.AreEqual("TestAHangs", after.AfterAbandoned, "a later event must name the abandoned test it ran beside");
        Assert.IsFalse(after.Abandoned);
    }

    // CAS, one side: the test reported its own verdict first, so the watchdog reports nothing and says so.
    [TestMethod]
    public void AbandonAfterTheTestReportedIsANoOp()
    {
        TestReporter reporter = new("watchdog", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("watchdog", []), new TestOptions(), reporter, ".", ".");
        TestExecution execution = new(runner, "TestFinishedFirst", null, "a_test.go", 1);

        execution.RecordGoroutinePanic("its own terminal report");

        Assert.IsFalse(execution.Abandon(TimeSpan.FromSeconds(1)), "Abandon must lose to a verdict already reported");
        Assert.AreEqual(1, reporter.Events.Count(e => e.Test == "TestFinishedFirst" && e.Action == "fail"), "exactly one terminal event");
        Assert.IsFalse(reporter.Events.Any(e => e.Abandoned), "no watchdog verdict for a test that finished");
    }

    // CAS, the other side: the watchdog abandoned the test, and the test's own late terminal path reports nothing.
    [TestMethod]
    public void ALateTerminalReportAfterAbandonIsDropped()
    {
        TestReporter reporter = new("watchdog", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("watchdog", []), new TestOptions(), reporter, ".", ".");
        TestExecution parent = new(runner, "TestAbandonedFirst", null, "a_test.go", 1);
        TestExecution child = new(runner, "TestAbandonedFirst/inFlight", parent, "a_test.go", 2);

        Assert.IsTrue(parent.Abandon(TimeSpan.FromSeconds(1)), "the first Abandon must win");

        child.RecordGoroutinePanic("late, after the abandon");
        parent.RecordGoroutinePanic("late, after the abandon");

        Assert.AreEqual(1, reporter.Events.Count(e => e.Test == "TestAbandonedFirst" && e.Action == "fail"), "the parent reports once");
        Assert.AreEqual(1, reporter.Events.Count(e => e.Test == "TestAbandonedFirst/inFlight" && e.Action == "fail"), "the in-flight subtest is abandoned with it, once");
        Assert.IsTrue(reporter.Events.Where(e => e.Action == "fail").All(e => e.Abandoned), "both verdicts are the watchdog's");
        Assert.IsFalse(reporter.Events.Any(e => e.Test.StartsWith("TestAbandonedFirst", StringComparison.Ordinal) && e.Action != "fail"),
            "a late report after the abandon reports nothing, not even an infrastructure-error naming the test");

        int childIndex = reporter.Events.ToList().FindIndex(e => e.Test == "TestAbandonedFirst/inFlight" && e.Action == "fail");
        int parentIndex = reporter.Events.ToList().FindIndex(e => e.Test == "TestAbandonedFirst" && e.Action == "fail");
        Assert.IsTrue(childIndex < parentIndex, "a subtest's verdict precedes its parent's, Go's order");
    }

    // G's review of the first cut (BLOCKING): an abandoned PARALLEL subtest still queued for a -parallel slot took one
    // AFTER the abandon and kept it. With -parallel 1: TestAParent's two parallel subtests both block; /1 holds the one
    // slot, /2 queues; the parent is abandoned, which gives /1's slot back; /2, abandoned while queued, then took it,
    // ran, blocked and never gave it back, so TestZParallel (a later parallel top-level test) waited forever for a slot.
    [TestMethod]
    public void AnAbandonedQueuedParallelSubtestNeverTakesASlot()
    {
        using ManualResetEventSlim release = new(false);

        TestRegistry registry = new("watchdog", []);
        registry.Add("TestAParent", t =>
        {
            t.Value.Execution!.Run("1", c => { c.Value.Parallel(); release.Wait(); });
            t.Value.Execution!.Run("2", c => { c.Value.Parallel(); release.Wait(); });
        }, "a_test.go", 1);
        registry.Add("TestZParallel", t => t.Value.Parallel(), "a_test.go", 2);

        TestReporter reporter = new("watchdog", json: false, verbose: false);
        Environment.SetEnvironmentVariable("GO2CS_TEST_DEADLINE", "300ms");
        TestRunner runner = new(registry, TestOptions.Parse(["-test.parallel=1"]), reporter, ".", ".");

        // /2 must reach the slot AFTER its abandon. On its own that is a race a body cannot force (its thread usually
        // wakes first, and then its own abandon gives the slot back), so the seam holds it until it is abandoned.
        TestExecution.BeforeParallelSlotForGuard = execution =>
        {
            if (execution.Name == "TestAParent/2")
                SpinWait.SpinUntil(() => execution.Abandoned, TimeSpan.FromSeconds(10));
        };

        bool returned;

        try
        {
            Task<nint> run = Task.Run(runner.RunAll);
            returned = run.Wait(TimeSpan.FromSeconds(30));
        }
        finally
        {
            TestExecution.BeforeParallelSlotForGuard = null;
            release.Set();
        }

        Assert.IsTrue(returned, "RunAll did not return within 30 s: an abandoned subtest kept the only -parallel slot");
        Assert.IsTrue(reporter.Events.Any(e => e.Test == "TestAParent" && e.Action == "fail" && e.Abandoned), "the parent was not abandoned");
        Assert.IsTrue(reporter.Events.Any(e => e.Test == "TestZParallel" && e.Action == "pass"), "the later parallel test did not run");
    }

    // G's review, fix 2: the descriptor limit. A test may lower RLIMIT_NOFILE (syscall's TestPrlimitFileLimit), and
    // the host puts the run's limit back between tests. An ABANDONED test's thread can still lower it later, while
    // another test runs. The run's limit is the one restored, and drift found after an abandon names the abandoned
    // test, since it may be the one that moved it. Linux and macOS only (HostDescriptorLimit.Applies).
    [TestMethod]
    public void AnAbandonedTestsLateDescriptorLimitChangeIsUndoneAndNamed()
    {
        if (!HostDescriptorLimit.Applies || HostDescriptorLimit.Snapshot() is not HostDescriptorLimit.RLimit original)
        {
            Assert.Inconclusive("RLIMIT_NOFILE is not readable on this host");
            return;
        }

        HostDescriptorLimit.RLimit lowered = new() { Cur = original.Cur - 1, Max = original.Max };
        using ManualResetEventSlim mayLower = new(false);
        using ManualResetEventSlim hasLowered = new(false);
        using ManualResetEventSlim release = new(false);

        TestRegistry registry = new("watchdog", []);
        registry.Add("TestAHangs", _ =>
        {
            // Blocks past the deadline; abandoned, its thread then lowers the limit while TestBPasses runs.
            mayLower.Wait();
            HostDescriptorLimit.Restore(lowered);
            hasLowered.Set();
            release.Wait();
        }, "a_test.go", 1);
        registry.Add("TestBPasses", _ => { mayLower.Set(); hasLowered.Wait(TimeSpan.FromSeconds(10)); }, "a_test.go", 2);

        TestReporter reporter = new("watchdog", json: false, verbose: false);
        TestRunner runner = new(registry, OptionsWithDeadline("300ms"), reporter, ".", ".");

        System.IO.TextWriter stderr = Console.Error;
        System.IO.StringWriter captured = new();
        bool returned;
        HostDescriptorLimit.RLimit? after;

        try
        {
            Console.SetError(captured);
            Task<nint> run = Task.Run(runner.RunAll);
            returned = run.Wait(TimeSpan.FromSeconds(30));
            after = HostDescriptorLimit.Snapshot();
        }
        finally
        {
            Console.SetError(stderr);
            release.Set();
            HostDescriptorLimit.Restore(original);
        }

        Assert.IsTrue(returned, "RunAll did not return within 30 s");
        Assert.IsTrue(hasLowered.IsSet, "the abandoned test's thread never lowered the limit: the arm did not run its case");
        Assert.AreEqual(original.Cur, after?.Cur, "the run ended under the abandoned test's lowered limit, not the run's own");
        StringAssert.Contains(captured.ToString(), "RLIMIT_NOFILE", "no drift report after the abandon");
        StringAssert.Contains(captured.ToString(), "TestAHangs", "the drift report does not name the abandoned test");
    }
}

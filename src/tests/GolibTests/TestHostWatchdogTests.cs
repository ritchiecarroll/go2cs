using System;
using System.Linq;
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
}

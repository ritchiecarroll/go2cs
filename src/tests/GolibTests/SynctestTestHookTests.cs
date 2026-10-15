using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;
using synctest = go.@internal.synctest_package;

namespace GolibTests;

[TestClass]
public class SynctestTestHookTests
{
    // WHY THIS EXISTS.
    //
    // Go 1.25's testing/synctest.Test(t, f) is `synctest.Run(func() { ok = testingSynctestTest(t, f) })`,
    // and testingSynctestTest is testing's, reached by a push linkname (testing.go:2125). It builds a
    // child T with isSynctest set, starts it with `go tRunner(t2, f)` INSIDE the bubble, and waits on
    // `<-t2.signal`. The host is hand-owned, so it owes that body (runbook H4). testing/synctest itself
    // enters the corpus at H3; this guard drives the body the way synctest.Test will, through
    // internal/synctest's Run, which the corpus already has.
    //
    // The readings, each Go 1.26.9's:
    //   - the child is a bubble MEMBER (it shares the fake clock), carries the parent's name, and is not
    //     reported on its own (`if t.isSynctest { return }` in report; no === RUN);
    //   - its failure is the caller's (Fail walks to the parent) and its records are the caller's
    //     (destination skips an isSynctest common);
    //   - inside the bubble t.Parallel, t.Run and t.Deadline panic with Go's texts (:1762, :2051, :2164);
    //   - during the caller's cleanup, testingSynctestTest panics with Go's text (:2126).
    //
    // MSTest runs this assembly serially (no [Parallelize]).

    private static TestReporter Run(TestRegistry registry)
    {
        TestReporter reporter = new("synctestguard", json: false, verbose: false);
        new TestRunner(registry, new TestOptions(), reporter, ".", Path.GetTempPath()).RunAll();
        return reporter;
    }

    private static string? s_childName;
    private static long s_fakeElapsed;
    private static bool s_ok;

    [TestMethod]
    public void TheChildIsABubbleMemberWithTheCallersNameAndNoReportOfItsOwn()
    {
        s_childName = null;
        s_fakeElapsed = 0;
        s_ok = false;

        TestRegistry registry = new("synctestguard", []);
        registry.Add("TestBubbleChild", static t =>
        {
            synctest.Run(() => s_ok = testing_package.testingSynctestTest(t, static t2 =>
            {
                s_childName = t2.Value.Name().ToString();

                // A bubble member's sleep advances the bubble's fake clock, not the wall clock. A thread
                // outside the bubble would really sleep the hour.
                time_package.Time start = time_package.Now();
                time_package.Sleep(time_package.ΔHour);
                s_fakeElapsed = (long)time_package.Since(start);
            }));
        }, "synctest_guard_test.go", 1);

        Stopwatch wall = Stopwatch.StartNew();
        TestReporter reporter = Run(registry);
        wall.Stop();

        Assert.IsTrue(s_ok, "a passing child reports ok");
        Assert.AreEqual("TestBubbleChild", s_childName, "the child carries its caller's name (testing.go:2140)");
        Assert.AreEqual((long)time_package.ΔHour, s_fakeElapsed, "the child's Sleep ran on the bubble's fake clock");
        Assert.IsTrue(wall.Elapsed < TimeSpan.FromSeconds(60.0D), $"a bubble member does not really sleep; the run took {wall.Elapsed}");
        Assert.AreEqual(1, reporter.Events.Count(e => e.Test == "TestBubbleChild" && e.Action == "run"), "no === RUN for the child");
        Assert.AreEqual(1, reporter.Events.Count(e => e.Test == "TestBubbleChild" && e.Action is "pass" or "fail" or "skip"),
            "one terminal event: the child is reported by its caller, not on its own");
    }

    [TestMethod]
    public void TheChildsFailureAndRecordsAreTheCallers()
    {
        s_ok = true;

        TestRegistry registry = new("synctestguard", []);
        registry.Add("TestBubbleFails", static t =>
        {
            synctest.Run(() => s_ok = testing_package.testingSynctestTest(t, static t2 => t2.Value.Errorf("inside the bubble")));
        }, "synctest_guard_test.go", 2);

        TestReporter reporter = Run(registry);
        TestEvent terminal = reporter.Events.Single(e => e.Test == "TestBubbleFails" && e.Action is "pass" or "fail" or "skip");

        Assert.IsFalse(s_ok, "a failing child reports !ok");
        Assert.AreEqual("fail", terminal.Action, "the child's Fail walks to its caller");
        CollectionAssert.Contains(terminal.Records!.ToList(), "inside the bubble", "the child's record is the caller's");
    }

    private static string? s_parallel;
    private static string? s_run;
    private static string? s_deadline;

    private static string? Panic(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (PanicException ex)
        {
            return ex.Message;
        }
    }

    [TestMethod]
    public void ParallelRunAndDeadlinePanicInsideTheBubbleWithGosTexts()
    {
        s_parallel = s_run = s_deadline = null;

        TestRegistry registry = new("synctestguard", []);
        registry.Add("TestBubbleRules", static t =>
        {
            synctest.Run(() => testing_package.testingSynctestTest(t, static t2 =>
            {
                s_parallel = Panic(() => t2.Value.Parallel());
                s_run = Panic(() => t2.Value.Run("sub", static _ => { }));
                s_deadline = Panic(() => t2.Value.Deadline());
            }));
        }, "synctest_guard_test.go", 3);

        Run(registry);

        Assert.AreEqual("testing: t.Parallel called inside synctest bubble", s_parallel);
        Assert.AreEqual("testing: t.Run called inside synctest bubble", s_run);
        Assert.AreEqual("testing: t.Deadline called inside synctest bubble", s_deadline);
    }

    private static string? s_cleanup;

    [TestMethod]
    public void TheHookRefusesDuringTheCallersCleanupWithGosText()
    {
        s_cleanup = null;

        TestRegistry registry = new("synctestguard", []);
        registry.Add("TestBubbleInCleanup", static t =>
        {
            t.Value.Cleanup(() => s_cleanup = Panic(() => testing_package.testingSynctestTest(t, static _ => { })));
        }, "synctest_guard_test.go", 4);

        Run(registry);

        Assert.AreEqual("testing: synctest.Run called during t.Cleanup", s_cleanup);
    }
}

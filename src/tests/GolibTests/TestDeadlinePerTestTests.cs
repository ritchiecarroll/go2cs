using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TestDeadlinePerTestTests
{
    // WHY THIS EXISTS. T.Deadline() reported only the package deadline (-timeout), while the host also abandons a
    // top-level test at its per-test deadline (TestOptions.TestDeadline, the watchdog). A test that sizes a wait from
    // t.Deadline() then waited past the bound that actually ends it: internal/poll's TestSplicePipePool waits 0.9 of
    // what t.Deadline() leaves, so under a 45m package budget and its 30m per-test floor it waited 40.5 minutes and was
    // abandoned at 30, without printing its disclosed signature (BOARD 2026-11-12, P2 A-7). COORD ruled (A),
    // 2026-11-16: T.Deadline() reports the EARLIER of the package deadline and the top-level test's start plus the
    // per-test deadline.

    private static readonly TimeSpan s_perTest = TimeSpan.FromMinutes(10);

    // Slack for a reading taken on a loaded box: the reported instant is computed a moment before or after the
    // assertion's own clock read.
    private static readonly TimeSpan s_slack = TimeSpan.FromSeconds(5);

    private static (DateTime Reported, bool Ok, DateTime CalledAt)? s_reading;

    // The per-test deadline shorter than the package deadline is the one reported.
    [TestMethod]
    public void APerTestDeadlineShorterThanThePackageDeadlineIsWhatDeadlineReports()
    {
        Run(DateTime.UtcNow + TimeSpan.FromHours(1), "10m", static t => Read(t));

        (DateTime reported, bool ok, DateTime calledAt) = Reading();
        Assert.IsTrue(ok, "a run with both deadlines reported none");
        AssertNear(calledAt + s_perTest, reported, "the per-test deadline (10m) is shorter than the package deadline (1h)");
    }

    // The control: a package deadline shorter than the per-test deadline is reported exactly, as before.
    [TestMethod]
    public void APackageDeadlineShorterThanThePerTestDeadlineIsReportedExactly()
    {
        DateTime package = DateTime.UtcNow + TimeSpan.FromMinutes(2);
        Run(package, "10m", static t => Read(t));

        (DateTime reported, bool ok, _) = Reading();
        Assert.IsTrue(ok, "a run with both deadlines reported none");
        Assert.AreEqual(package.Ticks, reported.Ticks, "the package deadline (2m) is the earlier one and must be reported to the tick");
    }

    // The control: with the watchdog off ("0"), the package deadline is all there is.
    [TestMethod]
    public void WithNoPerTestDeadlineThePackageDeadlineIsReported()
    {
        DateTime package = DateTime.UtcNow + TimeSpan.FromHours(1);
        Run(package, "0", static t => Read(t));

        (DateTime reported, bool ok, _) = Reading();
        Assert.IsTrue(ok, "a run with a package deadline reported none");
        Assert.AreEqual(package.Ticks, reported.Ticks, "with no per-test deadline the package deadline must be reported to the tick");
    }

    // With no package deadline (-timeout 0) the per-test deadline still ends the test, so it is reported.
    [TestMethod]
    public void WithNoPackageDeadlineThePerTestDeadlineIsReported()
    {
        Run(null, "10m", static t => Read(t));

        (DateTime reported, bool ok, DateTime calledAt) = Reading();
        Assert.IsTrue(ok, "the per-test deadline bounds the test, so Deadline must report it");
        AssertNear(calledAt + s_perTest, reported, "the per-test deadline (10m) is the only one");
    }

    // The deadline is the TOP-LEVEL test's: the watchdog bounds the top-level test, and a subtest started 1.5 s into it
    // has 1.5 s less, not a fresh 10 minutes of its own.
    [TestMethod]
    public void ASubtestReportsItsTopLevelTestsDeadline()
    {
        Run(DateTime.UtcNow + TimeSpan.FromHours(1), "10m", static t =>
        {
            Thread.Sleep(1500);
            t.Value.Execution!.Run("sub", static c => Read(c));
        });

        (DateTime reported, bool ok, DateTime calledAt) = Reading();
        Assert.IsTrue(ok, "a run with both deadlines reported none");
        AssertNear(calledAt + s_perTest - TimeSpan.FromMilliseconds(1500), reported, "the subtest started 1.5 s into its top-level test");
        Assert.IsTrue(reported < calledAt + s_perTest - TimeSpan.FromSeconds(1), $"the subtest reported its own clock: {reported:O} against a call at {calledAt:O}");
    }

    // A testing.RunTests list runs inside the calling test, and the watchdog bounds the CALLER: a listed test started
    // 1.5 s into it reports the caller's deadline.
    [TestMethod]
    public void ATestRunByRunTestsReportsItsCallersDeadline()
    {
        Run(DateTime.UtcNow + TimeSpan.FromHours(1), "10m", static t =>
        {
            Thread.Sleep(1500);
            testing_package.RunTests(static (_, _) => (true, null!), new testing_package.InternalTest[] { new("TestListed", static c => Read(c)) }.slice());
        });

        (DateTime reported, bool ok, DateTime calledAt) = Reading();
        Assert.IsTrue(ok, "a run with both deadlines reported none");
        AssertNear(calledAt + s_perTest - TimeSpan.FromMilliseconds(1500), reported, "the listed test started 1.5 s into its caller");
    }

    private static void Read(ж<testing_package.T> t)
    {
        DateTime calledAt = DateTime.UtcNow;
        (time_package.Time deadline, bool ok) = t.Value.Deadline();
        s_reading = (DateTime.UnixEpoch.AddTicks(deadline.UnixNano() / 100L), ok, calledAt);
    }

    private static (DateTime, bool, DateTime) Reading()
    {
        Assert.IsTrue(s_reading.HasValue, "the test body never read its deadline");
        return s_reading!.Value;
    }

    private static void AssertNear(DateTime expected, DateTime reported, string what) =>
        Assert.IsTrue((reported - expected).Duration() <= s_slack, $"{what}: expected about {expected:O}, reported {reported:O}");

    // Runs one registered test through the real run loop with the package deadline set as TestHost.Run sets it.
    private static void Run(DateTime? packageDeadline, string perTestDeadline, Action<ж<testing_package.T>> body)
    {
        s_reading = null;

        TestRegistry registry = new("deadline", []);
        registry.Add("TestDeadline", body, "a_test.go", 1);

        TestReporter reporter = new("deadline", json: false);
        Environment.SetEnvironmentVariable("GO2CS_TEST_DEADLINE", perTestDeadline);
        TestRunner runner = new(registry, TestOptions.Parse([]), reporter, ".", ".");

        PropertyInfo property = typeof(TestHost).GetProperty(nameof(TestHost.PackageDeadlineUtc))!;
        object? saved = property.GetValue(null);
        property.SetValue(null, packageDeadline);

        try
        {
            Assert.IsTrue(Task.Run(runner.RunAll).Wait(TimeSpan.FromSeconds(30)), "RunAll did not return within 30 s");
        }
        finally
        {
            property.SetValue(null, saved);
        }

        Assert.IsTrue(reporter.Events.Any(e => e.Test == "TestDeadline" && e.Action == "pass"),
            "the test did not pass: " + string.Join("; ", reporter.Events.Where(e => e.Action is "pass" or "fail").Select(e => $"{e.Test}={e.Action}")));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TestExecutionOutputCapTests
{
    // WHY THIS EXISTS, and why it is a GolibTests file.
    //
    // A test's log output is a diagnostic; unbounded, it is the thing that kills the host. On
    // sync/atomic's TestHammerStoreLoad a late-goroutine Fatalf storm drove one execution's log to a
    // 693 MB join, RecordGoroutinePanic went OutOfMemory building it INSIDE the fatal path, and the
    // process died instead of reporting -- losing 72 of 108 verdicts. Whether the storm won that
    // race was host-dependent, so the same package read 104/108 on one machine and 35/108 on
    // another. Coordinator Ruling A (2026-08-20) queued the bound as harness robustness: "a false
    // host-dependence the harness owes nobody."
    //
    // The Phase-4 test HOST has no _test.go anywhere and no behavioral test can reach it -- a
    // converted suite exercises it only by being run, which is precisely the path a robustness guard
    // must not depend on (the failure being guarded against is the run dying). So it takes the same
    // route Sha3ReinterpretVectorTests took one tier down: an MSTest guard binding the converted
    // package directly.
    //
    // What is pinned here is the BOUND, not the OOM. Reproducing 693 MB in a unit test would BE the
    // storm -- minutes and gigabytes to re-observe a thing the bound makes unreachable. The
    // invariant is asserted against the host's own constants rather than a magic number, so it
    // survives a retune of either cap.

    private static (TestRunner Runner, TestReporter Reporter) NewRunner()
    {
        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return (runner, reporter);
    }

    private static TestExecution NewExecution(TestRunner runner, string name = "TestStorm") =>
        new(runner, name, null, "guard.go", 1);

    private static string PanicOutput(TestReporter reporter, string name)
    {
        TestEvent terminal = reporter.Events.Single(e => e.Test == name && e.Action == "fail");

        Assert.IsNotNull(terminal.Output, "the panic report carried no output at all");

        return terminal.Output!;
    }

    // The ruling's case: a storm whose records far exceed the cap still produces a BOUNDED report,
    // the panic that ended the test is still in it, and the report says it was truncated.
    [TestMethod]
    public void GoroutinePanicRecordIsBoundedUnderALogStorm()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner);

        // ~4 MiB across 512 records: four times the aggregate cap, and no single record near the
        // per-record cap, so this exercises the AGGREGATE arm specifically.
        string record = new('x', 8 * 1024);

        for (int i = 0; i < 512; i++)
            execution.Log($"{i:D4} {record}");

        const string Panic = "panic: send on closed channel";

        execution.RecordGoroutinePanic(Panic);

        string output = PanicOutput(reporter, execution.Name);

        // The invariant: aggregate cap, plus the one terminal record the cap may not drop, plus the
        // truncation notice. Stated as the host states it, so retuning either constant keeps this
        // guard honest instead of stale.
        int ceiling = TestExecution.MaxLogCharacters + TestExecution.MaxRecordCharacters + 256;

        Assert.IsTrue(output.Length <= ceiling,
            $"the panic report is unbounded: {output.Length} characters against a {ceiling} ceiling");

        // The whole reason the report was built must survive the cap.
        StringAssert.Contains(output, Panic, "the terminal panic record was dropped by the cap");

        StringAssert.Contains(output, "log record(s) dropped", "a truncated report did not say so");
    }

    // Truncation keeps the HEAD, and that is load-bearing rather than incidental: a disclosure is
    // matched by a Contains over this text (testConversion.go's signature matching), and the message
    // a signature pins is the FIRST failure. Dropping the head would silently unpin disclosed rows.
    [TestMethod]
    public void TruncationKeepsTheHeadSoADisclosureSignatureSurvives()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner, "TestSignature");

        const string Signature = "want 400000, got 399997";

        execution.Log($"first failure: {Signature}");

        string record = new('y', 8 * 1024);

        for (int i = 0; i < 512; i++)
            execution.Log($"repeat {i:D4} {record}");

        execution.Log("LAST-RECORD-BEFORE-PANIC");
        execution.RecordGoroutinePanic("panic: storm");

        string output = PanicOutput(reporter, execution.Name);

        StringAssert.Contains(output, Signature, "the head was dropped, which unpins every disclosure signature");

        Assert.IsFalse(output.Contains("LAST-RECORD-BEFORE-PANIC", StringComparison.Ordinal),
            "the tail was kept, so the cap is not bounding what it claims to bound");
    }

    // One pathological record cannot grow the report either -- the aggregate arm never sees it,
    // because the per-record arm truncates first.
    [TestMethod]
    public void OneOversizeRecordIsTruncatedOnItsOwn()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner, "TestOneBigRecord");

        execution.Log(new string('z', 4 * TestExecution.MaxRecordCharacters));
        execution.RecordGoroutinePanic("panic: one big record");

        string output = PanicOutput(reporter, execution.Name);

        Assert.IsTrue(output.Length <= TestExecution.MaxLogCharacters + TestExecution.MaxRecordCharacters + 256,
            $"a single oversize record went unbounded at {output.Length} characters");

        StringAssert.Contains(output, " [record truncated: ",
            "an oversize record was truncated without saying so");
    }

    // An oversize record keeps its TAIL too, and says how much it dropped. A long-running test's last
    // state is its record's last line: crypto/tls's TestBogoSuite reports the BoGo runner's progress
    // as one record that grows past the cap, and a head-only cut showed the runner's state ~70-160 s
    // in as if it were the state at the runner's 600 s wall (H, 2026-11-08: "324 done" read off the
    // cut, against 1,145 measured directly).
    [TestMethod]
    public void AnOversizeRecordKeepsItsTailAndStatesWhatItDropped()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner, "TestLongProgress");

        const string Head = "FIRST-LINE-OF-THE-RECORD";
        const string Last = "LAST-LINE-OF-THE-RECORD 0/206/1145/1153/6473";
        string record = $"{Head}{new string('p', 2 * TestExecution.MaxRecordCharacters)}{Last}";

        execution.Log(record);
        execution.RecordGoroutinePanic("panic: long progress");

        string output = PanicOutput(reporter, execution.Name);

        StringAssert.StartsWith(output, record[..TestExecution.MaxRecordCharacters],
            "the head changed, which can unpin a disclosure signature");
        StringAssert.Contains(output, Last, "the record's last line was dropped by the cap");

        int dropped = record.Length - TestExecution.MaxRecordCharacters - TestExecution.RecordTailCharacters;

        StringAssert.Contains(output, string.Format(TestExecution.RecordTruncatedMarker, dropped),
            "the marker did not state the dropped character count");
    }

    // The bound must be invisible to every real test, which is what keeps it off the corpus's
    // verdict surface: under the cap the report is exactly the join it always was.
    [TestMethod]
    public void OutputIsUnchangedBelowTheCap()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner, "TestOrdinary");

        List<string> records = ["alpha", "beta", "gamma"];

        foreach (string entry in records)
            execution.Log(entry);

        const string Panic = "panic: ordinary";

        execution.RecordGoroutinePanic(Panic);

        string expected = string.Join(Environment.NewLine,
            records.Append($"panic on a goroutine started by {execution.Name}{Environment.NewLine}{Panic}"));

        Assert.AreEqual(expected, PanicOutput(reporter, execution.Name),
            "the cap changed a report that never reached it");
    }

    // The death was a SERIALIZATION death -- System.Text.Json refused a 693 MB value -- so the guard
    // ends where the host does: the bounded event must actually serialize.
    [TestMethod]
    public void TheBoundedEventSerializes()
    {
        (TestRunner runner, TestReporter reporter) = NewRunner();
        TestExecution execution = NewExecution(runner, "TestSerializes");

        string record = new('w', 8 * 1024);

        for (int i = 0; i < 512; i++)
            execution.Log($"{i:D4} {record}");

        execution.RecordGoroutinePanic("panic: storm");

        TestEvent terminal = reporter.Events.Single(e => e.Test == execution.Name && e.Action == "fail");
        string json = JsonSerializer.Serialize(terminal, TestReporter.JsonOptions);

        Assert.IsTrue(json.Length > 0, "the bounded event did not serialize");
    }
}

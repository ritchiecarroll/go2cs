using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// The host's results file is written on EVERY way out of the process -- a converted os.Exit included.
//
// Go's TestMain convention ends in os.Exit(m.Run()); net/http's does on every path. A converted
// os.Exit is syscall.Exit is Environment.Exit, which never returns to TestHost.Run's completion path,
// so until 2026-09-04 a row whose TestMain exited that way reached stdout with every verdict (the
// comparison reads that stream) and left NO go2cs_test_results.json behind -- the file the sweep's
// results-tail reader consults, whose "no tail" refusal then fired on the row by construction
// (measured: both arms of net/http's goroutine-leak pair preserved a comparison record and no
// results file). The fix hangs a flush on AppDomain.ProcessExit, latched off by the host's own
// write paths; these arms drive it through its guard seam, since a guard cannot end its own
// process to watch the real handler run. The handler's firing itself is a measured property of the
// runtime (Environment.Exit from a pool thread runs ProcessExit with Environment.ExitCode set) and
// is exercised by every converted row whose TestMain calls os.Exit.
[TestClass]
public class ProcessExitResultsFlushTests
{
    private static (TestReporter Reporter, TestRegistry Registry, TestOptions Options, string ResultFile, string JUnitFile) NewHost()
    {
        string dir = Path.Combine(Path.GetTempPath(), "go2cs-process-exit-flush-guard", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(dir);

        string resultFile = Path.Combine(dir, "results.json");
        string junitFile = Path.Combine(dir, "results.xml");

        // Parsed rather than hand-set, through the flags the pipeline actually passes.
        TestOptions options = TestOptions.Parse(["--result", resultFile, "--junit", junitFile]);
        TestReporter reporter = new("guard", json: false);
        TestRegistry registry = new("guard", []);

        return (reporter, registry, options, resultFile, junitFile);
    }

    private static (string Test, string Action, string? Output)[] EventsIn(string resultFile)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(resultFile));

        return document.RootElement.GetProperty("events").EnumerateArray()
            .Select(e => (
                e.GetProperty("test").GetString() ?? "",
                e.GetProperty("action").GetString() ?? "",
                e.TryGetProperty("output", out JsonElement output) && output.ValueKind == JsonValueKind.String ? output.GetString() : null))
            .ToArray();
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    // THE GUARD. An exit that reached none of the host's write paths still leaves the results (and
    // JUnit) file behind, and the file STATES the exit: Go's shape is the PASS line M.Run printed
    // followed by the `fail` action `go test` appends for a non-zero status, in that order.
    [TestMethod]
    public void AnExitThatBypassedTheHostWritesTheResultsStatingTheStatus()
    {
        (TestReporter reporter, TestRegistry registry, TestOptions options, string resultFile, string junitFile) = NewHost();

        TestHost.ResetResultsLatchForGuard();
        reporter.ReportPackage("pass", 0.01D);

        TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 3);

        Assert.IsTrue(File.Exists(resultFile), "the results file must exist after an exit that bypassed the host's completion path");
        Assert.IsTrue(File.Exists(junitFile), "the JUnit file must exist for the same reason");

        (string Test, string Action, string? Output)[] events = EventsIn(resultFile);

        Assert.AreEqual(2, events.Length, "expected M.Run's own terminal event followed by the exit's");
        Assert.AreEqual(("", "pass"), (events[0].Test, events[0].Action), "M.Run's own package verdict must survive ahead of the exit's");
        Assert.AreEqual(("", "fail"), (events[1].Test, events[1].Action), "a non-zero exit is the package's `fail` action, as `go test` appends it");
        StringAssert.Contains(events[1].Output ?? "", "exit status 3", "the terminal event must state the exit status it was written for");
    }

    // A zero status is a `pass` -- the flush states what happened, it does not invent a failure.
    [TestMethod]
    public void AZeroStatusExitIsStatedAsPass()
    {
        (TestReporter reporter, TestRegistry registry, TestOptions options, string resultFile, _) = NewHost();

        TestHost.ResetResultsLatchForGuard();
        TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 0);

        (string Test, string Action, string? Output)[] events = EventsIn(resultFile);

        Assert.AreEqual(1, events.Length);
        Assert.AreEqual(("", "pass"), (events[0].Test, events[0].Action));
        StringAssert.Contains(events[0].Output ?? "", "exit status 0");
    }

    // The flush writes ONCE. A second firing -- or a fatal path's own Environment.Exit(2) reaching
    // the handler after that path already wrote -- adds no event and rewrites nothing.
    [TestMethod]
    public void TheFlushWritesOnce()
    {
        (TestReporter reporter, TestRegistry registry, TestOptions options, string resultFile, _) = NewHost();

        TestHost.ResetResultsLatchForGuard();
        TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 3);

        string firstHash = Hash(resultFile);
        int firstCount = reporter.Events.Count;

        TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 5);

        Assert.AreEqual(firstCount, reporter.Events.Count, "a second flush appended a terminal event");
        Assert.AreEqual(firstHash, Hash(resultFile), "a second flush rewrote the results file");
    }

    // A run that completed normally has written its record; the exit flush must leave that record
    // exactly as the completion path wrote it. Driven through the real host so the latch is set by
    // the shipped write, not by the guard.
    [TestMethod]
    public void ARunThatAlreadyWroteDisarmsTheFlush()
    {
        (TestReporter reporter, TestRegistry registry, TestOptions options, string resultFile, string junitFile) = NewHost();

        int exitCode = TestHost.Run(new TestRegistry("guard", []), ["--result", resultFile, "--junit", junitFile]);

        Assert.AreEqual(0, exitCode, "the empty run must complete normally");
        Assert.IsTrue(File.Exists(resultFile), "the completion path must have written the results file");

        string completionHash = Hash(resultFile);

        TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 3);

        Assert.AreEqual(completionHash, Hash(resultFile), "the exit flush overwrote a record the completion path had already written");
        Assert.AreEqual(0, reporter.Events.Count, "the exit flush reported an event after a completed run");
    }

    // The flush writes NOTHING to the console, in either reporter mode. Go's binary prints nothing on
    // os.Exit, and a helper process re-executed by os/exec's tests has its stdout read back by the test
    // that spawned it: the first form of this flush printed a `PASS ... exit status 0` line there and
    // failed twenty of os/exec's verdicts on a control run (2026-09-04). Positive control: the flush
    // reporting through the printing path -> RED.
    [TestMethod]
    public void TheFlushWritesNothingToTheConsole()
    {
        foreach (bool json in new[] { false, true })
        {
            (_, TestRegistry registry, TestOptions options, _, _) = NewHost();
            TestReporter reporter = new("guard", json: json);
            TextWriter original = Console.Out;
            using StringWriter captured = new();

            Console.SetOut(captured);

            try
            {
                TestHost.ResetResultsLatchForGuard();
                TestHost.FlushResultsOnProcessExitForGuard(reporter, registry, options, 3);
            }
            finally
            {
                Console.SetOut(original);
            }

            Assert.AreEqual("", captured.ToString(), $"the exit flush wrote to stdout (json={json}) -- a re-executed helper's output is read back by the test that spawned it");
            Assert.AreEqual(1, reporter.Events.Count, "the exit's terminal event must still be RECORDED");
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// The standing guard for the test host's goroutine-death path -- attribute, flush, DIE.
//
// WHY IT LIVES HERE. No behavioral project can reach this: a behavioral test is a standalone program
// that references golib and fmt, never core/testing, so golib's own process-death fidelity is what
// GoroutinePanicExitCode guards and the HOST path had nothing. This is the same reason
// TestExecutionOutputCapTests lives in GolibTests, and it follows that precedent's shape -- build the
// host machinery in-process and drive it directly.
//
// WHAT IT IS GUARDING, and why the die half is the point. The host used to CONTAIN an unhandled
// non-panic exception escaping a goroutine, so one test's failure would not cost the run. Measured,
// that inverts: recording a failure cannot UNBLOCK whatever the dead goroutine was going to signal.
// A goroutine that dies before its `wg.Done()` leaves the test parked in `sync.Wait` forever, the
// package deadline fires, and the timeout path discards every verdict -- so containment did not save
// one test, it lost the package AND the evidence.
//
// The measured instance was reflect's TestOffsetLock: four goroutines each threw
// `NotImplementedException: addReflectOff` in under a second, and CONTAINED that presented as an
// unbounded hang which ate a 40-minute deadline and truncated the suite to 99 pass / 93 fail / 1
// skip. The exception was being recorded correctly the whole time and nobody could see it.
//
// So the assertion that matters is the EXIT. Attribution and the flush are observable from their own
// artifacts and were never really in doubt; "and then the process ended with 2" is the half that
// converts a hang into a red, and it is the half a future edit could quietly drop while every other
// assertion here still passed.
[TestClass]
public class GoroutineFatalHostTests
{
    private static (TestRunner Runner, TestReporter Reporter, TestOptions Options, string ResultFile, string JUnitFile) NewHost()
    {
        string dir = Path.Combine(Path.GetTempPath(), "go2cs-goroutine-fatal-guard", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(dir);

        string resultFile = Path.Combine(dir, "results.json");
        string junitFile = Path.Combine(dir, "results.xml");

        // Parsed rather than hand-set: ResultFile/JUnitFile are private-set by design, and going
        // through the real parser is also what keeps this guard honest about the flags the pipeline
        // actually passes.
        TestOptions options = TestOptions.Parse(["--result", resultFile, "--junit", junitFile]);
        TestReporter reporter = new("guard", json: false);
        TestRunner runner = new(new TestRegistry("guard", []), options, reporter, ".", ".");

        return (runner, reporter, options, resultFile, junitFile);
    }

    private static Exception ThrownFailure()
    {
        // A REAL thrown exception, caught, so it carries a stack the way the goroutine root hands one
        // over. A fabricated `new Exception(...)` would assert against a shape the host never sees.
        try
        {
            throw new NotImplementedException("addReflectOff: external (assembly or cgo) function is not implemented");
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // THE ONE THAT MATTERS. A future edit that restores containment -- or simply forgets the exit --
    // leaves every other assertion in this file passing and reintroduces the 40-minute hang.
    [TestMethod]
    public void AGoroutineDeathENDSTheRunWithGosStatus()
    {
        (TestRunner runner, TestReporter reporter, TestOptions options, _, _) = NewHost();

        int exitCalls = 0;
        int observed = -1;

        TestHost.ReportFatalGoroutineExceptionForGuard(runner, reporter, new TestRegistry("guard", []), options, ThrownFailure(),
            code => { exitCalls++; observed = code; });

        Assert.AreEqual(1, exitCalls, "the run must end exactly once -- a goroutine death that does not exit is the hang this arc removed");
        Assert.AreEqual(2, observed, "exit status 2 is Go's own for an unrecovered panic, which is the nearest thing Go has to this");
    }

    // Attribution: the failure has to name itself in the run's events, or the red says nothing about
    // which goroutine died.
    [TestMethod]
    public void TheFailureIsRecordedAgainstTheRun()
    {
        (TestRunner runner, TestReporter reporter, TestOptions options, _, _) = NewHost();

        TestHost.ReportFatalGoroutineExceptionForGuard(runner, reporter, new TestRegistry("guard", []), options, ThrownFailure(), _ => { });

        TestEvent[] recorded = reporter.Events.Where(e => e.Output is not null && e.Output.Contains("addReflectOff", StringComparison.Ordinal)).ToArray();

        Assert.AreNotEqual(0, recorded.Length, "the goroutine failure reached no event at all -- it would die silently");
        Assert.IsTrue(recorded.Any(e => e.Output!.Contains("goroutine", StringComparison.OrdinalIgnoreCase)),
            "the record must say a GOROUTINE failed, not merely that something threw");
    }

    // The flush is why the die is survivable evidence rather than a disappearing act: the fatal path
    // discards the whole run's verdicts otherwise, which is the defect the PANIC path had to learn
    // first ("a package that had already passed six tests recorded zero").
    [TestMethod]
    public void TheEvidenceIsFlushedBEFORETheProcessEnds()
    {
        (TestRunner runner, TestReporter reporter, TestOptions options, string resultFile, string junitFile) = NewHost();

        bool resultsExistedAtExit = false;
        bool junitExistedAtExit = false;

        TestHost.ReportFatalGoroutineExceptionForGuard(runner, reporter, new TestRegistry("guard", []), options, ThrownFailure(),
            _ =>
            {
                // Sampled INSIDE the exit callback: "the files exist afterwards" would also pass if
                // the flush raced the death. This is the ordering assertion.
                resultsExistedAtExit = File.Exists(resultFile);
                junitExistedAtExit = File.Exists(junitFile);
            });

        Assert.IsTrue(resultsExistedAtExit, "the result file must be written BEFORE the process ends, or the run's verdicts die with it");
        Assert.IsTrue(junitExistedAtExit, "the JUnit file must be written before the process ends for the same reason");

        string json = File.ReadAllText(resultFile);

        Assert.IsTrue(json.Contains("addReflectOff", StringComparison.Ordinal),
            "the flushed evidence must carry the failure that caused the death, not merely exist");
    }
    // THE MARKER. The other two host-death signatures announce themselves -- a panic writes
    // "died on an unrecovered panic in a goroutine", a package deadline writes an "action":"timeout"
    // event -- but until this line existed an unhandled .NET exception escaping a goroutine simply
    // STOPPED the results stream mid-test, with no timeout and no death event. Measured in the
    // runtime/pprof walls census (2026-09-03): a stub throwing inside Goroutine.Run took the host
    // down and a slice reading 2 of 7 verdicts was indistinguishable from a mass-empty conversion
    // failure until its log was read by hand. The family's own diagnostic rule -- read the results
    // tail first, because a kill states itself -- was FALSE for exactly this member.
    [TestMethod]
    public void TheDeathWritesAPackageTerminalEventNamingTheException()
    {
        (TestRunner runner, TestReporter reporter, TestOptions options, _, _) = NewHost();

        TestHost.ReportFatalGoroutineExceptionForGuard(runner, reporter, new TestRegistry("guard", []), options, ThrownFailure(), _ => { });

        TestEvent[] terminal = reporter.Events.Where(e => e.Test.Length == 0).ToArray();

        Assert.AreNotEqual(0, terminal.Length,
            "no package-level event: the stream would simply STOP mid-test, which is the markerless truncation this closes");
        Assert.IsTrue(terminal.Any(e => (e.Output ?? "").Contains("test binary died", StringComparison.Ordinal)),
            "the package event must SAY the binary died, in the shape the panic path already uses");
        Assert.IsTrue(terminal.Any(e => (e.Output ?? "").Contains(nameof(NotImplementedException), StringComparison.Ordinal)),
            "the event must name the exception TYPE -- 'died' alone does not attribute the death to anything");
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class CrashReportTests
{
    // Go's crash report for a panic nobody recovered: `panic: <value>`, a BLANK line,
    // `goroutine N [running]:`, then the traceback — written to stderr and, when
    // debug.SetCrashOutput configured one, copied to that descriptor as well. Guarded here rather
    // than behaviorally because the behavioral suite compares only the FIRST LINE of stderr (see
    // BehavioralRunner's Program.FirstLine and its comment), which is exactly the line this arc
    // does not change; everything the arc DOES add is below it. The Go-observable end-to-end halves
    // are guarded elsewhere: GoroutinePanicExitCode pins stderr-not-stdout and exit 2
    // differentially against `go run`, and runtime/debug's own TestSetCrashOutput reads the report
    // back from a re-executed child's stderr AND from the crash file.
    //
    // Design: docs/phase4/DESIGN-crash-report.md.

    private Func<PanicException, Exception, string> m_savedRenderer;
    private nuint m_savedFd;
    private TextWriter m_savedError;

    [TestInitialize]
    public void SaveGlobalState()
    {
        // Both are process-wide slots by design (Go's crashFD is one variable, and there is one
        // crash printer), so every test here restores what it borrowed.
        m_savedRenderer = CrashReport.TracebackRenderer;
        m_savedFd = CrashReport.SetCrashOutputFd(CrashReport.NoCrashOutput);
        m_savedError = Console.Error;
    }

    [TestCleanup]
    public void RestoreGlobalState()
    {
        Console.SetError(m_savedError);
        CrashReport.SetCrashOutputFd(m_savedFd);
        CrashReport.TracebackRenderer = m_savedRenderer;
    }

    // Captures what Report writes to stderr, returning it verbatim — no line-ending translation,
    // because the report's newlines are Go's `\n` and that is part of what is being asserted.
    private static string CaptureStdErr(Action body)
    {
        StringWriter captured = new() { NewLine = "\n" };

        Console.SetError(captured);
        body();

        return captured.ToString();
    }

    private static PanicException PanicWith(object state)
    {
        return new PanicException(state);
    }

    // ---------------------------------------------------------------------------------------
    // Format
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void ReportIsPanicValueBlankLineThenTraceback()
    {
        CrashReport.TracebackRenderer = (_, _) => "goroutine 1 [running]:\nmain.main()\n\t/tmp/main.go:7\n";

        PanicException panic = PanicWith((@string)"oops");
        string report = CrashReport.Format(panic, panic);

        // Go's own shape, recorded verbatim in runtime/debug/stack_test.go above TestSetCrashOutput.
        // Asserted as ONE exact string rather than a set of Contains checks: the blank line between
        // the value and the goroutine header is the element a Contains-based guard cannot see, and
        // it is the element a naive implementation drops.
        Assert.AreEqual(
            "panic: oops\n" +
            "\n" +
            "goroutine 1 [running]:\n" +
            "main.main()\n" +
            "\t/tmp/main.go:7\n",
            report);
    }

    [TestMethod]
    public void ReportUsesGoNewlinesNotThePlatformNewline()
    {
        CrashReport.TracebackRenderer = (_, _) => "goroutine 1 [running]:\n";

        PanicException panic = PanicWith((@string)"oops");

        // A crash report is a document a Go program can be asked to read, not console decoration.
        // Console.Error.WriteLine would have put `\r\n` after the first line on Windows.
        Assert.IsFalse(CrashReport.Format(panic, panic).Contains('\r'), "the report must carry Go's newlines only");
    }

    [TestMethod]
    public void ValueRenderingFollowsGoPreprintpanics()
    {
        CrashReport.TracebackRenderer = null;

        // PanicException.Message IS Go's preprintpanics rule; the printer reads it rather than
        // reimplementing it. Pinned here so a future printer cannot start rendering State directly,
        // which is what once printed a pointer-held error's ADDRESS.
        Assert.AreEqual("panic: 42\n", CrashReport.Format(PanicWith(42), new Exception()));
        Assert.AreEqual("panic: nil\n", CrashReport.Format(PanicWith(null), new Exception()));
    }

    // ---------------------------------------------------------------------------------------
    // The fallback: no renderer registered
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void WithNoRendererTheReportIsExactlyTheOldSingleLine()
    {
        CrashReport.TracebackRenderer = null;

        PanicException panic = PanicWith((@string)"boom");

        // golib cannot spell a Go frame name — that machinery is core/runtime's, and it registers
        // the renderer from its own module initializer. Until it has, the report must be BYTE-
        // IDENTICAL to what golib printed before this arc existed: no blank line, no goroutine
        // header over frames that are not there. An uninstalled renderer costs the traceback and
        // can never produce a wrong report.
        Assert.AreEqual("panic: boom\n", CrashReport.Format(panic, panic));
    }

    // ---------------------------------------------------------------------------------------
    // The tee, and its asymmetry
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void CrashOutputReceivesTheReportAndNothingTheProgramWroteToStdErr()
    {
        CrashReport.TracebackRenderer = (_, _) => "goroutine 1 [running]:\n";

        string path = Path.Combine(Path.GetTempPath(), $"go2cs-crash-{Guid.NewGuid():N}.out");
        PanicException panic = PanicWith((@string)"oops");
        string stderr;

        try
        {
            using (SafeFileHandle handle = File.OpenHandle(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            {
                CrashReport.SetCrashOutputFd(unchecked((nuint)(nint)handle.DangerousGetHandle()));

                stderr = CaptureStdErr(() =>
                {
                    // The program's own output, exactly as the child in TestSetCrashOutput writes
                    // `println("hello")` before it panics.
                    Console.Error.Write("hello\n");
                    CrashReport.Report(panic, panic);
                });
            }

            string crash = File.ReadAllText(path);

            // The asymmetry TestSetCrashOutput pins, both directions.
            Assert.AreEqual("panic: oops\n\ngoroutine 1 [running]:\n", crash, "the crash file gets the report");
            Assert.IsFalse(crash.Contains("hello"), "the crash file must NOT get the program's own output");
            Assert.AreEqual("hello\npanic: oops\n\ngoroutine 1 [running]:\n", stderr, "stderr gets both, in order");
        }
        finally
        {
            CrashReport.SetCrashOutputFd(CrashReport.NoCrashOutput);
            File.Delete(path);
        }
    }

    [TestMethod]
    public void SetCrashOutputFdSwapsAndReturnsThePrevious()
    {
        // Go's runtime.setCrashFD contract, which runtime/debug.SetCrashOutput relies on to decide
        // whether it owns a descriptor to close: ^uintptr(0) means "nothing to close".
        Assert.AreEqual(CrashReport.NoCrashOutput, CrashReport.SetCrashOutputFd((nuint)7));
        Assert.AreEqual((nuint)7, CrashReport.CrashOutputFd);
        Assert.AreEqual((nuint)7, CrashReport.SetCrashOutputFd((nuint)9));
        Assert.AreEqual((nuint)9, CrashReport.SetCrashOutputFd(CrashReport.NoCrashOutput));
        Assert.AreEqual(CrashReport.NoCrashOutput, CrashReport.CrashOutputFd);
    }

    [TestMethod]
    public void WithNoCrashOutputConfiguredOnlyStdErrIsWritten()
    {
        CrashReport.TracebackRenderer = (_, _) => "goroutine 1 [running]:\n";

        PanicException panic = PanicWith((@string)"oops");
        string stderr = CaptureStdErr(() => CrashReport.Report(panic, panic));

        Assert.AreEqual("panic: oops\n\ngoroutine 1 [running]:\n", stderr);
    }

    // ---------------------------------------------------------------------------------------
    // Charter §7 lens (a): a panic DURING the crash print
    // ---------------------------------------------------------------------------------------

    // A value whose rendering itself faults. Go answers "panic while printing panic value"; the
    // printer must reach that answer rather than propagate, because the caller's next statement is
    // Environment.Exit(2) and a throw here would skip it.
    private sealed class HostileValue
    {
        public override string ToString() => throw new InvalidOperationException("value rendering faulted");
    }

    [TestMethod]
    public void AValueWhoseRenderingFaultsStillProducesAReport()
    {
        CrashReport.TracebackRenderer = null;

        PanicException panic = PanicWith(new HostileValue());

        Assert.AreEqual("panic: panic while printing panic value\n", CrashReport.Format(panic, panic));
    }

    [TestMethod]
    public void ARendererThatThrowsCostsTheTracebackAndNothingElse()
    {
        CrashReport.TracebackRenderer = (_, _) => throw new InvalidOperationException("renderer faulted");

        PanicException panic = PanicWith((@string)"oops");

        // A traceback is diagnostic output and must never be the thing that takes the report down.
        Assert.AreEqual("panic: oops\n", CrashReport.Format(panic, panic));
    }

    [TestMethod]
    public void ReportReturnsNormallyWhenEveryPartOfItFails()
    {
        CrashReport.TracebackRenderer = (_, _) => throw new InvalidOperationException("renderer faulted");

        PanicException panic = PanicWith(new HostileValue());

        // An invalid descriptor as well: all three of the report's moving parts faulting at once.
        CrashReport.SetCrashOutputFd((nuint)0xDEAD);

        string stderr = CaptureStdErr(() => CrashReport.Report(panic, panic));

        // Report must RETURN — the exit code is the caller's decision and is taken after this call
        // (golib's backstop does Environment.Exit(2), the test host returns 2). A printer that threw
        // here would skip both, turning a Go-faithful exit 2 into an unhandled .NET exception.
        Assert.AreEqual("panic: panic while printing panic value\n", stderr);
    }

    [TestMethod]
    public void StdErrIsWrittenEvenWhenTheTeeFails()
    {
        CrashReport.TracebackRenderer = (_, _) => "goroutine 1 [running]:\n";

        PanicException panic = PanicWith((@string)"oops");

        CrashReport.SetCrashOutputFd((nuint)0xDEAD);

        // stderr goes first and is guarded on its own: a failure teeing to the crash file must not
        // cost the report the operator actually reads.
        Assert.AreEqual("panic: oops\n\ngoroutine 1 [running]:\n", CaptureStdErr(() => CrashReport.Report(panic, panic)));
    }

    // ---------------------------------------------------------------------------------------
    // Unwrapping: the shape an escaped panic actually arrives in
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void APanicWrappedByTaskWaitIsStillFoundAndCarriesItsThrowSite()
    {
        Exception wrapper = null;

        try
        {
            // Exactly TestMain's shape in runtime/debug's TestSetCrashOutput: the host runs the
            // suite on a Task and the panic surfaces from Task.Wait as an AggregateException.
            Task.Run(static () => throw panic((@string)"oops")).Wait();
        }
        catch (AggregateException ex)
        {
            wrapper = ex;
        }

        Assert.IsNotNull(wrapper, "expected Task.Wait to wrap the panic");
        Assert.IsTrue(CrashReport.TryUnwrapPanic(wrapper, out PanicException unwrapped, out Exception thrown));
        Assert.AreEqual("oops", unwrapped.Message);
        Assert.IsNotNull(thrown, "the exception that travelled is handed back with the panic");

        // The renderer needs frames, and for a panic no GoFrame ever caught they come from the
        // exception that travelled. TryAsPanic snapshots that as PanicTrace on the way through.
        Assert.IsNotNull(unwrapped.PanicTrace);
        Assert.IsTrue(unwrapped.PanicTrace.FrameCount > 0, "the throw site must survive the wrapping");
    }

    [TestMethod]
    public void AMappedRuntimeErrorIsFoundAndRendersFromTheOriginalException()
    {
        Exception thrownAtSite = null;

        try
        {
            int zero = 0;
            _ = 1 / zero;
        }
        catch (DivideByZeroException ex)
        {
            thrownAtSite = ex;
        }

        Assert.IsTrue(CrashReport.TryUnwrapPanic(thrownAtSite, out PanicException unwrapped, out Exception thrown));
        Assert.AreEqual("runtime error: integer divide by zero", unwrapped.Message);

        // The synthesized panic was never thrown, so only the original carries frames — which is
        // why the renderer takes BOTH.
        Assert.AreSame(thrownAtSite, thrown);
        Assert.IsNotNull(unwrapped.PanicTrace);
    }

    [TestMethod]
    public void AnExceptionThatCarriesNoPanicIsLeftAlone()
    {
        // The backstop's other branches must keep their behavior: a managed failure escaping
        // converted code is a defect to diagnose, reported with its full chain, not dressed up as
        // a Go panic.
        Assert.IsFalse(CrashReport.TryUnwrapPanic(new InvalidOperationException("not a panic"), out _, out _));
        Assert.IsFalse(CrashReport.TryUnwrapPanic(new AggregateException(new InvalidOperationException("nope")), out _, out _));
        Assert.IsFalse(CrashReport.TryUnwrapPanic(null, out _, out _));
    }

    // ---------------------------------------------------------------------------------------
    // The registration the converted runtime performs
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void TheConvertedRuntimeRegistersTheTracebackRenderer()
    {
        // Touching the runtime package runs its module initializer, which is where the hook is
        // filled — the same inverted-dependency shape panicvalues_impl.cs uses for the
        // divide-by-zero panic VALUE. Without this the report would silently keep the fallback
        // shape forever, which compiles and looks fine.
        _ = runtime_package.GOMAXPROCS(0);

        Assert.IsNotNull(CrashReport.TracebackRenderer, "core/runtime must register the crash traceback renderer");

        // A module initializer runs once per process, so tearing the registration down again would
        // be permanent. Keep it as this class's restore point.
        m_savedRenderer = CrashReport.TracebackRenderer;

        PanicException raised = PanicWith((@string)"oops");

        try
        {
            tracebackprobe_package.Raise(raised);
        }
        catch (PanicException caught)
        {
            string report = CrashReport.Format(caught, caught);

            // The header Go writes above a traceback, and the blank line above it — composed from
            // the SAME appendGoFrames debug.Stack() uses, so a frame here is spelled exactly as
            // runtime/debug's banked TestStack requires. The Go frame that raised the panic prints;
            // this test method is a host frame, which Go's traceback has no place for.
            StringAssert.StartsWith(report, "panic: oops\n\ngoroutine 1 [running]:\n");
            StringAssert.Contains(report, "tracebackprobe.Raise()\n");
            Assert.IsFalse(report.Contains(nameof(TheConvertedRuntimeRegistersTheTracebackRenderer)), report);
        }
    }
}

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

[TestClass]
public class FatalReportTests
{
    // Go's report for a FATAL error — runtime.throw and runtime.fatal, which no recover() can see:
    // `fatal error: <text>`, a BLANK line, the failing goroutine's header and frames, then every
    // other goroutine's block; and then exit 2. The shape is measured at the corpus pin in
    // docs/phase4/DESIGN-fatal-path.md §1, and the acceptance it carries is §6.
    //
    // WHAT THIS FILE CAN AND CANNOT ASSERT, stated because the split is not obvious. Format is the
    // whole testable surface: Fatal ends with Environment.Exit(2), so a test that called it would
    // take the test host down, and the two predicates that need a dead process — exit 2, and the
    // fatal being unrecoverable — are the PROBE's (docs/phase4/probes/c1-fatal-path), which runs a
    // real converted program end to end on both hosts. §6's predicates 1, 2 and 4 are here; 3, 5
    // and 6 are the probe's. Splitting them the other way would give this file an arm that cannot
    // run and the probe an arm that needs no process.
    //
    // Sibling: CrashReportTests, which guards the PANIC path's report. The two paths diverge inside
    // runtime/panic.cs and are deliberately guarded apart.

    private Func<bool, string> m_savedRenderer;

    [TestInitialize]
    public void SaveGlobalState()
    {
        // The converted runtime is loaded FIRST and the slot saved AFTER, and the order is
        // load-bearing rather than tidy: a [ModuleInitializer] runs ONCE per process, so saving a
        // null slot before runtime had loaded and restoring it in TestCleanup would null the real
        // renderer out for every later test in the assembly — a guard that silently disarms the
        // thing it guards.
        LoadConvertedRuntime();

        // A process-wide slot by design — Go has one fatal printer — so every test here restores
        // what it borrowed. MSTest runs serially within an assembly (no [Parallelize], no
        // .runsettings), which is what makes a process-global slot assertable at all.
        m_savedRenderer = FatalReport.TracebackRenderer;
    }

    [TestCleanup]
    public void RestoreGlobalState()
    {
        FatalReport.TracebackRenderer = m_savedRenderer;
    }

    // Forces the converted runtime assembly to load, so its [ModuleInitializer] has run and the
    // real renderer is installed. GOMAXPROCS(0) is the cheapest touch that changes nothing: Go
    // documents n < 1 as "does not change the current setting".
    private static void LoadConvertedRuntime()
    {
        runtime_package.GOMAXPROCS(0);
    }

    // ---------------------------------------------------------------------------------------
    // Format — the shape, against a synthetic renderer
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void ReportIsFatalErrorLineBlankLineThenTraceback()
    {
        FatalReport.TracebackRenderer = _ => "goroutine 1 [running]:\nmain.main()\n\t/tmp/main.go:7\n";

        // Asserted as ONE exact string rather than a set of Contains checks, for the reason the
        // panic side's twin records: the BLANK line between the text and the first goroutine header
        // is the element a Contains-based guard cannot see, and it is the element a naive
        // implementation drops. Go's own fatal carries it — measured at the corpus pin, §1.
        Assert.AreEqual(
            "fatal error: boom\n" +
            "\n" +
            "goroutine 1 [running]:\n" +
            "main.main()\n" +
            "\t/tmp/main.go:7\n",
            FatalReport.Format("boom", userFault: false));
    }

    [TestMethod]
    public void ReportUsesGoNewlinesNotThePlatformNewline()
    {
        FatalReport.TracebackRenderer = _ => "goroutine 1 [running]:\n";

        // A fatal report is a document a Go program can be asked to read, not console decoration:
        // Go's own tests grep their own crash output. Console.Error.WriteLine would have put `\r\n`
        // after the first line on Windows.
        Assert.IsFalse(
            FatalReport.Format("boom", userFault: false).Contains('\r'),
            "the report must carry Go's newlines only");
    }

    [TestMethod]
    public void WithNoRendererTheReportIsExactlyTheSingleLine()
    {
        FatalReport.TracebackRenderer = null;

        // golib cannot spell a Go frame name — that machinery is core/runtime's, which registers
        // the renderer from its own module initializer. Until it has, the report must be the line
        // and nothing more: no blank line, no goroutine header over frames that are not there. An
        // uninstalled renderer costs the traceback and can never produce a wrong report.
        Assert.AreEqual("fatal error: boom\n", FatalReport.Format("boom", userFault: false));
    }

    [TestMethod]
    public void AThrowingRendererCostsTheTracebackAndNotTheReport()
    {
        FatalReport.TracebackRenderer = _ => throw new InvalidOperationException("renderer defect");

        // A traceback is diagnostic output and must never be the thing that takes the report down:
        // a report ABOUT the reporter is worse than the divergence it would describe, and the
        // operator still needs the line that says what happened. Go itself takes this posture one
        // level over, answering "panic while printing panic value" rather than losing the report.
        Assert.AreEqual("fatal error: boom\n", FatalReport.Format("boom", userFault: false));
    }

    [TestMethod]
    public void AnEmptyTracebackDoesNotAddTheBlankLine()
    {
        FatalReport.TracebackRenderer = _ => "";

        // The blank line is the SEPARATOR between the text and the first header, so a renderer that
        // answers nothing must not leave a trailing one hanging under the text.
        Assert.AreEqual("fatal error: boom\n", FatalReport.Format("boom", userFault: false));
    }

    [TestMethod]
    public void TheThrowTypeAxisReachesTheRenderer()
    {
        bool? seen = null;

        FatalReport.TracebackRenderer = userFault => { seen = userFault; return "goroutine 1 [running]:\n"; };

        // Go's throw is throwTypeRuntime — the runtime itself is at fault, and gotraceback raises
        // the level so system goroutines and runtime frames ARE shown — while fatal is
        // throwTypeUser and leaves the level alone. That axis is the ONLY difference between the
        // two displaced bodies, so a report that dropped it would make `fatal` print a throw's dump
        // and nothing else would notice.
        FatalReport.Format("boom", userFault: false);
        Assert.IsNotNull(seen, "the renderer was not reached at all");
        Assert.IsFalse(seen!.Value, "throw must reach the renderer as throwTypeRuntime");

        FatalReport.Format("boom", userFault: true);
        Assert.IsTrue(seen!.Value, "fatal must reach the renderer as throwTypeUser");
    }

    // ---------------------------------------------------------------------------------------
    // The REAL renderer, as core/runtime registers it — §6 predicates 1, 2 and 4
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void TheConvertedRuntimeRegistersTheRendererFromItsModuleInitializer()
    {
        LoadConvertedRuntime();

        // The dependency inverts exactly as it does for the crash traceback and the divide-by-zero
        // panic VALUE: golib declares the hook, the runtime package fills it. Asserted as the
        // DECISION (the slot is filled) rather than by grepping an emitted file, which is the shape
        // that goes vacuous when a construct legitimately relocates.
        Assert.IsNotNull(
            FatalReport.TracebackRenderer,
            "core/runtime must install the fatal traceback renderer from its module initializer");
    }

    [TestMethod]
    public void TheRealRendererProducesAGoShapedReportAndNoDotNetFrames()
    {
        // No renderer is installed here: TestInitialize loaded the converted runtime and the slot
        // still holds what IT registered, which is the whole point of this arm.
        string report = FatalReport.Format("boom", userFault: false);
        string[] lines = report.Split('\n');

        // §6.1 — exactly ONE `fatal error:` line, and the text is the one that was handed in.
        int fatalLines = 0;

        foreach (string line in lines)
        {
            if (line.StartsWith("fatal error: ", StringComparison.Ordinal))
                fatalLines++;
        }

        Assert.AreEqual(1, fatalLines, "the report carries Go's fatal line exactly once");
        Assert.IsTrue(lines.Length > 2, $"the report is too short to carry a traceback:\n{report}");
        Assert.AreEqual("fatal error: boom", lines[0]);
        Assert.AreEqual("", lines[1], "a BLANK line separates the text from the first goroutine header");

        // §6.2 — at least one goroutine header in Go's shape. The extended `gp=/m=/mp=` form Go's
        // throw-level traceback carries is deliberately NOT produced (§5a: we hold no g, m or mp
        // addresses that mean anything, and inventing three plausible hex numbers would be
        // fabrication in the one artifact an operator reads when things have already gone wrong),
        // so this matches the plain form the panic side already prints.
        Regex header = new(@"^goroutine \d+ \[[^\]]+\]:$");
        bool sawHeader = false;

        foreach (string line in lines)
        {
            if (header.IsMatch(line))
            {
                sawHeader = true;
                break;
            }
        }

        Assert.IsTrue(sawHeader, $"no `goroutine <N> [<status>]:` header in the report:\n{report}");

        // §6.4 — THE DISCRIMINATOR, and the one predicate that failed before this arc. What a fatal
        // printed then was Go's first line followed by a .NET exception dump naming getcallerpc;
        // a pass is that second block being a Go-spelled traceback instead. The CLR's frame form is
        // `   at <Namespace>.<Type>.<Method>(...)`, which no Go frame line can look like: a Go frame
        // is `<pkg>.<Func>()` with a tab-indented `<file>:<line>` beneath it.
        Regex dotNetFrame = new(@"^\s+at \S+\.\S+");

        foreach (string line in lines)
        {
            Assert.IsFalse(
                dotNetFrame.IsMatch(line),
                $"a .NET stack-trace line reached the fatal report — the very shape this arc replaced:\n{line}");
        }
    }

    // ---------------------------------------------------------------------------------------
    // The two attributes the frame BOUNDARY depends on
    // ---------------------------------------------------------------------------------------

    [TestMethod]
    public void TheFatalEntryPointIsNotInlinable()
    {
        MethodInfo fatal = typeof(FatalReport).GetMethod(
            nameof(FatalReport.Fatal),
            BindingFlags.Public | BindingFlags.Static)!;

        // Load-bearing rather than defensive, and guarded because nothing else would notice its
        // removal: the registered renderer starts its walk at the first frame ABOVE this class's
        // own, which is only a boundary while this method HAS a frame. Inlined, the report would
        // silently begin one frame deep — the same defect, on the same mechanism, that made
        // net/http's goroutine-leak filter count the host as a leak in 2026-09-04.
        Assert.IsTrue(
            (fatal.MethodImplementationFlags & MethodImplAttributes.NoInlining) != 0,
            "FatalReport.Fatal must be NoInlining: the traceback's frame boundary is its frame");
    }

    [TestMethod]
    public void TheRegisteredRendererIsNotInlinable()
    {
        LoadConvertedRuntime();

        MethodInfo? renderer = typeof(runtime_package).GetMethod(
            "fatalTraceback",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            [typeof(bool)],
            modifiers: null);

        Assert.IsNotNull(renderer, "core/runtime must declare fatalTraceback(bool)");

        // Same rule from the other side: the renderer anchors on ITS OWN frame by identity, so an
        // inlined renderer has no boundary to find and falls to the count-based fallback the
        // identity boundary exists to replace.
        Assert.IsTrue(
            (renderer!.MethodImplementationFlags & MethodImplAttributes.NoInlining) != 0,
            "runtime.fatalTraceback must be NoInlining: it anchors the walk on its own frame");
    }
}

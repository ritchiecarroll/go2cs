using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class HostTestMainParseOrderTests
{
    // This class swaps the process-global flag.CommandLine; every other in-process class shares it,
    // so the original is put back after each test (i9, 2026-09-27: the shared-CommandLine class).
    private ж<flag_package.FlagSet>? m_originalCommandLine;

    [TestInitialize]
    public void CaptureTheCommandLine() => m_originalCommandLine = flag_package.CommandLine;

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        if (m_originalCommandLine is not null)
            flag_package.CommandLine = m_originalCommandLine;
    }

    // THE INVARIANT: the host must not parse the command line before a package's TestMain runs.
    //
    // Go puts the parse in exactly one place — m.Run, testing.go:1944:
    //
    //     // TestMain may have already called flag.Parse.
    //     if !flag.Parsed() {
    //         flag.Parse()
    //     }
    //
    // and its own TestMain documentation says "call flag.Parse() here if TestMain uses flags".
    // So TestMain runs FIRST and owns the moment: it may install a flag.Usage and only then parse.
    // crypto/tls's converted TestMain is exactly that shape —
    // `flag.Usage = () => { …; if (bogoMode.Value) os.Exit(89); }; flag.Parse();` — and BoGo's
    // runner reads that 89 as errUnimplemented → SKIP.
    //
    // A host that parses before TestMain applies the DEFAULT Usage instead, which exits 2, and the
    // override never gets the chance to run. That is the same ordering defect as the host's old
    // hand-written unrecognized-flag verdict, one layer further down: something outside TestMain
    // deciding when the parse happens, for a package written to decide it itself. Measured
    // 2026-08-28 by i9: removing the verdict alone left the numbers byte-identical (1,340/1,902/0),
    // because TestFlagBridge.Parse() still ran unconditionally in Run() before RunTests reached
    // TestMain.
    //
    // This binds the real converted flag package (see the csproj note) because "has a parse
    // happened yet" is only observable there.

    // flag.Parsed() is PROCESS-global, and every TestHost.Run in this assembly reaches a parse, so
    // these two guards would otherwise report whatever an unrelated test did first — the order
    // dependence is in the observation, not in the host. Replacing flag.CommandLine with a fresh
    // FlagSet resets Parsed() to the state a real test binary starts in, which is what makes the
    // assertions mean anything regardless of execution order. (Found the honest way: adding the
    // fixture guards made this file fail without touching it.)
    private static void ResetConvertedFlagState()
    {
        flag_package.CommandLine = flag_package.NewFlagSet("guard", flag_package.ContinueOnError);
    }

    [TestMethod]
    public void TestMainRunsBeforeTheCommandLineIsParsed()
    {
        ResetConvertedFlagState();

        // Failing-first record (2026-08-28): flag.Parsed() was already TRUE here — the host had
        // parsed in Run() before RunTests invoked TestMain, so a Usage installed by this TestMain
        // could never apply to that parse.
        bool? parsedAtTestMainEntry = null;

        TestRegistry registry = new("guard", []);
        registry.SetTestMain(_ => parsedAtTestMainEntry = flag_package.Parsed());

        TestHost.Run(registry, []);

        Assert.IsNotNull(parsedAtTestMainEntry, "TestMain was never invoked, so the ordering could not be observed");
        Assert.IsFalse(parsedAtTestMainEntry!.Value,
            "the command line was already parsed when TestMain ran — the host parsed first, so a flag.Usage installed by TestMain (crypto/tls's os.Exit(89) override) can never apply");
    }

    [TestMethod]
    public void PackageWithoutTestMainStillGetsItsFlagsParsed()
    {
        // The other side of the same contract, and the reason the host's parse cannot simply be
        // deleted: Go's m.Run parses when TestMain did not. A package with custom test flags and NO
        // TestMain relies on exactly that — os/signal's TestDetectNohup re-exec recursion is the
        // corpus's witness, where an unparsed -check_sighup_ignored read false in the child and it
        // re-exec'd itself without bound. So the parse moves rather than disappears.
        ResetConvertedFlagState();

        TestRegistry registry = new("guard", []);

        TestHost.Run(registry, []);

        Assert.IsTrue(flag_package.Parsed(),
            "a package with no TestMain had its command line left unparsed — Go's m.Run parses in that case, and custom test flags would silently keep their defaults");
    }
}

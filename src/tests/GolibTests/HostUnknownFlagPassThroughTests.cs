using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class HostUnknownFlagPassThroughTests
{
    // THE INVARIANT: the HOST does not decide what an unrecognized flag means. Go's test binary
    // reaches exactly one flag.Parse(), and the package under test is entitled to have installed
    // its own flag.Usage by then — so an unknown name must reach the converted flag package and be
    // answered by GO'S contract, not short-circuited here.
    //
    // Why it matters beyond tidiness, measured 2026-08-28 on crypto/tls's BoGo shim: Go's
    // crypto/tls TestMain overrides flag.Usage to `if *bogoMode { os.Exit(89) }`, and the BoGo
    // runner reads exit 89 as errUnimplemented -> SKIP (runner.go:1685/20380). That is the shim's
    // DESIGNED grace path: Go's shim defines ~45 flags, the runner asks for ~100, and the ~55 it
    // has never heard of are meant to exit 89 and skip. The host used to answer first —
    // "flag provided but not defined: -<name>", return 2 — which the runner reads as a hard
    // failure. i9's run of the instrument turned 1,902 cases into FAILs on exactly that line.
    // The class is wider than BoGo: ANY package that installs a flag.Usage doing something other
    // than flag's default diverges the same way.
    //
    // These guards bind the converted testing host directly, the route
    // TestExecutionOutputCapTests established for a host that no _test.go can reach.
    //
    // ⚠ CORRECTED 2026-09-02. This comment used to read "GolibTests does not reference the
    // converted `flag` assembly, so TestFlagBridge's Type.GetType resolves null and its members
    // no-op". That was true when it was written and is FALSE now: the csproj gained
    // `core\flag\flag.csproj` for HostTestMainParseOrderTests, which needs the real package
    // because "has a parse happened yet" is only observable there. The premise was disarmed by a
    // legitimate change elsewhere — and unlike the usual shape of that failure, this one did not
    // go quietly vacuous. It took the whole suite down.
    //
    // MEASURED (2026-09-02, Linux): with the real package bound, TestHost.Run reached
    // TestFlagBridge.Parse -> flag.Parse(), which parses os.Args[1:] — the MSTEST TESTHOST's own
    // command line — against `flag.CommandLine = NewFlagSet(os.Args[0], ExitOnError)`. It rejected
    // `--port`, printed the -test.* set under "Usage of .../testhost.dll", and ExitOnError called
    // os.Exit(2). The host PROCESS died mid-suite: 82 reported, then `Test Run Aborted`.
    //
    // It was ORDER-DEPENDENT, which is the part worth remembering. flag.CommandLine is
    // process-global, and HostTestMainParseOrderTests replaces it with a ContinueOnError FlagSet
    // for its own reasons — so a runner that scheduled that class first inoculated the whole
    // process and saw a clean suite, while one that did not aborted. FIVE classes in this
    // assembly drive TestHost.Run and only two owned flag.CommandLine, so which of them tripped
    // it was luck: a suite whose completion depends on class order is a false green waiting for
    // its turn.
    //
    // FIXED IN THE HOST, not here, and in the same commit as this note: TestHost.Run now announces
    // its own `args` (TestFlagBridge.HostCommandLine) and the bridge parses THOSE rather than the
    // ambient process command line. In a real converted test binary the two are the same array —
    // go2cs_test_host.cs is `Main(string[] args) => TestHost.Run(registry, args)` — so the change
    // is inert in production and retires the class of hazard rather than the three classes that
    // happened to lack a reset. Measured after: the suite completes, 458 passed / 2 failed / 1
    // skipped of 461, and BOTH failures reproduce identically with the fix stashed, so the abort
    // had been hiding two pre-existing Linux-flavor defects.

    // AND THE HALF THE HOST FIX DOES NOT COVER, measured the same day by running this class
    // ALONE with the host fix in place — it still took the process down. The two are
    // complementary, not alternatives. The host fix stops the RUNNER's command line reaching a
    // parse; it cannot stop THIS class, which exists to drive a deliberate unrecognized flag
    // through the converted package, from meeting Go's own ExitOnError contract — and Go's
    // contract for an undefined flag on the default CommandLine is os.Exit(2), which is correct
    // in a real test binary and fatal in-process. So the guard must OWN the FlagSet it drives,
    // exactly as HostTestMainParseOrderTests already does and for the same process-global reason.
    // ContinueOnError is what makes the two assertions expressible at all: both ask what the HOST
    // decides — whether it answers an unrecognized flag itself, and whether execution REACHES the
    // stage where the converted flag package parses — and neither asks anything of flag's
    // error-handling mode.
    // The original is put back after each test: flag.CommandLine is process-global and shared by every
    // other in-process class (i9, 2026-09-27: the shared-CommandLine class).
    private ж<flag_package.FlagSet>? m_originalCommandLine;

    [TestInitialize]
    public void OwnTheConvertedFlagSet()
    {
        m_originalCommandLine = flag_package.CommandLine;
        flag_package.CommandLine = flag_package.NewFlagSet("guard", flag_package.ContinueOnError);
    }

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        if (m_originalCommandLine is not null)
            flag_package.CommandLine = m_originalCommandLine;
    }

    private static TestRegistry EmptyRegistry() => new("guard", []);

    [TestMethod]
    public void UnknownFlagIsNotRejectedByTheHost()
    {
        // Failing-first record (2026-08-28): this returned 2 — the host's own verdict — before the
        // pass-through landed. Go's exit code for a bad command line is also 2, which is precisely
        // why the divergence hid: the CODE matched while the DECISION was taken in the wrong place
        // and too early, so the package's Usage never ran.
        int exitCode = TestHost.Run(EmptyRegistry(), ["-no-such-flag-anywhere"]);

        Assert.AreNotEqual(2, exitCode,
            "the host answered an unrecognized flag itself; it must defer to the converted flag package so the package's own flag.Usage (crypto/tls's os.Exit(89) among them) can run");
    }

    [TestMethod]
    public void UnknownFlagStillReachesTheRun()
    {
        // The other half, and the one that actually enables the 89: not merely "no early return"
        // but that execution REACHES the stage where the converted flag package parses. Observed
        // through the host's own output: a run that got that far reports a terminal verdict.
        // Failing-first this captured the EMPTY string — the host had already returned 2 above it.
        StringWriter captured = new();
        TextWriter previous = Console.Out;

        try
        {
            Console.SetOut(captured);
            TestHost.Run(EmptyRegistry(), ["-no-such-flag-anywhere"]);
        }
        finally
        {
            Console.SetOut(previous);
        }

        StringAssert.Contains(captured.ToString(), "PASS",
            "the run never reached the package stage with an unrecognized flag present, so a converted flag.Parse — and any flag.Usage the package installed — could not have run");
    }
}

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// H1: -test.list LISTS and exits. It never runs a test.
//
// Go's M.Run, after the flag parse (testing.go):
//
//     if *matchList != "" {
//         listTests(m.deps.MatchString, m.tests, m.benchmarks, m.fuzzTargets, m.examples)
//         m.exitCode = 0
//         return
//     }
//
// listTests prints each matching name on its own line (fmt.Println) and an invalid pattern is
// "testing: invalid regexp in -test.list (%q): %s" on stderr with status 1.
//
// WHAT IT GUARDS. The host registered the flag on the converted flag package (TestFlagBridge) but
// never acted on it, so `-test.list=^$` ran the whole suite. x/sync/singleflight's executable(t)
// spawns its own binary with exactly that argument as a control case before re-executing the
// panic tests; the control child ran the suite, its panic tests spawned controls of their own, and
// the process tree doubled each generation (measured 2026-10-02, Linux: TestPanicDoChan and
// TestPanicDoSharedByDoChan read C#="" against Go's pass).
[TestClass]
public class HostTestListTests
{
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

    private static (int ExitCode, string Stdout, string Stderr, int Ran) RunHost(params string[] args)
    {
        int ran = 0;

        TestRegistry registry = new("guard", []);
        // Registered out of Go's order, so the listing must take the run's order, not the registry's.
        registry.Add("TestSecond", _ => ran++, "a_test.go", 20);
        registry.Add("TestFirst", _ => ran++, "a_test.go", 10);

        StringWriter stdout = new();
        StringWriter stderr = new();
        TextWriter previousOut = Console.Out;
        TextWriter previousError = Console.Error;
        int exitCode;

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            exitCode = TestHost.Run(registry, args);
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }

        return (exitCode, stdout.ToString(), stderr.ToString(), ran);
    }

    [TestMethod]
    public void AnEmptyMatchListsNothingAndRunsNothing()
    {
        // singleflight's control case, verbatim: exec.Command(exe, "-test.list=^$").
        (int exitCode, string stdout, _, int ran) = RunHost("-test.list=^$");

        Assert.AreEqual(0, ran, $"-test.list ran {ran} test(s); Go lists and exits without running any");
        Assert.AreEqual(0, exitCode, "Go's -test.list exits 0");
        Assert.AreEqual("", stdout, $"nothing matches ^$, so Go prints nothing; the host wrote {Escape(stdout)}");
    }

    [TestMethod]
    public void MatchingNamesAreListedOnePerLineInRunOrder()
    {
        (int exitCode, string stdout, _, int ran) = RunHost("-test.list=Test");

        Assert.AreEqual(0, ran, $"-test.list ran {ran} test(s)");
        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("TestFirst\nTestSecond\n", stdout, $"wrote {Escape(stdout)}");
    }

    [TestMethod]
    public void TheValueMayBeTheNextArgument()
    {
        (int exitCode, string stdout, _, int ran) = RunHost("-test.list", "Second$");

        Assert.AreEqual(0, ran, $"-test.list ran {ran} test(s)");
        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("TestSecond\n", stdout, $"wrote {Escape(stdout)}");
    }

    [TestMethod]
    public void AnInvalidPatternIsGosErrorAndStatusOne()
    {
        (int exitCode, string stdout, string stderr, int ran) = RunHost("-test.list=(");

        Assert.AreEqual(0, ran, $"-test.list ran {ran} test(s)");
        Assert.AreEqual(1, exitCode, "Go exits 1 on an invalid -test.list pattern");
        Assert.AreEqual("", stdout, $"wrote {Escape(stdout)}");
        StringAssert.StartsWith(stderr, "testing: invalid regexp in -test.list (\"(\"): ", $"stderr was {Escape(stderr)}");
    }

    private static string Escape(string text) => "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
}

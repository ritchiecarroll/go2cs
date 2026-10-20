using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The host's parse steps past every Go <c>-test.*</c> flag it defines, so a host flag after one still
/// takes effect, as in Go's one flag set.
/// </summary>
/// <remarks>
/// Failing-first record (go1.26.10): testing's <c>runTest</c> re-execs the binary as
/// <c>-test.run=^X$ -test.bench=X -test.v -test.parallel=2 -test.benchtime=2x</c>. The parse stopped at
/// <c>-test.bench</c>, so <c>-test.v</c> was lost, and TestAttrSet and the four TestArtifactDir* parents
/// read a child that printed only "PASS".
/// </remarks>
[TestClass]
public class HostGoTestFlagArityTests
{
    private ж<flag_package.FlagSet>? m_originalCommandLine;

    [TestInitialize]
    public void CaptureTheCommandLine() => m_originalCommandLine = flag_package.CommandLine;

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        if (m_originalCommandLine is not null)
            flag_package.CommandLine = m_originalCommandLine;
    }

    [TestMethod]
    public void RunTestsArgvKeepsTheVerboseFlagAfterBench()
    {
        TestOptions options = TestOptions.Parse(
            ["-test.run=^TestAttrExample$", "-test.bench=TestAttrExample", "-test.v", "-test.parallel=2", "-test.benchtime=2x"]);

        Assert.IsNull(options.UnrecognizedFlag, "-test.bench and -test.benchtime are Go's own flags, defined by this host");
        Assert.IsTrue(options.Verbose, "-test.v after -test.bench was lost: the parse stopped at -test.bench");
        Assert.AreEqual(2, options.Parallel);
        Assert.AreEqual("^TestAttrExample$", options.RunPattern);
    }

    [TestMethod]
    public void AValueFlagSpelledApartTakesTheNextArgumentAndABoolFlagDoesNot()
    {
        TestOptions valued = TestOptions.Parse(["-test.bench", "X", "-test.v"]);

        Assert.IsNull(valued.UnrecognizedFlag);
        Assert.IsTrue(valued.Verbose, "-test.bench X: X is the flag's value, and -test.v after it must still be read");

        // A bool flag never takes the next argument (Go's flag: `-flag x` leaves x as a program argument).
        TestOptions boolean = TestOptions.Parse(["-test.failfast", "-test.v"]);

        Assert.IsNull(boolean.UnrecognizedFlag);
        Assert.IsTrue(boolean.Verbose, "-test.failfast swallowed the -test.v after it as a value");
    }

    [TestMethod]
    public void EveryTestFlagTheHostDefinesIsSteppedPast()
    {
        flag_package.CommandLine = flag_package.NewFlagSet((@string)"guard", flag_package.ContinueOnError);
        TestFlagBridge.Register(TestOptions.Parse([]));

        List<(string Name, string DefValue)> defined = [];

        flag_package.CommandLine.Value.VisitAll(flag =>
        {
            string name = (~flag).Name.ToString();

            if (name.StartsWith("test.", System.StringComparison.Ordinal))
                defined.Add((name, (~flag).DefValue.ToString()));
        });

        // Go 1.26 defines 34: testing.Init's 26, initBenchmarkFlags' 3 and initFuzzFlags' 5.
        Assert.AreEqual(34, defined.Count, "the bridge's -test.* set is not Go 1.26's 34; the guard is not looking at Go's set");

        foreach ((string name, string defValue) in defined)
        {
            TestOptions options = TestOptions.Parse([$"-{name}={defValue}", "-test.v"]);

            Assert.IsNull(options.UnrecognizedFlag, $"-{name} is defined by the host but stops its parse");
            Assert.IsTrue(options.Verbose, $"-test.v after -{name} was lost");
        }
    }
}

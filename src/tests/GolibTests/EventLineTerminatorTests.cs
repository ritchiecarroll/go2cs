using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// D5: the host ends each event line with a bare "\n", as Go's test binary does on every OS.
//
// WHAT IT GUARDS. A Go test that re-executes its own binary reads the child's output back, and Go writes "\n" on
// every OS. runtime's TestFinalizerRegisterABI (abi_test.go) and TestArenaCollision (malloc_test.go) run
// themselves with -test.v and require strings.Contains(out, "PASS\n"). The host ended its lines with the console
// writer's NewLine, "\r\n" on windows, so a child that PASSED printed "PASS\r\n" and the parent failed it:
// "PASS ... (exit status <nil>)". golib's own output already writes a bare "\n" (println, the fatal and crash
// reports) for the same reason.
//
// HOW. Both branches of TestReporter.WriteEventLine: the process console (bytes to the process's stdout) and a
// replaced Console.Out (an in-process capture). Each runs under a writer whose NewLine is "\r\n", a windows
// console's, so the guard holds on every OS rather than only where the platform default is CRLF.
[TestClass]
public class EventLineTerminatorTests
{
    private static string ReportPassSummary(bool processConsole, out string captured)
    {
        MemoryStream stdout = new();
        StringWriter console = new() { NewLine = "\r\n" };
        TextWriter previous = Console.Out;

        Console.SetOut(console);
        TestReporter.ProcessConsoleForGuard = processConsole;
        TestReporter.StdoutForGuard = stdout;

        try
        {
            // A package-level pass with nothing to say: the summary line a -test.v child ends with.
            new TestReporter("guard", json: false, verbose: true).Report(new TestEvent("guard", "", "pass"));
        }
        finally
        {
            TestReporter.ProcessConsoleForGuard = null;
            TestReporter.StdoutForGuard = null;
            Console.SetOut(previous);
        }

        captured = console.ToString();
        return Console.OutputEncoding.GetString(stdout.ToArray());
    }

    [TestMethod]
    public void TheProcessConsoleSummaryLineEndsInABareNewline()
    {
        string written = ReportPassSummary(processConsole: true, out _);

        Assert.AreEqual("PASS\n", written, $"the host's summary line must end in \"\\n\" on every OS, as Go's does; wrote {Escape(written)}");
    }

    [TestMethod]
    public void ARedirectedConsoleOutGetsABareNewlineToo()
    {
        ReportPassSummary(processConsole: false, out string captured);

        Assert.AreEqual("PASS\n", captured, $"a replaced Console.Out must receive the same \"\\n\"-terminated line; got {Escape(captured)}");
    }

    // The reading Go's own tests take of a re-executed child, verbatim (abi_test.go, malloc_test.go).
    [TestMethod]
    public void AChildThatPassedReadsAsPassedToGosCheck()
    {
        string written = ReportPassSummary(processConsole: true, out _);

        Assert.IsTrue(written.Contains("PASS\n", StringComparison.Ordinal), $"strings.Contains(out, \"PASS\\n\") fails on {Escape(written)}");
    }

    private static string Escape(string text) => "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
}

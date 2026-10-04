using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// The host's package deadline NAMES the tests it interrupted.
//
// Go's test binary, on its -timeout, panics with "test timed out after <d>" and then lists the tests still running
// ("running tests:"), so a hung or spinning suite points at its own culprit. The host's package-timeout event said
// only "package timeout after <d>": a suite that spun until its deadline (a recursion the runtime never overflowed,
// a goroutine nobody unblocks) left the reader to guess which test had held it. The event's text keeps its prefix --
// the comparison classifier quotes it by that prefix -- and gains the running tests' names.
[TestClass]
public class HostPackageTimeoutRunningTestsTests
{
    // flag.CommandLine is process-global and shared by every in-process class: put the original back after each test.
    private ж<flag_package.FlagSet>? m_originalCommandLine;

    // The blocked test's release, so its thread ends with the test rather than outliving the run.
    private static readonly ManualResetEventSlim s_release = new(false);

    [TestInitialize]
    public void OwnTheConvertedFlagSet()
    {
        m_originalCommandLine = flag_package.CommandLine;
        flag_package.CommandLine = flag_package.NewFlagSet("guard", flag_package.ContinueOnError);
        s_release.Reset();
    }

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        s_release.Set();

        if (m_originalCommandLine is not null)
            flag_package.CommandLine = m_originalCommandLine;
    }

    [TestMethod]
    public void ThePackageTimeoutEventNamesTheTestsStillRunning()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestFinished", _ => { }, "a_test.go", 5);
        registry.Add("TestBlocks", _ => s_release.Wait(), "a_test.go", 10);

        StringWriter stdout = new();
        StringWriter stderr = new();
        TextWriter previousOut = Console.Out;
        TextWriter previousError = Console.Error;
        int exitCode;

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            exitCode = TestHost.Run(registry, ["--json", "-timeout", "1s"]);
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }

        string? timeoutLine = stdout.ToString().Split('\n').FirstOrDefault(line => line.Contains("\"action\":\"timeout\""));

        Assert.AreEqual(1, exitCode, "a package timeout exits 1");
        Assert.IsNotNull(timeoutLine, $"no package timeout event was written; stdout:\n{stdout}");
        StringAssert.Contains(timeoutLine, "package timeout after 00:00:01", "the event keeps the prefix the classifier quotes");
        StringAssert.Contains(timeoutLine, "TestBlocks", "TestBlocks was running at the deadline and the event does not name it");
        Assert.IsFalse(timeoutLine.Contains("TestFinished"), $"TestFinished passed before the deadline and must not be named: {timeoutLine}");
    }
}

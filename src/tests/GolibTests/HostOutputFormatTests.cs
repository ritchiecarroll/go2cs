using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The host's plain-text output is Go's test binary's (COORD ruling 2026-10-17, the output-format seat): every log
/// record is DECORATED with its call site as Go's <c>(*common).callSite</c> spells it (the file's base name and line,
/// helpers skipped as <c>frameSkip</c> skips them), and the text mode prints what Go's prints: nothing for a passing
/// or skipped test unless <c>-test.v</c>, a failure as <c>--- FAIL: Name (d.dds)</c> with its records indented four
/// spaces, chatty <c>=== RUN</c> lines under <c>-test.v</c>, then the bare <c>PASS</c> or <c>FAIL</c>.
/// </summary>
/// <remarks>
/// The witnesses are testing/synctest's TestError, TestFatal, TestHelper, TestSkip, TestVerboseError and
/// TestVerboseSkip, which re-run their own binary and match its WHOLE output against anchored patterns
/// (go1.26.9 synctest_test.go:25-153). These arms pin the same shapes in-process. A C# test body has no Go
/// position map, so its call site is its honest C# position: this file's name and the line of the call.
/// </remarks>
[TestClass]
public class HostOutputFormatTests
{
    private const string ThisFile = "HostOutputFormatTests.cs";

    private static int Line([CallerLineNumber] int line = 0) => line;

    [TestMethod]
    public void AQuietRunPrintsOnlyTheFailureThenFail()
    {
        int line = 0;
        TestRegistry registry = new("guard", []);
        registry.Add("TestPass", t => t.Value.Log("quiet"), "a_test.go", 10);
        registry.Add("TestFail", t => { line = Line(); t.Value.Error("boom"); }, "a_test.go", 20);

        (int exit, string output) = Run(registry, "-test.run=^Test(Pass|Fail)$");

        Assert.AreEqual(1, exit);
        StringAssert.Matches(output, new Regex($@"\A--- FAIL: TestFail \(\d+\.\d\ds\)\n    {Regex.Escape(ThisFile)}:{line}: boom\nFAIL\n\z"));
    }

    [TestMethod]
    public void AQuietSkipPrintsOnlyPass()
    {
        TestRegistry registry = new("guard", []);
        registry.Add("TestSkip", t => t.Value.Skip("skip"), "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.run=^TestSkip$");

        Assert.AreEqual(0, exit);
        Assert.AreEqual("PASS\n", output);
    }

    [TestMethod]
    public void AVerboseRunPrintsRunLinesRecordsAsTheyHappenAndVerdicts()
    {
        int line = 0;
        TestRegistry registry = new("guard", []);
        registry.Add("TestVerboseError", t => { line = Line(); t.Value.Error("error"); }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestVerboseError$");

        Assert.AreEqual(1, exit);
        StringAssert.Matches(output, new Regex(
            $@"\A=== RUN   TestVerboseError\n    {Regex.Escape(ThisFile)}:{line}: error\n--- FAIL: TestVerboseError \(\d+\.\d\ds\)\nFAIL\n\z"));
    }

    [TestMethod]
    public void AVerboseSkipPrintsItsDecoratedReasonAndPass()
    {
        int line = 0;
        TestRegistry registry = new("guard", []);
        registry.Add("TestVerboseSkip", t => { line = Line(); t.Value.Skip("skip"); }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestVerboseSkip$");

        Assert.AreEqual(0, exit);
        StringAssert.Matches(output, new Regex(
            $@"\A=== RUN   TestVerboseSkip\n    {Regex.Escape(ThisFile)}:{line}: skip\n--- SKIP: TestVerboseSkip \(\d+\.\d\ds\)\nPASS\n\z"));
    }

    [TestMethod]
    public void AHelperIsSkippedForItsCaller()
    {
        int line = 0;
        TestRegistry registry = new("guard", []);
        registry.Add("TestHelper", t => { line = Line(); HelperLog(t, "log in helper"); }, "a_test.go", 10);

        (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestHelper$");

        Assert.AreEqual(0, exit);
        StringAssert.Matches(output, new Regex(
            $@"\A=== RUN   TestHelper\n    {Regex.Escape(ThisFile)}:{line}: log in helper\n--- PASS: TestHelper \(\d+\.\d\ds\)\nPASS\n\z"));
    }

    [TestMethod]
    public void ALaterLineOfARecordIsIndentedFourMore()
    {
        int line = 0;
        TestRegistry registry = new("guard", []);
        registry.Add("TestLines", t => { line = Line(); t.Value.Error("first\nsecond"); }, "a_test.go", 10);

        (_, string output) = Run(registry, "-test.run=^TestLines$");

        StringAssert.Matches(output, new Regex(
            $@"\A--- FAIL: TestLines \(\d+\.\d\ds\)\n    {Regex.Escape(ThisFile)}:{line}: first\n        second\nFAIL\n\z"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HelperLog(ж<testing_package.T> t, string message)
    {
        t.Value.Helper();
        t.Value.Log(message);
    }

    private static (int exit, string output) Run(TestRegistry registry, params string[] args)
    {
        TextWriter previousOut = Console.Out;
        StringWriter captured = new();
        int exit;

        try
        {
            Console.SetOut(captured);
            exit = TestHost.Run(registry, args);
        }
        finally
        {
            Console.SetOut(previousOut);
        }

        return (exit, captured.ToString().Replace("\r\n", "\n"));
    }
}

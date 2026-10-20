using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests
{
    /// <summary>
    /// The host's plain-text output is Go's test binary's (COORD ruling 2026-10-17, the output-format seat): every log
    /// record is DECORATED with its call site as Go's <c>(*common).callSite</c> spells it (the file's base name and
    /// line, helpers skipped as <c>frameSkip</c> skips them), and the text mode prints what Go's prints: nothing for a
    /// passing or skipped test unless <c>-test.v</c>, a failure as <c>--- FAIL: Name (d.dds)</c> with its records
    /// indented four spaces, chatty <c>=== RUN</c> lines under <c>-test.v</c>, then the bare <c>PASS</c> or <c>FAIL</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The witnesses are testing/synctest's TestError, TestFatal, TestHelper, TestSkip, TestVerboseError and
    /// TestVerboseSkip, which re-run their own binary and match its WHOLE output against anchored patterns
    /// (go1.26.9 synctest_test.go:25-153). These arms pin the same shapes in-process.
    /// </para>
    /// <para>
    /// The test bodies live in <see cref="go.hostfmtguard.hostfmtguard_test_package"/>, declared as the converter
    /// declares a test package, because a call site is a GO frame: runtime.Callers reports only frames the Go source
    /// declares, so a C# lambda here would be invisible to it. That class has no Go position map, so its frames report
    /// their honest C# position: this file's name and the line of the call.
    /// </para>
    /// </remarks>
    [TestClass]
    public class HostOutputFormatTests
    {
        private const string ThisFile = "HostOutputFormatTests.cs";

        private static int Line => go.hostfmtguard.hostfmtguard_test_package.CallLine;

        [TestMethod]
        public void AQuietRunPrintsOnlyTheFailureThenFail()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestPass", go.hostfmtguard.hostfmtguard_test_package.TestPass, "a_test.go", 10);
            registry.Add("TestFail", go.hostfmtguard.hostfmtguard_test_package.TestFail, "a_test.go", 20);

            (int exit, string output) = Run(registry, "-test.run=^Test(Pass|Fail)$");

            Assert.AreEqual(1, exit);
            StringAssert.Matches(output, new Regex($@"\A--- FAIL: TestFail \(\d+\.\d\ds\)\n    {Regex.Escape(ThisFile)}:{Line}: boom\nFAIL\n\z"));
        }

        [TestMethod]
        public void AQuietSkipPrintsOnlyPass()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestSkip", go.hostfmtguard.hostfmtguard_test_package.TestSkip, "a_test.go", 10);

            (int exit, string output) = Run(registry, "-test.run=^TestSkip$");

            Assert.AreEqual(0, exit);
            Assert.AreEqual("PASS\n", output);
        }

        [TestMethod]
        public void AVerboseRunPrintsRunLinesRecordsAsTheyHappenAndVerdicts()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestVerboseError", go.hostfmtguard.hostfmtguard_test_package.TestVerboseError, "a_test.go", 10);

            (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestVerboseError$");

            Assert.AreEqual(1, exit);
            StringAssert.Matches(output, new Regex(
                $@"\A=== RUN   TestVerboseError\n    {Regex.Escape(ThisFile)}:{Line}: error\n--- FAIL: TestVerboseError \(\d+\.\d\ds\)\nFAIL\n\z"));
        }

        [TestMethod]
        public void AVerboseSkipPrintsItsDecoratedReasonAndPass()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestVerboseSkip", go.hostfmtguard.hostfmtguard_test_package.TestVerboseSkip, "a_test.go", 10);

            (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestVerboseSkip$");

            Assert.AreEqual(0, exit);
            StringAssert.Matches(output, new Regex(
                $@"\A=== RUN   TestVerboseSkip\n    {Regex.Escape(ThisFile)}:{Line}: skip\n--- SKIP: TestVerboseSkip \(\d+\.\d\ds\)\nPASS\n\z"));
        }

        [TestMethod]
        public void AHelperIsSkippedForItsCaller()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestHelper", go.hostfmtguard.hostfmtguard_test_package.TestHelper, "a_test.go", 10);

            (int exit, string output) = Run(registry, "-test.v", "-test.run=^TestHelper$");

            Assert.AreEqual(0, exit);
            StringAssert.Matches(output, new Regex(
                $@"\A=== RUN   TestHelper\n    {Regex.Escape(ThisFile)}:{Line}: log in helper\n--- PASS: TestHelper \(\d+\.\d\ds\)\nPASS\n\z"));
        }

        [TestMethod]
        public void ALaterLineOfARecordIsIndentedFourMore()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestLines", go.hostfmtguard.hostfmtguard_test_package.TestLines, "a_test.go", 10);

            (_, string output) = Run(registry, "-test.run=^TestLines$");

            StringAssert.Matches(output, new Regex(
                $@"\A--- FAIL: TestLines \(\d+\.\d\ds\)\n    {Regex.Escape(ThisFile)}:{Line}: first\n        second\nFAIL\n\z"));
        }

        // Go's TestTBHelper cases 9 and 10: a record a HELPER logs inside a cleanup is attributed to the line that called
        // t.Cleanup (frameSkip walks on into the stack Cleanup kept, cleanupPc), not to the cleanup's own line.
        [TestMethod]
        public void AHelperInACleanupIsAttributedToTheCleanupCall()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestCleanupHelper", go.hostfmtguard.hostfmtguard_test_package.TestCleanupHelper, "a_test.go", 10);

            (_, string output) = Run(registry, "-test.run=^TestCleanupHelper$");

            StringAssert.Matches(output, new Regex(
                $@"\A--- FAIL: TestCleanupHelper \(\d+\.\d\ds\)\n    {Regex.Escape(ThisFile)}:{Line}: in cleanup\nFAIL\n\z"));
        }

        // G's PIN 1 (review of 1d1c02bb3e): callSite's fixed skip is by NAME here (past runtime.Callers, then every frame of
        // the ONE testing API function entered), where Go counts three frames. The two agree only while each public entry
        // reaches the host with no other Go-named testing frame between, so every public logging entry is pinned to its
        // caller's line, under -test.v so a passing entry's record prints too.
        [DataTestMethod]
        [DataRow("TestEntryLog", "entry Log")]
        [DataRow("TestEntryLogf", "entry Logf")]
        [DataRow("TestEntryError", "entry Error")]
        [DataRow("TestEntryErrorf", "entry Errorf")]
        [DataRow("TestEntryFatal", "entry Fatal")]
        [DataRow("TestEntryFatalf", "entry Fatalf")]
        [DataRow("TestEntrySkip", "entry Skip")]
        [DataRow("TestEntrySkipf", "entry Skipf")]
        public void EachLoggingEntryNamesItsCallersLine(string test, string message)
        {
            TestRegistry registry = new("guard", []);
            registry.Add(test, EntryBody(test), "a_test.go", 10);

            (_, string output) = Run(registry, "-test.v", $"-test.run=^{test}$");

            StringAssert.Contains(output, $"\n    {ThisFile}:{Line}: {message}\n", $"{test}: the record must name its caller's line; output:\n{output}");
        }

        // Attr's refusal is logged through Errorf FROM Attr, so Go's fixed skip stops at Attr's own frame (testing.go:1609
        // in Go); the host's Attr frame is testing.cs, never testing.go (the 2026-08-26 ruling's letter).
        [TestMethod]
        public void AttrsRefusalNamesAttrsOwnFrame()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestEntryAttr", go.hostfmtguard.hostfmtguard_test_package.TestEntryAttr, "a_test.go", 10);

            (_, string output) = Run(registry, "-test.run=^TestEntryAttr$");

            StringAssert.Matches(output, new Regex(@"\n    testing\.cs:\d+: disallowed whitespace in attribute key ""bad key""\n"),
                $"output:\n{output}");
        }

        // G's PIN 2: Go prints t.Log("two\nlines\n") as "f.go:12: two" / "    lines" / "    " inside the test's indent:
        // Sprintln's newline plus the text's own leaves ONE empty continuation line, indented eight.
        [TestMethod]
        public void ARecordEndingInANewlineKeepsOneEmptyContinuationLine()
        {
            TestRegistry registry = new("guard", []);
            registry.Add("TestTrailingNewline", go.hostfmtguard.hostfmtguard_test_package.TestTrailingNewline, "a_test.go", 10);

            (_, string output) = Run(registry, "-test.v", "-test.run=^TestTrailingNewline$");

            StringAssert.Matches(output, new Regex(
                $@"\A=== RUN   TestTrailingNewline\n    {Regex.Escape(ThisFile)}:{Line}: two\n        lines\n        \n--- PASS: TestTrailingNewline \(\d+\.\d\ds\)\nPASS\n\z"),
                $"output:\n{output}");
        }

        private static Action<ж<testing_package.T>> EntryBody(string test) => test switch
        {
            "TestEntryLog" => go.hostfmtguard.hostfmtguard_test_package.TestEntryLog,
            "TestEntryLogf" => go.hostfmtguard.hostfmtguard_test_package.TestEntryLogf,
            "TestEntryError" => go.hostfmtguard.hostfmtguard_test_package.TestEntryError,
            "TestEntryErrorf" => go.hostfmtguard.hostfmtguard_test_package.TestEntryErrorf,
            "TestEntryFatal" => go.hostfmtguard.hostfmtguard_test_package.TestEntryFatal,
            "TestEntryFatalf" => go.hostfmtguard.hostfmtguard_test_package.TestEntryFatalf,
            "TestEntrySkip" => go.hostfmtguard.hostfmtguard_test_package.TestEntrySkip,
            "TestEntrySkipf" => go.hostfmtguard.hostfmtguard_test_package.TestEntrySkipf,
            _ => throw new ArgumentOutOfRangeException(nameof(test), test, null)
        };

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
}

// The test bodies, declared as the converter declares a test package (namespace under `go`, a top-level type whose
// name ends in `_package`), so runtime.Callers counts their frames as Go frames. Each records the line of its own
// logging call, written on the SAME line, for the arm to expect.
namespace go.hostfmtguard
{
    public static class hostfmtguard_test_package
    {
        internal static int CallLine;

        private static int At([CallerLineNumber] int line = 0) => CallLine = line;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestPass(ж<testing_package.T> t) { t.Log("quiet"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestFail(ж<testing_package.T> t) { At(); t.Error("boom"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestSkip(ж<testing_package.T> t) { At(); t.Skip("skip"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestVerboseError(ж<testing_package.T> t) { At(); t.Error("error"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestVerboseSkip(ж<testing_package.T> t) { At(); t.Skip("skip"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestHelper(ж<testing_package.T> t) { At(); helperLog(t, "log in helper"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestLines(ж<testing_package.T> t) { At(); t.Error("first\nsecond"); }

        // The cleanup's t.Error is on its OWN line, apart from the t.Cleanup call, so the arm tells the two apart.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestCleanupHelper(ж<testing_package.T> t)
        {
            Action cleanup = () =>
            {
                t.Helper();
                t.Error("in cleanup");
            };

            At(); t.Cleanup(cleanup);
        }

        // PIN 1's bodies: one per public logging entry, each called on the line At() records.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryLog(ж<testing_package.T> t) { At(); t.Log("entry Log"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryLogf(ж<testing_package.T> t) { At(); t.Logf("entry %s", "Logf"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryError(ж<testing_package.T> t) { At(); t.Error("entry Error"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryErrorf(ж<testing_package.T> t) { At(); t.Errorf("entry %s", "Errorf"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryFatal(ж<testing_package.T> t) { At(); t.Fatal("entry Fatal"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryFatalf(ж<testing_package.T> t) { At(); t.Fatalf("entry %s", "Fatalf"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntrySkip(ж<testing_package.T> t) { At(); t.Skip("entry Skip"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntrySkipf(ж<testing_package.T> t) { At(); t.Skipf("entry %s", "Skipf"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestEntryAttr(ж<testing_package.T> t) { t.Value.Attr("bad key", "v"); }

        // PIN 2's body: Go's Sprintln adds a newline after the text's own trailing one.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestTrailingNewline(ж<testing_package.T> t) { At(); t.Log("two\nlines\n"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void helperLog(ж<testing_package.T> t, string message)
        {
            t.Helper();
            t.Log(message);
        }
    }
}

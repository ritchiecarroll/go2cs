using System;
using System.IO;
using System.Linq;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class AttrOutputArtifactDirTests
{
    // WHY THIS EXISTS.
    //
    // Go 1.25 added (*common).Attr and (*common).Output, and Go 1.26 added (*common).ArtifactDir
    // (api/go1.25.txt, api/go1.26.txt); TB gains all three, so T, B and F carry them. The host is
    // hand-owned, so nothing reaches it from a regeneration (runbook H4, "the hand-owned test host").
    // Each reading below is Go 1.26.9's own, from src/testing/testing.go:
    //
    //   Attr        (:1604) Errorf on a key holding Unicode whitespace or a value holding \r or \n;
    //               otherwise "=== ATTR  name key value" under chatty, which test2json turns into an
    //               "attr" event with Key and Value (cmd/internal/test2json/test2json.go:337-340).
    //   Output      (:1114) a line-buffered writer onto the same log as Log: a complete line is a
    //               record, a trailing partial line waits for the next Write, for Log (flushPartial,
    //               :1096) or for the end of the test. "Output called after NAME has completed" and
    //               "Write called after NAME has completed" once the test and its parents are done.
    //   ArtifactDir (:1333) one directory per test, the same on every call: a temporary directory that
    //               is removed with the test, or, with -test.artifacts, a directory under
    //               <outputdir>/_artifacts that is kept and reported as an "artifacts" event.
    //
    // Every reading was checked against go1.26.9 itself on 2026-10-14 (a probe module run with -v, -json,
    // -artifacts and -outputdir): the ATTR line, the artifacts and attr JSON events, the Output records
    // a, b, c, d in that order, and the _artifacts/<random> layout.
    //
    // The host has no _test.go for these, so the guard binds the converted package directly. MSTest runs
    // this assembly serially (no [Parallelize]).

    private static (TestReporter reporter, TestRunner runner) Run(TestRegistry registry, params string[] args)
    {
        TestOptions options = TestOptions.Parse(args);
        TestReporter reporter = new("attrguard", json: false, verbose: options.Verbose);
        TestRunner runner = new(registry, options, reporter, ".", Path.GetTempPath());
        runner.RunAll();
        return (reporter, runner);
    }

    private static TestEvent Terminal(TestReporter reporter, string name) =>
        reporter.Events.Single(e => e.Test == name && e.Action is "pass" or "fail" or "skip");

    [TestMethod]
    public void AttrRefusesWhitespaceKeysAndNewlineValuesWithGosTexts()
    {
        TestRegistry registry = new("attrguard", []);
        registry.Add("TestAttrRefused", static t =>
        {
            t.Value.Attr("bad key", "v");
            t.Value.Attr("key", "two\nlines");
        }, "attr_guard_test.go", 1);

        (TestReporter reporter, _) = Run(registry);
        TestEvent terminal = Terminal(reporter, "TestAttrRefused");

        Assert.AreEqual("fail", terminal.Action, "Attr's refusals are Errorf, so the test fails");
        CollectionAssert.Contains(terminal.Records!.ToList(), "disallowed whitespace in attribute key \"bad key\"");
        CollectionAssert.Contains(terminal.Records!.ToList(), "disallowed newline in attribute value \"two\\nlines\"");
    }

    [TestMethod]
    public void AttrReportsAnAttrEventOnlyWhenChatty()
    {
        TestRegistry registry = new("attrguard", []);
        registry.Add("TestAttrReported", static t => t.Value.Attr("owner", "lane R"), "attr_guard_test.go", 2);

        (TestReporter verbose, _) = Run(registry, "-test.v");
        TestEvent attr = verbose.Events.Single(e => e.Action == "attr");

        Assert.AreEqual("TestAttrReported", attr.Test);
        Assert.AreEqual("owner", attr.Key);
        Assert.AreEqual("lane R", attr.Value, "test2json cuts the key at the first space; the rest is the value");
        Assert.AreEqual("pass", Terminal(verbose, "TestAttrReported").Action);

        (TestReporter quiet, _) = Run(registry);
        Assert.IsFalse(quiet.Events.Any(e => e.Action == "attr"), "without chatty output Go prints no ATTR line");
    }

    private static io_package.Writer? s_writer;
    private static ж<testing_package.T>? s_t;

    [TestMethod]
    public void OutputBuffersAPartialLineUntilLogOrTheEndOfTheTest()
    {
        TestRegistry registry = new("attrguard", []);
        registry.Add("TestOutputLines", static t =>
        {
            io_package.Writer w = t.Value.Output();
            w.Write(new byte[] { (byte)'a', (byte)'\n', (byte)'b' }.slice());
            t.Value.Log("c");
            w.Write(new byte[] { (byte)'d' }.slice());
            t.Value.Fail();
        }, "attr_guard_test.go", 3);

        (TestReporter reporter, _) = Run(registry);

        CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, Terminal(reporter, "TestOutputLines").Records!.ToArray(),
            "a complete line is a record; the partial 'b' is flushed by Log, and the partial 'd' at the end of the test");
    }

    [TestMethod]
    public void OutputAndItsWriterPanicWithGosTextsOnceTheTestHasCompleted()
    {
        s_writer = null;
        s_t = null;

        TestRegistry registry = new("attrguard", []);
        registry.Add("TestOutputAfter", static t =>
        {
            s_t = t;
            s_writer = t.Value.Output();
        }, "attr_guard_test.go", 4);

        Run(registry);

        PanicException write = Assert.ThrowsException<PanicException>(() => s_writer!.Write(new byte[] { (byte)'x' }.slice()));
        Assert.AreEqual("Write called after TestOutputAfter has completed", write.Message);

        PanicException output = Assert.ThrowsException<PanicException>(() => s_t!.Value.Output());
        Assert.AreEqual("Output called after TestOutputAfter has completed", output.Message);
    }

    private static string? s_first;
    private static string? s_second;
    private static bool s_existedDuring;

    [TestMethod]
    public void ArtifactDirIsOneTemporaryDirectoryPerTestWithoutTheArtifactsFlag()
    {
        s_first = s_second = null;
        s_existedDuring = false;

        TestRegistry registry = new("attrguard", []);
        registry.Add("TestArtifactTemp", static t =>
        {
            s_first = t.Value.ArtifactDir().ToString();
            s_second = t.Value.ArtifactDir().ToString();
            s_existedDuring = Directory.Exists(s_first);
        }, "attr_guard_test.go", 5);

        (TestReporter reporter, _) = Run(registry);

        Assert.AreEqual("pass", Terminal(reporter, "TestArtifactTemp").Action);
        Assert.AreEqual(s_first, s_second, "repeated calls in one test return the same directory");
        Assert.IsTrue(s_existedDuring, "the directory exists while the test runs");
        Assert.IsFalse(Directory.Exists(s_first), "without -test.artifacts the directory is removed after the test");
    }

    [TestMethod]
    public void ArtifactsFlagKeepsGosLayoutUnderTheOutputDirAndReportsIt()
    {
        s_first = null;
        string outputDir = Path.Combine(Path.GetTempPath(), "go2cs-artifacts-guard-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDir);

        try
        {
            TestRegistry registry = new("attrguard", []);
            registry.Add("TestArtifactKept", static t => s_first = t.Value.ArtifactDir().ToString(), "attr_guard_test.go", 6);

            (TestReporter reporter, _) = Run(registry, "-test.v", "-test.artifacts", "-test.outputdir=" + outputDir);

            // Go 1.26.9's layout as it RUNS, not as its comment describes: makeArtifactDir builds
            // "/" + pkg + "/" + name and hands it to filepath.Localize, which refuses a leading slash, so
            // the base is dropped and every directory sits directly under _artifacts (measured
            // 2026-10-14 with go1.26.9's own `go test -artifacts -outputdir`: TestArt and
            // TestArt/sub_one both got _artifacts/<random>).
            string expectedParent = Path.Combine(outputDir, "_artifacts");
            Assert.AreEqual(expectedParent, Path.GetDirectoryName(s_first),
                "Go's layout as measured: <outputdir>/_artifacts/<random>");
            Assert.IsTrue(Directory.Exists(s_first), "with -test.artifacts the directory is kept");

            TestEvent artifacts = reporter.Events.Single(e => e.Action == "artifacts");
            Assert.AreEqual("TestArtifactKept", artifacts.Test);
            Assert.AreEqual(s_first, artifacts.Path);
        }
        finally
        {
            Directory.Delete(outputDir, recursive: true);
        }
    }
}

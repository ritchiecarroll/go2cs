using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// Go 1.25/1.26 behavior of <c>t.Run</c> during a cleanup and of <c>makeTempDir</c>, in the host.
/// </summary>
/// <remarks>
/// Failing-first record (go1.26.10): testing's TestMorePanic re-execs TestCallRunInCleanupHelper, whose cleanup
/// calls <c>t.Run</c>; the host ran the subtest and the child printed "PASS". TestTempDir's long-name leaf
/// overflowed the path component (an infrastructure error, where Go cuts the name to 64 bytes), and
/// TestTempDirGOTMPDIR's directory ignored GOTMPDIR.
/// </remarks>
[TestClass]
public class HostGo126TempDirAndCleanupTests
{
    private const string GuardPackage = "guard";

    private static TestRunner NewRunner(string runRoot) =>
        new(new TestRegistry(GuardPackage, []), new TestOptions(),
            new TestReporter(GuardPackage, json: false, verbose: false), ".", runRoot);

    private static string NewRunRoot(string tag)
    {
        string runRoot = Path.Combine(Path.GetTempPath(), $"r-go126-{tag}-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(runRoot);
        return runRoot;
    }

    private static void RemoveQuietly(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
            // The arm's subject is the host, not this cleanup.
        }
    }

    [TestMethod]
    public void RunCalledDuringCleanupPanicsWithGosTextAndDoesNotRunTheSubtest()
    {
        string runRoot = NewRunRoot("run-in-cleanup");
        TestExecution parent = new(NewRunner(runRoot), "TestRunInCleanupGuard", null, "guard.go", 1);
        TestExecution? child = null;
        bool subtestRan = false;
        string? refusal = null;

        try
        {
            parent.Run("helper", t =>
            {
                child = t.Value.Execution;
                child!.Cleanup(() =>
                {
                    try
                    {
                        child.Run("in-cleanup", _ => subtestRan = true);
                    }
                    catch (PanicException ex)
                    {
                        refusal = ex.Message;
                        throw;
                    }
                });
            });

            Assert.IsNotNull(child, "the subtest body must have run");
            Assert.IsFalse(subtestRan, "t.Run during t.Cleanup must not run its function (Go's Issue 48515)");
            Assert.AreEqual("testing: t.Run called during t.Cleanup", refusal, "Go's panic text, testing.go:2055");
            Assert.IsTrue(child!.Failed, "a panicking cleanup fails the test");
        }
        finally
        {
            RemoveQuietly(runRoot);
        }
    }

    [TestMethod]
    public void TempDirNameIsCutToSixtyFourBytesBeforeMapping()
    {
        string longName = "TestTempDir/" + new string('a', 300);
        string pattern = TestExecution.TempDirName(longName).Split('-')[0];

        Assert.AreEqual(TestExecution.GoTempDirPattern(longName[..64]), pattern, "Go cuts c.Name() to 64 bytes, then maps");
        Assert.AreEqual(63, pattern.Length, "64 bytes less the dropped '/'");

        // A rune the cut splits is dropped, as Go's strings.Map drops the RuneError the slice leaves.
        string split = new string('a', 63) + "étail";
        Assert.AreEqual(new string('a', 63), TestExecution.TempDirName(split).Split('-')[0]);

        // The discriminator is over the WHOLE name: two names sharing 64 bytes keep two directories.
        Assert.AreNotEqual(TestExecution.TempDirName(longName), TestExecution.TempDirName(longName + "b"));
    }

    [TestMethod]
    public void TempDirHonorsGotmpdirAndNumbersChildrenAsGoDoes()
    {
        string runRoot = NewRunRoot("gotmpdir");
        string custom = Path.Combine(Path.GetTempPath(), "r-go126-custom-gotmpdir-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(custom);
        TestExecution parent = new(NewRunner(runRoot), "TestTempDirGotmpdirGuard", null, "guard.go", 1);
        string first = string.Empty;
        string second = string.Empty;
        string longLeaf = string.Empty;

        try
        {
            parent.Run("custom", t =>
            {
                TestExecution child = t.Value.Execution!;
                child.Setenv("GOTMPDIR", custom);
                first = child.TempDir().ToString();
                second = child.TempDir().ToString();
            });

            parent.Run(new string('a', 300), t => longLeaf = t.Value.Execution!.TempDir().ToString());

            Assert.IsTrue(first.StartsWith(custom, StringComparison.Ordinal), $"TempDir did not use GOTMPDIR: got {first}, want prefix {custom}");
            Assert.AreEqual("001", Path.GetFileName(first), "Go numbers a child %03d");
            Assert.AreEqual("002", Path.GetFileName(second));
            Assert.IsTrue(longLeaf.Length > 0, "a 300-byte test name must still get a temp directory");
        }
        finally
        {
            RemoveQuietly(runRoot);
            RemoveQuietly(custom);
        }
    }
}

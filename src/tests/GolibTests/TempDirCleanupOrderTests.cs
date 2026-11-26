using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class TempDirCleanupOrderTests
{
    // WHY THIS EXISTS, and why the ORDER is the arm.
    //
    // Go's TempDir registers ONE cleanup — the removal of the test's PARENT temp directory — on the
    // FIRST call (testing.go:1265-1269); later calls only number a child inside it and register
    // nothing. Until 2026-09-15 this host registered a cleanup PER CALL, and because cleanups run
    // LAST-IN-FIRST-OUT that granularity became an ORDER: a Chdir restore registered BETWEEN two
    // TempDir calls ran AFTER the later child's removal. Windows refuses to delete the directory a
    // process stands in, so the removal failed and the host reported an infrastructure error.
    //
    // THE SHAPE IS os's OWN TEST. TestChdirAndGetwd does t.Chdir(t.TempDir()), takes two more
    // TempDirs and chdirs into the last of them; it failed 3 of 3 in a fresh os host with "The
    // process cannot access the file ... because it is being used by another process" on the THIRD
    // directory, while `go test` passed it every time. TestProgWideChdir — one TempDir, registered
    // BEFORE its Chdir — passed on the same host, which is the contrast that located the order.
    //
    // ⚠ WHY THIS FILE IS IN THE WINDOWS/UNSET COMPILE GROUP. The wrong order is platform-independent,
    // but the FAILURE is not: Linux happily unlinks a directory that is some process's working
    // directory, so on Linux this arm would pass with the defect present and guard nothing. A guard
    // placed where it cannot go red reads green forever, so it is compiled where it can fail.
    //
    // RED-FIRST, measured before the fix was believed: with the per-call registration restored, the
    // arm FAILS on the infrastructure assertion below, naming the directory the process stands in.

    private const string GuardPackage = "guard";

    private static TestRunner NewRunner(string runRoot) =>
        new(new TestRegistry(GuardPackage, []), new TestOptions(),
            new TestReporter(GuardPackage, json: false), ".", runRoot);

    [TestMethod]
    public void EveryCleanupSucceedsWhenAChdirSitsBetweenTempDirCalls()
    {
        // An ABSOLUTE run root: TempDir builds its path from it, and this arm deliberately moves the
        // process working directory, so a relative root (what the sibling guards pass) would follow the
        // Chdir and put the later children somewhere else entirely.
        string runRoot = Path.Combine(Path.GetTempPath(), "i9-tempdir-order-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(runRoot);

        string cwdBefore = Directory.GetCurrentDirectory();
        TestExecution parent = new(NewRunner(runRoot), "TestTempDirCleanupOrderGuard", null, "guard.go", 1);

        TestExecution? child = null;
        string first = string.Empty;
        string last = string.Empty;

        try
        {
            // os's TestChdirAndGetwd in miniature, through the host's real subtest path: the child's
            // cleanup phase runs as Run returns.
            parent.Run("chdir_between_tempdirs", t =>
            {
                child = t.Value.Execution;

                first = child.TempDir().ToString();
                child.Chdir(first);

                _ = child.TempDir();
                last = child.TempDir().ToString();

                // ⚠ A RAW SetCurrentDirectory, deliberately, and NOT the host's Chdir helper. os's
                // TestChdirAndGetwd reaches its last temp directory through os.Chdir, which registers
                // NOTHING. The helper would register ANOTHER restore, and LIFO would pop that restore
                // BEFORE the child removals — moving the process out of harm's way and making the
                // order unobservable. That is precisely what the first version of this arm did, and
                // why its red-first control fired on the wrong assertion instead of the intended one.
                // The test must END standing in the LAST temp directory, as os's does.
                Directory.SetCurrentDirectory(last);
            });

            Assert.IsNotNull(child, "the subtest body must have run and exposed its execution");
            Assert.AreNotEqual(first, last, "the arm needs at least two distinct temp directories to order");

            Assert.IsFalse(child!.InfrastructureFailed,
                "EVERY cleanup must have succeeded. A failure here is the per-call registration: LIFO removes the " +
                "LAST temp directory while the process still stands in it, and Windows refuses. Go registers one " +
                "parent removal on the FIRST TempDir call, so the Chdir restore — registered after it — runs first.");

            string parentDir = Path.GetDirectoryName(first)!;
            Assert.IsFalse(Directory.Exists(parentDir),
                "the test's parent temp directory must be gone after the cleanup phase: one registration removes the " +
                "whole tree, exactly as Go's does");

            Assert.AreEqual(cwdBefore, Directory.GetCurrentDirectory(),
                "the Chdir restore must have run, putting the process back where it started");
        }
        finally
        {
            // Never leave the process somewhere else, whatever the arm concluded: MSTest runs this
            // assembly serially and every later test would inherit it.
            Directory.SetCurrentDirectory(cwdBefore);

            try
            {
                if (Directory.Exists(runRoot))
                    Directory.Delete(runRoot, recursive: true);
            }
            catch (IOException)
            {
                // The arm's subject is the host's cleanup, not this one's.
            }
        }
    }

    [TestMethod]
    public void TwoTempDirCallsShareOneParentAndBothSurviveUntilTheCleanupPhase()
    {
        // MUST-NOT-REGRESS, and the half Go's own TestTempDir asserts: repeated calls return DISTINCT
        // directories under ONE parent, and neither is removed early by the single registration.
        string runRoot = Path.Combine(Path.GetTempPath(), "i9-tempdir-share-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(runRoot);

        TestExecution parent = new(NewRunner(runRoot), "TestTempDirSharesAParentGuard", null, "guard.go", 2);
        TestExecution? child = null;
        string one = string.Empty;
        string two = string.Empty;

        try
        {
            parent.Run("two_calls", t =>
            {
                child = t.Value.Execution;
                one = child.TempDir().ToString();
                two = child.TempDir().ToString();

                Assert.AreNotEqual(one, two, "TempDir must return a fresh directory per call");
                Assert.AreEqual(Path.GetDirectoryName(one), Path.GetDirectoryName(two),
                    "calls to TempDir must share a parent — Go's TestTempDir asserts exactly this");
                Assert.IsTrue(Directory.Exists(one) && Directory.Exists(two),
                    "both directories must exist while the test is running");
            });

            Assert.IsNotNull(child, "the subtest body must have run");
            Assert.IsFalse(child!.InfrastructureFailed, "the cleanup phase must have succeeded");

            // ⚠ NO post-cleanup assertion on the PARENT here, deliberately. Whether the parent is
            // removed is arm 1's subject, and measured: with the per-call registration restored, this
            // arm went red on exactly that assertion — so a companion carrying it fails whenever the
            // subject fails and can no longer tell the ORDER from the machinery. What stays is the
            // shape Go's own TestTempDir asserts (distinct directories, one shared parent, both alive
            // while the test runs), which holds on every target and with or without the defect.
        }
        finally
        {
            try
            {
                if (Directory.Exists(runRoot))
                    Directory.Delete(runRoot, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}

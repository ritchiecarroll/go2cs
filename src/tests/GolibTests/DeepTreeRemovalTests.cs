using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class DeepTreeRemovalTests
{
    // WHY THIS EXISTS. os's TestGetwdDeep chdirs one 200-byte name at a time until its working
    // directory passes PATH_MAX, and leaves that tree in its TempDir. `go test` removes it: Go's
    // os.RemoveAll works relative to directory descriptors (removeall_at.go), so no call ever names a
    // path longer than one component. .NET's recursive delete names FULL paths, and on Linux both of
    // the host's routes threw ArgumentException ("The value cannot be an empty string") once a path
    // passed PATH_MAX — measured 2026-09-26 on the WSL arm, in a standalone .NET 10 program as well:
    //
    //   * TestExecution.RemoveAll (the TempDir cleanup): TestGetwdDeep PASSED in C# and was then
    //     reported as an infrastructure-error by its own cleanup — os linux family 3.
    //   * PackageAncestry.Delete (the sandbox teardown and the next run's reclaim): the stranded
    //     sandbox made ReclaimAbandonedSandboxes throw out of TryStage, so the NEXT os run on the box
    //     died before its first test, with zero C# results.
    //
    // ⚠ Compiled for GoTargetOS=linux only (GolibTests.csproj): Windows names long paths natively,
    // so there these arms would pass with the defect present and guard nothing.

    // A path this deep in bytes is past Linux's PATH_MAX (4096) with room to spare.
    private const int Levels = 22;
    private static readonly string Component = new('a', 200);

    [TestMethod]
    public void TheSandboxTeardownRemovesATreeDeeperThanPathMax()
    {
        string runRoot = NewRoot("g-deeptree-sandbox-");
        string deepest = BuildDeepChain(Path.Combine(runRoot, ".tmp"));

        try
        {
            Assert.IsTrue(Encoding.UTF8.GetByteCount(deepest) > 4096, "the fixture must actually pass PATH_MAX");

            PackageAncestry.Delete(runRoot);

            Assert.IsFalse(Directory.Exists(runRoot), "the sandbox teardown must remove a tree deeper than PATH_MAX, as Go's os.RemoveAll does");
        }
        finally
        {
            Scrub(runRoot);
        }
    }

    [TestMethod]
    public void ATempDirHoldingATreeDeeperThanPathMaxIsCleanedUp()
    {
        string runRoot = NewRoot("g-deeptree-tempdir-");
        TestExecution parent = new(NewRunner(runRoot), "TestDeepTempDirGuard", null, "guard.go", 1);
        TestExecution? child = null;
        string tempDir = string.Empty;

        try
        {
            parent.Run("getwd_deep", t =>
            {
                child = t.Value.Execution;
                tempDir = child.TempDir().ToString();
                BuildDeepChain(Path.Combine(tempDir, "deep"));
            });

            Assert.IsNotNull(child, "the subtest body must have run");
            Assert.IsFalse(child!.InfrastructureFailed, "the TempDir cleanup must remove a tree deeper than PATH_MAX; os's TestGetwdDeep passes and was reported as an infrastructure-error by this cleanup");
            Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(tempDir)!), "the test's temp parent must be gone");
        }
        finally
        {
            Scrub(runRoot);
        }
    }

    private static TestRunner NewRunner(string runRoot) =>
        new(new TestRegistry("guard", []), new TestOptions(), new TestReporter("guard", json: false), ".", runRoot);

    private static string NewRoot(string prefix)
    {
        string root = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        return root;
    }

    // Builds `Levels` nested `Component` directories under `parent` WITHOUT ever naming a long path:
    // the chain grows at a short scratch location and each step renames it one level down, so every
    // call's arguments stay short while the tree itself ends up deeper than PATH_MAX. (Creating it
    // by full path is impossible in .NET for the same reason the defect exists; chdir is process-wide.)
    // Returns the deepest path, which is only a string: nothing may open it.
    private static string BuildDeepChain(string parent)
    {
        Directory.CreateDirectory(parent);
        string chain = Path.Combine(parent, "c0");
        Directory.CreateDirectory(chain);
        File.WriteAllText(Path.Combine(chain, "leaf"), "leaf");

        for (int i = 1; i <= Levels; i++)
        {
            string holder = Path.Combine(parent, "c" + i);
            Directory.CreateDirectory(holder);
            Directory.Move(chain, Path.Combine(holder, Component));
            chain = holder;
        }

        string deepest = chain;

        for (int i = 0; i < Levels; i++)
            deepest = Path.Combine(deepest, Component);

        return deepest;
    }

    // Leaves nothing behind whatever the arm concluded — by the system's own rm, which removes by
    // descriptor and so is immune to the defect under test.
    private static void Scrub(string root)
    {
        if (!Directory.Exists(root))
            return;

        using System.Diagnostics.Process rm = System.Diagnostics.Process.Start("rm", ["-rf", root])!;
        rm.WaitForExit();
    }
}

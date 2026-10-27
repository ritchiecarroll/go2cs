using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class HelperSandboxContainmentTests
{
    // THE INVARIANT: a re-exec'd helper whose working directory is its parent's sandbox SPELLED THROUGH A
    // SYMLINK is still recognized as that sandbox's helper (on unix), and nothing that is not inside the
    // sandbox is, on any OS.
    //
    // A helper spawned through an environment-filtering API (net/http/cgi) cannot see the
    // GO2CS_TEST_SANDBOX variable, so it recognizes itself by the marker FILE beside the executable, and
    // only when its working directory is inside the marker's run root (TestHost.InheritedSandboxFromMarkerFile).
    // The run root is the parent's spelling (Path.GetTempPath: $TMPDIR, /var/folders/... on darwin); the
    // helper's working directory is getcwd()'s, which is physical (/private/var/folders/..., /var being a
    // symlink). The lexical containment test said no, the helper sandboxed afresh under /tmp, and cgi's
    // TestDir and TestEnvOverride read the fresh GUID root as the child's os.Getwd() on both mac RIDs
    // (go1.26.10 D-W1, run 40218859736). TestHost.IsWithinCanonical asks through realpath(3) on unix.
    //
    // RED-FIRST, 2026-10-25, on linux: with IsWithinCanonical planted back to the lexical IsWithin,
    // APhysicalChildOfASymlinkedRootIsContainedOnUnix fails and names the pair; restored, it passes.
    // Windows keeps the lexical comparison by ruling, so there the same test pins that NOTHING changed.

    private sealed class SymlinkedRoot : IDisposable
    {
        // <base>/real/run is the sandbox; <base>/link -> <base>/real, so <base>/link/run is the parent's
        // (logical) spelling of it and <base>/real/run the helper's (physical) one.
        public readonly string Base;
        public readonly string LogicalRun;
        public readonly string PhysicalRun;

        public SymlinkedRoot()
        {
            Base = Path.Combine(Path.GetTempPath(), "g2cs-containment-" + Guid.NewGuid().ToString("N"));
            string real = Path.Combine(Base, "real");
            PhysicalRun = Path.Combine(real, "run");
            Directory.CreateDirectory(Path.Combine(PhysicalRun, "src", "net", "http", "cgi"));
            Directory.CreateDirectory(Path.Combine(real, "run2", "src"));
            Directory.CreateDirectory(Path.Combine(real, "runX", "src"));

            string link = Path.Combine(Base, "link");

            try
            {
                Directory.CreateSymbolicLink(link, real);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                Dispose();
                Assert.Inconclusive($"this host cannot create a directory symlink ({ex.GetType().Name}): {ex.Message}");
            }

            LogicalRun = Path.Combine(link, "run");
        }

        public void Dispose()
        {
            try
            {
                string link = Path.Combine(Base, "link");

                if (Directory.Exists(link))
                    Directory.Delete(link);   // the link itself, never its target's contents

                if (Directory.Exists(Base))
                    Directory.Delete(Base, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    [TestMethod]
    public void APhysicalChildOfASymlinkedRootIsContainedOnUnix()
    {
        using SymlinkedRoot root = new();
        string physicalChild = Path.Combine(root.PhysicalRun, "src", "net", "http", "cgi");

        bool contained = TestHost.IsWithinCanonical(physicalChild, root.LogicalRun);

        if (OperatingSystem.IsWindows())
            Assert.IsFalse(contained, "windows keeps the LEXICAL containment test (COORD's ruling), so a symlink spelling must not widen it there");
        else
            Assert.IsTrue(contained, $"the helper's physical working directory {physicalChild} is inside the parent's run root spelled through a symlink, {root.LogicalRun}, and was not recognized -- net/http/cgi's darwin TestDir/TestEnvOverride shape");
    }

    [TestMethod]
    public void TheLexicalAnswersAreUnchanged()
    {
        using SymlinkedRoot root = new();

        // Every pair the lexical test admitted is still admitted, on every OS: the root itself and a child
        // spelled the same way the root is.
        Assert.IsTrue(TestHost.IsWithinCanonical(root.LogicalRun, root.LogicalRun), "a root is inside itself");
        Assert.IsTrue(TestHost.IsWithinCanonical(Path.Combine(root.LogicalRun, "src"), root.LogicalRun), "a child spelled like its root");
        Assert.IsTrue(TestHost.IsWithinCanonical(Path.Combine(root.PhysicalRun, "src"), root.PhysicalRun), "a physical child of a physical root");
    }

    [TestMethod]
    public void AForeignRunIsNeverTakenForAHelper()
    {
        // THE POLARITY: a second, genuinely separate run -- a sibling sandbox, or a directory whose name merely
        // SHARES the run root's prefix -- is not inside the root under any spelling, so it sandboxes normally.
        using SymlinkedRoot root = new();
        string realParent = Path.GetDirectoryName(root.PhysicalRun)!;

        foreach (string sibling in new[] { Path.Combine(realParent, "run2", "src"), Path.Combine(realParent, "runX", "src") })
        {
            Assert.IsFalse(TestHost.IsWithinCanonical(sibling, root.LogicalRun), $"{sibling} is a foreign run, not inside {root.LogicalRun}");
            Assert.IsFalse(TestHost.IsWithinCanonical(sibling, root.PhysicalRun), $"{sibling} is a foreign run, not inside {root.PhysicalRun}");
        }

        // And the root is never inside its own child, through the link or not.
        Assert.IsFalse(TestHost.IsWithinCanonical(root.LogicalRun, Path.Combine(root.PhysicalRun, "src")), "a root is not inside its child");

        // A path that does not exist resolves to nothing and answers through the lexical test alone.
        Assert.IsFalse(TestHost.IsWithinCanonical(Path.Combine(realParent, "missing", "x"), root.LogicalRun), "an unresolvable path is not contained");
    }

    // THE SAME CLASS, ONE PACKAGE OVER: PackageAncestry's staged-toolchain composer starts a `go` child that
    // starts inside the staged ancestry in the REAL GOROOT directory it stands for. With cmd.Dir empty its
    // directory is the host's getcwd() (physical), measured against the run root's GetTempPath spelling, and
    // on darwin math/big's TestEscape child stayed in the staged copy (go1.26.10 D-W2, both RIDs).
    //
    // RED-FIRST, 2026-10-25, on linux: with the composer's realpath arm planted out,
    // AToolchainChildInAPhysicalSpellingOfTheStagedRootStartsInRealGorootOnUnix fails; restored, it passes.

    private sealed class StagedToolchain
    {
        public readonly string RealGoroot;
        public readonly string GoTool;

        public StagedToolchain(SymlinkedRoot root)
        {
            RealGoroot = Path.Combine(root.Base, "goroot");
            Directory.CreateDirectory(Path.Combine(RealGoroot, "src", "math", "big"));
            Directory.CreateDirectory(Path.Combine(root.PhysicalRun, "src", "math", "big"));
            GoTool = Path.GetFullPath(Path.Combine(RealGoroot, "bin", "go"));
        }

        public string Compose(SymlinkedRoot root, string executable, string directory) =>
            PackageAncestry.ComposeStagedToolchainDirectory(GoTool, Path.GetFullPath(root.LogicalRun), RealGoroot, executable, directory);
    }

    [TestMethod]
    public void AToolchainChildInAPhysicalSpellingOfTheStagedRootStartsInRealGorootOnUnix()
    {
        using SymlinkedRoot root = new();
        StagedToolchain toolchain = new(root);

        string composed = toolchain.Compose(root, toolchain.GoTool, Path.Combine(root.PhysicalRun, "src", "math", "big"));

        if (OperatingSystem.IsWindows())
            Assert.IsNull(composed, "windows keeps the LEXICAL relative-path test (COORD's ruling), so a symlink spelling must not widen it there");
        else
            Assert.AreEqual(Path.Combine(toolchain.RealGoroot, "src", "math", "big"), composed,
                "a go child started in the physical spelling of the staged math/big directory must start in the real GOROOT's -- math/big TestEscape's darwin shape");
    }

    // cfile's TestIssue59563 (seat #8 (b)): its go child starts in testdata/issue59563 under the staged package,
    // and that testdata is a STAGED LINK to the real tree. Its relative cmd.Dir resolves against getcwd(), the
    // physical spelling, so on darwin the lexical test fails by the /var spelling; and realpath of the whole
    // directory follows the link out of the staged root. Neither arm admitted it, so the child stayed in the
    // sandbox spelling on both mac RIDs (run 40396761653). The physical spelling is asked against the canonical
    // root first, before the directory's own links are resolved.
    [TestMethod]
    public void AToolchainChildUnderAStagedFixtureLinkStartsInRealGorootOnUnix()
    {
        using SymlinkedRoot root = new();
        StagedToolchain toolchain = new(root);
        string realTestdata = Path.Combine(toolchain.RealGoroot, "src", "internal", "coverage", "cfile", "testdata");
        string stagedPackage = Path.Combine(root.PhysicalRun, "src", "internal", "coverage", "cfile");

        Directory.CreateDirectory(Path.Combine(realTestdata, "issue59563"));
        Directory.CreateDirectory(stagedPackage);
        Directory.CreateSymbolicLink(Path.Combine(stagedPackage, "testdata"), realTestdata);

        string? composed = toolchain.Compose(root, toolchain.GoTool, Path.Combine(stagedPackage, "testdata", "issue59563"));

        if (OperatingSystem.IsWindows())
            Assert.IsNull(composed, "windows keeps the LEXICAL relative-path test (COORD's ruling), so a symlink spelling must not widen it there");
        else
            Assert.AreEqual(Path.Combine(realTestdata, "issue59563"), composed,
                "a go child started under a staged fixture link, in the physical spelling of the staged root, must start in the real GOROOT's directory -- cfile TestIssue59563's darwin shape");
    }

    [TestMethod]
    public void TheComposersLexicalAnswersAndPolarityAreUnchanged()
    {
        using SymlinkedRoot root = new();
        StagedToolchain toolchain = new(root);
        string expected = Path.Combine(toolchain.RealGoroot, "src", "math", "big");

        // The lexical answer, on every OS: a child spelled like the staged root.
        Assert.AreEqual(expected, toolchain.Compose(root, toolchain.GoTool, Path.Combine(root.LogicalRun, "src", "math", "big")), "a child spelled like its staged root");

        // The polarity: a sibling run under either spelling, an executable that is not this GOROOT's go, and a
        // directory with no real counterpart all stay "unchanged".
        string realParent = Path.GetDirectoryName(root.PhysicalRun)!;
        Assert.IsNull(toolchain.Compose(root, toolchain.GoTool, Path.Combine(realParent, "run2", "src")), "a sibling run (physical) is not inside the staged root");
        Assert.IsNull(toolchain.Compose(root, toolchain.GoTool, Path.Combine(root.Base, "link", "runX", "src")), "a prefix-sharing sibling (logical) is not inside the staged root");
        Assert.IsNull(toolchain.Compose(root, Path.Combine(root.Base, "other", "go"), Path.Combine(root.PhysicalRun, "src", "math", "big")), "only this GOROOT's own go binary qualifies");
        Assert.IsNull(toolchain.Compose(root, toolchain.GoTool, Path.Combine(root.PhysicalRun, "src", "net", "http", "cgi")), "no real counterpart, no composed directory");
    }
}

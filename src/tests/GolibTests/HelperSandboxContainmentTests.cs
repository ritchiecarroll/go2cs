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
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class SandboxPhysicalPathTests
{
    // THE INVARIANT: a re-exec'd helper recognizes its parent's sandbox even when the parent named
    // that sandbox through a symbolic link.
    //
    // A child learns its working directory from getcwd(3), which answers the PHYSICAL path. The
    // host records its run root as it built it, from the temp path's own spelling — and on macOS
    // that spelling is /var/folders/…, where /var is a link to /private/var. net/http/cgi spawns
    // its child with a filtered environment (no PWD, no marker variable), so the child falls back
    // to the marker FILE and asks whether its getcwd answer is inside the recorded root. Compared
    // literally, /private/var/… is never inside /var/…: the child took itself for a fresh run,
    // staged its own sandbox under /tmp and reported that as its cwd. cgi's TestDir and
    // TestEnvOverride are the measured witnesses (darwin, both legs).
    //
    // Unix only: Windows hands a child the current directory string its parent passed, unresolved,
    // and creating a link there needs a privilege a test runner may not hold.

    private static string NewDirectory(string tag)
    {
        string path = Path.Combine(Path.GetTempPath(), $"g2cs-physpath-{tag}-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void SkipOnWindows()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Assert.Inconclusive("Unix only: a Windows child's current directory is the string its parent passed.");
    }

    [TestMethod]
    public void AWorkingDirectoryReadThroughGetcwdIsInsideARootNamedThroughALink()
    {
        SkipOnWindows();

        string real = NewDirectory("real");
        string link = real + "-link";

        try
        {
            Directory.CreateSymbolicLink(link, real);

            // The parent records its root through the link; the child's getcwd answers the physical
            // directory beneath it.
            string recordedRoot = Path.Combine(link, "go2cs-tests", "run");
            string childCwd = Path.Combine(real, "go2cs-tests", "run", "src", "net", "http", "cgi");
            Directory.CreateDirectory(childCwd);

            Assert.IsTrue(TestHost.IsWithin(childCwd, recordedRoot),
                "a physical working directory under a root recorded through a link was judged OUTSIDE it — " +
                "a filtered-environment helper would then sandbox again and report the wrong cwd");

            // The control: a directory genuinely outside the root stays outside, links or not.
            string outside = NewDirectory("outside");

            try
            {
                Assert.IsFalse(TestHost.IsWithin(outside, recordedRoot),
                    "containment must not be widened: an unrelated directory was judged inside the run root");
            }
            finally
            {
                Directory.Delete(outside, recursive: true);
            }
        }
        finally
        {
            File.Delete(link);
            Directory.Delete(real, recursive: true);
        }
    }
}

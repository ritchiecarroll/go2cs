using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The test host's run root is short, so a socket a test binds inside its <c>t.TempDir</c> stays within the unix
/// socket path limit (108 bytes on linux, 104 on darwin) wherever Go's own does.
/// </summary>
/// <remarks>
/// Failing-first record (go1.26.10.2): the run root was <c>&lt;temp&gt;/go2cs-tests/&lt;package&gt;/&lt;32 hex&gt;</c>, 50 bytes plus
/// the package name above the temp path, and bufio's TestCopyUnixpacket socket reached 116 bytes under a 19-byte
/// TMPDIR (/dev/shm/p2-h10-tmp): bind returned EINVAL and the converted side skipped a test Go passed
/// (docs/KnownIssues.md, the long-TMPDIR entry). The arm reaches CreateRunDirectory by reflection so the same
/// assertion runs against the old host.
/// </remarks>
[TestClass]
public class HostRunRootLengthTests
{
    // <temp>/go2cs-t/<8 hex>/.tmp: the run root's .tmp, where every t.TempDir sits, 21 bytes below the temp path.
    private const int MaxTempDirParentDepth = 21;

    [TestMethod]
    public void TheRunRootsTempDirParentIsShortAndIndependentOfThePackageName()
    {
        string temp = Path.GetTempPath();

        foreach (string package in new[] { "bufio", "net/http/internal/httpcommon", "vendor/golang.org/x/net/http2/hpack" })
        {
            (string runRoot, string workingDirectory) = CreateRunDirectory(package);

            try
            {
                Assert.IsTrue(runRoot.StartsWith(temp, StringComparison.Ordinal), $"{package}: run root {runRoot} is not under the temp path {temp}");

                int depth = Path.Combine(runRoot, ".tmp").Length - temp.Length;

                Assert.IsTrue(depth <= MaxTempDirParentDepth,
                    $"{package}: the run root's .tmp is {depth} bytes below the temp path ({runRoot}), more than {MaxTempDirParentDepth}; a unix socket in a t.TempDir can pass the path limit where Go's does not");
                Assert.IsFalse(runRoot.Contains(package.Replace('/', '_'), StringComparison.Ordinal),
                    $"{package}: the run root {runRoot} carries the package name, so its depth grows with it");
                Assert.IsTrue(workingDirectory.StartsWith(runRoot, StringComparison.Ordinal) && Directory.Exists(workingDirectory),
                    $"{package}: the working directory {workingDirectory} is not inside the run root");
            }
            finally
            {
                try
                {
                    Directory.Delete(runRoot, recursive: true);
                }
                catch (IOException)
                {
                    // The arm's subject is the path, not this cleanup.
                }
            }
        }
    }

    private static (string runRoot, string workingDirectory) CreateRunDirectory(string package)
    {
        MethodInfo method = typeof(TestHost).GetMethod("CreateRunDirectory", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new AssertFailedException("TestHost.CreateRunDirectory is gone");

        return ((string, string))method.Invoke(null, [package])!;
    }
}

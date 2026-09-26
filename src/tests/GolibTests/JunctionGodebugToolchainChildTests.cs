// JunctionGodebugToolchainChildTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.os;
using go.testing_runtime;
using static go.builtin;

namespace GolibTests;

// THE JUNCTION FALLBACK'S SETTING MUST SURVIVE A TOOLCHAIN CHILD THAT SETS ITS OWN GODEBUG (i9,
// 2026-09-26, COORD ruling after b80cd80056). A host without the symbolic-link privilege stages Go's
// fixture trees as JUNCTIONS, which Go 1.24 accepts only under GODEBUG=winsymlink=0, and the host
// publishes that setting into both environments (PackageAncestry.ApplyJunctionGodebug). But a test
// that starts the toolchain with its OWN GODEBUG -- internal/trace's testTraceProg does
// `cmd.Env = append(os.Environ(), ..., "GODEBUG=tracecheckstackownership=1,...")` -- replaces it: os/exec
// keeps the LAST duplicate key, the child `go` loses winsymlink=0, and its `go run` of the staged
// testprog is refused ("use of internal package internal/profile not allowed"). Measured on the i9
// at master 3ffd1d8a8d: all four TestTraceCPUProfile verdicts fail that way, and appending
// winsymlink=0 to that one line makes all four pass (two runs).
//
// Arms, all through the CONVERTED os/exec -- the path the failing test takes:
//   1. a toolchain child with its own GODEBUG receives it with winsymlink=0 appended, other keys
//      unaltered and in order (`go env GODEBUG` echoes what the child received);
//   2. the consequence: that child can `go list` the junction-staged testprog;
//   3. a child GODEBUG that already names winsymlink is left exactly as it is;
//   4. a NON-toolchain child keeps its own GODEBUG exactly (the program under test is never touched);
//   5. once the fallback is restored, a toolchain child's GODEBUG is its own again.
// Arms 1 and 2 are red wherever the fallback is active and the setting does not survive. On a host
// holding the privilege no junction is staged and nothing is composed: the test is Inconclusive there,
// by name.
[TestClass]
public class JunctionGodebugToolchainChildTests
{
    private const string OwnGodebug = "tracecheckstackownership=1,traceadvanceperiod=0";

    [TestMethod]
    public void AToolchainChildThatSetsItsOwnGodebugKeepsTheJunctionSetting()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("the junction fallback exists only on Windows");
            return;
        }

        string? goRoot = Environment.GetEnvironmentVariable("GOROOT");
        string relative = Path.Combine("testdata", "testprog");

        if (string.IsNullOrWhiteSpace(goRoot) ||
            !Directory.Exists(Path.Combine(goRoot, "src", "internal", "trace", relative)) ||
            !File.Exists(Path.Combine(goRoot, "bin", "go.exe")))
        {
            Assert.Inconclusive("needs a GOROOT holding internal/trace/testdata/testprog and a go binary");
            return;
        }

        string goExe = Path.Combine(goRoot, "bin", "go.exe");

        // A sibling test stages without restoring, which leaves the process GODEBUG carrying the
        // setting and the apply marked done: start from the state a fresh host starts from.
        PackageAncestry.RestoreJunctionGodebug();

        if (NamesWinsymlink(Environment.GetEnvironmentVariable("GODEBUG")))
        {
            Assert.Inconclusive("this run's own environment names winsymlink, so the host imposes nothing to carry");
            return;
        }

        string runRoot = Path.Combine(Path.GetTempPath(), "g2cs-jgodebug-" + Guid.NewGuid().ToString("N"));

        try
        {
            // The module-rooted sandbox the host builds (see FixtureLinkStagingTests' toolchain-probe
            // test): GOROOT's src/go.mod above the package, the real testprog tree linked in.
            string sandboxSrc = Path.Combine(runRoot, "src");
            Directory.CreateDirectory(sandboxSrc);
            File.Copy(Path.Combine(goRoot, "src", "go.mod"), Path.Combine(sandboxSrc, "go.mod"));

            string workingDirectory = Path.Combine(sandboxSrc, "internal", "trace");
            Directory.CreateDirectory(Path.Combine(workingDirectory, "testdata"));

            PackageAncestry.StageFixtureLinks(["testdata/testprog"], goRoot, "internal/trace", workingDirectory, runRoot);

            if (!NamesWinsymlink(Environment.GetEnvironmentVariable("GODEBUG")))
            {
                Assert.Inconclusive("symbolic links were staged (the privilege is held): no junction, nothing to compose");
                return;
            }

            // 1. The replace-GODEBUG shape.
            Assert.AreEqual(OwnGodebug + ",winsymlink=0", EchoGodebug(goExe, OwnGodebug, workingDirectory),
                "a toolchain child that sets its own GODEBUG lost the junction fallback's winsymlink=0");

            // 2. Its consequence, the refusal internal/trace's TestTraceCPUProfile died of.
            (string listed, bool listedOk) = Run(goExe, OwnGodebug, workingDirectory, "list", "testdata/testprog/cpu-profile.go");
            Assert.IsTrue(listedOk && listed == "command-line-arguments",
                $"the toolchain child refused the junction-staged testprog: {listed}");

            // 3. The child's own explicit winsymlink wins, whichever way it points.
            Assert.AreEqual("winsymlink=1,x=2", EchoGodebug(goExe, "winsymlink=1,x=2", workingDirectory),
                "a child GODEBUG that already names winsymlink was altered");

            // 4. A non-toolchain child is never touched.
            (string echoed, _) = Run(Path.Combine(Environment.SystemDirectory, "cmd.exe"), OwnGodebug, workingDirectory,
                "/c", "echo %GODEBUG%");
            Assert.AreEqual(OwnGodebug, echoed, "a non-toolchain child's GODEBUG was altered");
        }
        finally
        {
            PackageAncestry.ReleaseFixtureLinks();
            PackageAncestry.RestoreJunctionGodebug();

            try { Directory.Delete(runRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        // 5. Restored: the toolchain child's GODEBUG is its own again.
        Assert.AreEqual("x=1", EchoGodebug(goExe, "x=1", Path.GetTempPath()),
            "the composition outlived the junction fallback");
    }

    private static string EchoGodebug(string goExe, string godebug, string directory)
    {
        (string output, bool ok) = Run(goExe, godebug, directory, "env", "GODEBUG");
        Assert.IsTrue(ok, $"`go env GODEBUG` failed: {output}");
        return output;
    }

    // Starts the child through the CONVERTED os/exec with the test's own GODEBUG appended last,
    // exactly as testTraceProg does.
    private static (string Output, bool Ok) Run(string name, string godebug, string directory, params string[] args)
    {
        @string[] converted = new @string[args.Length];

        for (int i = 0; i < args.Length; i++)
            converted[i] = args[i];

        ж<exec_package.Cmd> cmd = exec_package.Command(name, converted);
        cmd.Value.Dir = directory;
        cmd.Value.Env = append(os_package.Environ(), (@string)("GODEBUG=" + godebug));

        var (output, err) = cmd.CombinedOutput();

        return (((@string)output).ToString().Trim(), err == nil);
    }

    // Token-wise, as PackageAncestry.NamesJunctionSetting reads it.
    private static bool NamesWinsymlink(string? godebug)
    {
        if (string.IsNullOrEmpty(godebug))
            return false;

        foreach (string setting in godebug.Split(','))
        {
            int separator = setting.IndexOf('=');
            string name = separator < 0 ? setting : setting[..separator];

            if (name == "winsymlink")
                return true;
        }

        return false;
    }
}

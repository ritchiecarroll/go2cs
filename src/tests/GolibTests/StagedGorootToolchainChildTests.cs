// StagedGorootToolchainChildTests.cs - Gbtc
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

namespace GolibTests;

// A TOOLCHAIN CHILD THAT STARTS INSIDE THE STAGED GOROOT ANCESTRY IS GIVEN THAT ANCESTRY AS ITS GOROOT, and
// nothing else is (COORD ruling 2026-10-21, math/big's TestEscape). Go runs `go build -gcflags=-m` with
// cwd = GOROOT/src/math/big; the host's cwd is the sandbox's copy under the staged src/go.mod (module std),
// and with GOROOT left real the child go named the package std/math/big and refused its internal/byteorder
// import. Failing-first record: math/big -tests at ed51b5a3bd, TestEscape "go build -gcflags=-m: exit
// status 1 ... use of internal package internal/byteorder not allowed".
//
// Arms, all through the CONVERTED os/exec, the path the failing test takes, with the real go binary:
//   1. a go child whose cmd.Dir is the staged package directory gets the run root as its GOROOT;
//   2. the consequence: `go list .` there names math/big, not std/math/big;
//   3. a go child with NO cmd.Dir, started while the host's cwd is the staged directory, gets it too;
//   4. a go child with a cmd.Dir OUTSIDE the staged root keeps the real GOROOT;
//   5. a non-toolchain child keeps the real GOROOT;
//   6. once released (and in any process without the host's composer, which no program has), a go child
//      started inside the staged directory keeps the real GOROOT.
[TestClass]
public class StagedGorootToolchainChildTests
{
    [TestMethod]
    public void AToolchainChildStartedInsideTheStagedAncestryGetsItAsItsGoroot()
    {
        string? goRoot = Environment.GetEnvironmentVariable("GOROOT");
        string goExe = goRoot is null ? "" : Path.Combine(goRoot, "bin", OperatingSystem.IsWindows() ? "go.exe" : "go");

        if (string.IsNullOrWhiteSpace(goRoot) || !File.Exists(goExe) ||
            !Directory.Exists(Path.Combine(goRoot, "src", "math", "big")))
        {
            Assert.Inconclusive("needs a GOROOT holding src/math/big and a go binary");
            return;
        }

        string realGoroot = Path.GetFullPath(goRoot);
        string runRoot = Path.Combine(Path.GetTempPath(), "g2cs-sgoroot-" + Guid.NewGuid().ToString("N"));
        string workingDirectory = Path.Combine(runRoot, "src", "math", "big");
        string originalCwd = Directory.GetCurrentDirectory();

        try
        {
            Directory.CreateDirectory(workingDirectory);
            Assert.IsTrue(PackageAncestry.TryStage(goRoot, "math/big", runRoot, workingDirectory), "the ancestry did not stage");

            string stagedRoot = Path.GetFullPath(runRoot);

            // 1. cmd.Dir inside the staged root.
            AssertSamePath(stagedRoot, GoEnvGoroot(goExe, workingDirectory),
                "a go child started in the staged package directory did not get the staged root as its GOROOT");

            // 2. The consequence TestEscape needs: the package is std's math/big, not a module named std.
            (string listed, bool listedOk) = Run(goExe, workingDirectory, "list", ".");
            Assert.IsTrue(listedOk && listed == "math/big", $"`go list .` in the staged directory read: {listed}");

            // 3. No cmd.Dir: the child starts in the host's cwd, which is the staged directory, as TestHost sets it.
            Directory.SetCurrentDirectory(workingDirectory);
            AssertSamePath(stagedRoot, GoEnvGoroot(goExe, null),
                "a go child with no cmd.Dir, started from the staged directory, did not get the staged root");
            Directory.SetCurrentDirectory(originalCwd);

            // 4. A cmd.Dir of its own, outside the staged root.
            AssertSamePath(realGoroot, GoEnvGoroot(goExe, Path.GetTempPath()),
                "a go child with a cmd.Dir outside the staged root was given the staged root");

            // 5. Not the toolchain.
            (string echoed, _) = OperatingSystem.IsWindows()
                ? Run(Path.Combine(Environment.SystemDirectory, "cmd.exe"), workingDirectory, "/c", "echo %GOROOT%")
                : Run("/bin/sh", workingDirectory, "-c", "echo $GOROOT");
            AssertSamePath(realGoroot, echoed, "a non-toolchain child's GOROOT was altered");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
            PackageAncestry.ReleaseStagedGoroot();
        }

        try
        {
            // 6. Released: the state of every process that is not this host -- a program never has a composer.
            AssertSamePath(realGoroot, GoEnvGoroot(goExe, workingDirectory),
                "a go child kept the staged root after the facet was released");
        }
        finally
        {
            PackageAncestry.Delete(runRoot);
        }
    }

    private static string GoEnvGoroot(string goExe, string? directory)
    {
        (string output, bool ok) = Run(goExe, directory, "env", "GOROOT");
        Assert.IsTrue(ok, $"`go env GOROOT` failed: {output}");
        return output;
    }

    private static void AssertSamePath(string expected, string actual, string message) =>
        Assert.AreEqual(Path.TrimEndingDirectorySeparator(Path.GetFullPath(expected)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(actual)),
            OperatingSystem.IsWindows(), message);

    // Through the CONVERTED os/exec, with os.Environ() as the child's environment, exactly as Go's tests start go.
    private static (string Output, bool Ok) Run(string name, string? directory, params string[] args)
    {
        @string[] converted = new @string[args.Length];

        for (int i = 0; i < args.Length; i++)
            converted[i] = args[i];

        ж<exec_package.Cmd> cmd = exec_package.Command(name, converted);

        if (directory is not null)
            cmd.Value.Dir = directory;

        cmd.Value.Env = os_package.Environ();

        var (output, err) = cmd.CombinedOutput();

        return (((@string)output).ToString().Trim(), err == nil);
    }
}

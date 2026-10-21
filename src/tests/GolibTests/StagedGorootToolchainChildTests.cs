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

// A TOOLCHAIN CHILD THAT STARTS INSIDE THE STAGED GOROOT ANCESTRY STARTS IN THE REAL GOROOT DIRECTORY IT
// STANDS FOR, and nothing else moves (COORD ruling 2026-10-21, math/big's TestEscape). Go runs
// `go build -gcflags=-m` with cwd = GOROOT/src/math/big; the host's cwd is the sandbox's copy under the
// staged src/go.mod (module std), and there the child go named the package std/math/big and refused its
// internal/byteorder import. Failing-first record: math/big -tests at ed51b5a3bd, TestEscape "go build
// -gcflags=-m: exit status 1 ... use of internal package internal/byteorder not allowed".
//
// Arm 2 is the second record: handing that child the staged root as its GOROOT fixed TestEscape and broke
// every `go list std` started there, because the toolchain's package walk does not follow the mirror's
// links (go/internal/gcimporter TestImportStdLib "too few packages (1) were imported" and
// internal/godebugs TestAll, measured 2026-10-21 at a705f10a04).
//
// Arms, all through the CONVERTED os/exec, the path the failing tests take, with the real go binary:
//   1. a go child whose cmd.Dir is the staged package directory starts in the real one: `go list .` names
//      math/big and its Dir is GOROOT/src/math/big, and its GOROOT is the real one;
//   2. `go list std` started there names every package a neutral directory's does;
//   3. a go child with NO cmd.Dir, started while the host's cwd is the staged directory, starts there too;
//   4. a go child in a directory OUTSIDE the staged root, or in one inside it with no real counterpart,
//      keeps its directory;
//   5. a non-toolchain child started in the staged directory keeps it;
//   6. once released (and in any process without the host's composer, which no program has), a go child
//      started inside the staged directory keeps it.
[TestClass]
public class StagedGorootToolchainChildTests
{
    [TestMethod]
    public void AToolchainChildStartedInsideTheStagedAncestryStartsInTheRealGorootDirectory()
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
        string realPackage = Path.Combine(realGoroot, "src", "math", "big");
        string runRoot = Path.Combine(Path.GetTempPath(), "g2cs-sgoroot-" + Guid.NewGuid().ToString("N"));
        string workingDirectory = Path.Combine(runRoot, "src", "math", "big");
        string outsideModule = Path.Combine(Path.GetTempPath(), "g2cs-sgoroot-out-" + Guid.NewGuid().ToString("N"));
        string originalCwd = Directory.GetCurrentDirectory();

        try
        {
            Directory.CreateDirectory(workingDirectory);
            Assert.IsTrue(PackageAncestry.TryStage(goRoot, "math/big", runRoot, workingDirectory), "the ancestry did not stage");

            // 1. cmd.Dir inside the staged root: the package is std's math/big, read from the real directory,
            // and the environment is untouched.
            Assert.AreEqual("math/big", GoList(goExe, workingDirectory, "{{.ImportPath}}", "."),
                "`go list .` in the staged directory did not name std's math/big");
            AssertSamePath(realPackage, GoList(goExe, workingDirectory, "{{.Dir}}", "."),
                "a go child started in the staged package directory did not start in the real one");
            AssertSamePath(realGoroot, GoEnvGoroot(goExe, workingDirectory), "a go child's GOROOT was altered");

            // 2. The pattern a linked GOROOT cannot serve: std, walked from the staged directory.
            string[] neutral = GoList(goExe, Path.GetTempPath(), "{{.ImportPath}}", "std").Split('\n');
            string[] staged = GoList(goExe, workingDirectory, "{{.ImportPath}}", "std").Split('\n');
            Assert.IsTrue(neutral.Length > 225, $"`go list std` from a neutral directory named {neutral.Length} packages");
            CollectionAssert.AreEqual(neutral, staged, "`go list std` from the staged directory named a different set");

            // 3. No cmd.Dir: the child starts in the host's cwd, which is the staged directory, as TestHost sets it.
            Directory.SetCurrentDirectory(workingDirectory);
            AssertSamePath(realPackage, GoList(goExe, null, "{{.Dir}}", "."),
                "a go child with no cmd.Dir, started from the staged directory, did not start in the real one");
            Directory.SetCurrentDirectory(originalCwd);

            // 4. A directory of its own: outside the staged root, and inside it with no real counterpart.
            WriteModule(outsideModule);
            AssertSamePath(outsideModule, GoList(goExe, outsideModule, "{{.Dir}}", "."),
                "a go child with a cmd.Dir outside the staged root was moved");

            string madeHere = Path.Combine(workingDirectory, "zz_made_by_the_test");
            WriteModule(madeHere);
            AssertSamePath(madeHere, GoList(goExe, madeHere, "{{.Dir}}", "."),
                "a go child in a staged directory with no real counterpart was moved");

            // 5. Not the toolchain: it reads a file only the staged directory holds.
            File.WriteAllText(Path.Combine(workingDirectory, "zz_staged_marker"), "staged");
            (string marker, bool markerOk) = OperatingSystem.IsWindows()
                ? Run(Path.Combine(Environment.SystemDirectory, "cmd.exe"), workingDirectory, "/c", "type zz_staged_marker")
                : Run("/bin/sh", workingDirectory, "-c", "cat zz_staged_marker");
            Assert.IsTrue(markerOk && marker == "staged", $"a non-toolchain child's directory was altered: {marker}");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
            PackageAncestry.ReleaseStagedToolchainDirectory();
        }

        try
        {
            // 6. Released: the state of every process that is not this host -- a program never has a composer.
            AssertSamePath(workingDirectory, GoList(goExe, workingDirectory, "{{.Dir}}", "-e", "."),
                "a go child was still moved after the composer was released");
        }
        finally
        {
            PackageAncestry.Delete(runRoot);

            if (Directory.Exists(outsideModule))
                Directory.Delete(outsideModule, recursive: true);
        }
    }

    private static void WriteModule(string directory)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "go.mod"), "module example.com/made\n\ngo 1.24\n");
        File.WriteAllText(Path.Combine(directory, "made.go"), "package made\n");
    }

    private static string GoList(string goExe, string? directory, string format, params string[] args)
    {
        (string output, bool ok) = Run(goExe, directory, ["list", "-f", format, .. args]);
        Assert.IsTrue(ok, $"`go list -f {format} {string.Join(' ', args)}` failed: {output}");
        return output.Replace("\r\n", "\n");
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

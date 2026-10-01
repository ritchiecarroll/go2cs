using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

[TestClass]
public class ModuleAncestryTests
{
    // THE MODULE ANCESTRY (PackageAncestry.TryStageModule): a third-party package's sandbox is a real
    // COPY of its module, so its tests read their module's files by relative path as `go test` lets
    // them -- and, because it is a copy and never a link, NOTHING the sandbox does reaches the user's
    // module tree, whoever writes. Every test builds a throwaway module; none touches a real one.

    private const string ModulePath = "example.test/keys";

    private static string NewDirectory(string tag)
    {
        string path = Path.Combine(Path.GetTempPath(), $"g2cs-modstage-{tag}-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Write(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static void Discard(string root)
    {
        try
        {
            PackageAncestry.Delete(root);
        }
        catch (Exception)
        {
        }
    }

    // A jwt-shaped module: a root fixture in a NON-testdata directory (test/), a sub-package with its
    // own non-Go file and testdata, plus the things the copy must leave out (.git, a previous
    // conversion's build output) and one it must keep (a real Go package that happens to be named bin).
    private static string NewModule()
    {
        string root = NewDirectory("module");
        Write(Path.Combine(root, "go.mod"), $"module {ModulePath}\n\ngo 1.23\n");
        Write(Path.Combine(root, "keys.go"), "package keys\n");
        Write(Path.Combine(root, "test", "key.pem"), "KEY-MATERIAL\n");
        Write(Path.Combine(root, "parse", "parse.go"), "package parse\n");
        Write(Path.Combine(root, "parse", "notes.txt"), "own non-Go file\n");
        Write(Path.Combine(root, "parse", "testdata", "case.json"), "{}\n");
        Write(Path.Combine(root, ".git", "HEAD"), "ref: refs/heads/main\n");
        Write(Path.Combine(root, "parse", "bin", "Release", "parse.dll"), "not the module's\n");
        Write(Path.Combine(root, "tools", "bin", "main.go"), "package main\n");
        return root;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static Dictionary<string, string> HashTree(string root)
    {
        Dictionary<string, string> hashes = new(StringComparer.OrdinalIgnoreCase);

        foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            hashes[Path.GetRelativePath(root, file)] = Hash(file);

        return hashes;
    }

    private static string WorkingDirectoryFor(string runRoot, string importPath) =>
        Path.Combine([runRoot, "src", .. importPath.Split('/')]);

    [TestMethod]
    public void TheSandboxIsARealCopyOfTheModuleAtItsRelativePath()
    {
        string module = NewModule();
        string runRoot = NewDirectory("run");
        string workingDirectory = WorkingDirectoryFor(runRoot, ModulePath + "/parse");

        try
        {
            Assert.IsTrue(PackageAncestry.TryStageModule(module, ModulePath, ModulePath + "/parse", runRoot, workingDirectory),
                "a package inside its module, with the module root given, must be staged");

            // What a test reads by relative path, exactly as `go test` presents it.
            Assert.AreEqual("KEY-MATERIAL\n", File.ReadAllText(Path.Combine(workingDirectory, "..", "test", "key.pem")),
                "a SIBLING directory's fixture (no testdata segment) must resolve through ../");
            StringAssert.Contains(File.ReadAllText(Path.Combine(workingDirectory, "..", "go.mod")), $"module {ModulePath}",
                "the module's go.mod must sit at the module root above the package");
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "notes.txt")), "the package's own non-Go file");
            Assert.IsTrue(File.Exists(Path.Combine(workingDirectory, "testdata", "case.json")), "the package's own testdata");

            // What the copy leaves out, and the one look-alike it keeps.
            string mirrorRoot = WorkingDirectoryFor(runRoot, ModulePath);
            Assert.IsFalse(Directory.Exists(Path.Combine(mirrorRoot, ".git")), ".git is not the module's source");
            Assert.IsFalse(Directory.Exists(Path.Combine(workingDirectory, "bin")), "a previous conversion's build output is skipped");
            Assert.IsTrue(File.Exists(Path.Combine(mirrorRoot, "tools", "bin", "main.go")), "a Go package named bin is still copied");

            // A COPY: no component of the staged tree is a link back into the module.
            foreach (string directory in Directory.EnumerateDirectories(mirrorRoot, "*", SearchOption.AllDirectories))
                Assert.AreEqual((FileAttributes)0, File.GetAttributes(directory) & FileAttributes.ReparsePoint, $"'{directory}' is a link");
        }
        finally
        {
            Discard(runRoot);
            Discard(module);
        }
    }

    [TestMethod]
    public void ATestSideWriteLandsInTheCopyAndTheModuleIsByteIdentical()
    {
        string module = NewModule();
        string runRoot = NewDirectory("run");
        string workingDirectory = WorkingDirectoryFor(runRoot, ModulePath + "/parse");

        try
        {
            Dictionary<string, string> before = HashTree(module);

            Assert.IsTrue(PackageAncestry.TryStageModule(module, ModulePath, ModulePath + "/parse", runRoot, workingDirectory));

            // The writes a converted TEST could make -- not the harness's: overwrite a sibling's fixture
            // and the module's go.mod through ../, and add a new file beside them.
            File.WriteAllText(Path.Combine(workingDirectory, "..", "test", "key.pem"), "OVERWRITTEN");
            File.WriteAllText(Path.Combine(workingDirectory, "..", "go.mod"), "module overwritten\n");
            File.WriteAllText(Path.Combine(workingDirectory, "..", "test", "new.txt"), "added");

            Assert.AreEqual("OVERWRITTEN", File.ReadAllText(Path.Combine(workingDirectory, "..", "test", "key.pem")), "the write landed in the copy");

            Dictionary<string, string> after = HashTree(module);

            CollectionAssert.AreEquivalent(new List<string>(before.Keys), new List<string>(after.Keys),
                "the user's module gained or lost a file");

            foreach ((string file, string hash) in before)
                Assert.AreEqual(hash, after[file], $"the user's module file '{file}' changed");
        }
        finally
        {
            Discard(runRoot);
            Discard(module);
        }
    }

    [TestMethod]
    public void AModuleThatContainsTheSandboxIsCopiedWithoutRecursingIntoIt()
    {
        string module = NewModule();

        // The output root -- and with it this run's sandbox -- inside the module tree.
        string runRoot = Path.Combine(module, "out", "go2cs-tests", "run");
        Directory.CreateDirectory(runRoot);
        string workingDirectory = WorkingDirectoryFor(runRoot, ModulePath + "/parse");

        try
        {
            Assert.IsTrue(PackageAncestry.TryStageModule(module, ModulePath, ModulePath + "/parse", runRoot, workingDirectory));

            string mirrorRoot = WorkingDirectoryFor(runRoot, ModulePath);
            Assert.IsTrue(File.Exists(Path.Combine(mirrorRoot, "test", "key.pem")), "the rest of the module is copied");
            Assert.IsFalse(Directory.Exists(Path.Combine(mirrorRoot, "out", "go2cs-tests", "run")),
                "the copy recursed into its own sandbox");
        }
        finally
        {
            Discard(runRoot);
            Discard(module);
        }
    }

    [TestMethod]
    public void StagingDeclinesWhatIsNotAModulePackage()
    {
        string module = NewModule();
        string runRoot = NewDirectory("run");
        string workingDirectory = WorkingDirectoryFor(runRoot, "other.test/pkg");

        try
        {
            Assert.IsFalse(PackageAncestry.TryStageModule(null, ModulePath, ModulePath, runRoot, workingDirectory), "no root");
            Assert.IsFalse(PackageAncestry.TryStageModule(module, "", ModulePath, runRoot, workingDirectory), "no module path (the stdlib)");
            Assert.IsFalse(PackageAncestry.TryStageModule(module, ModulePath, "other.test/pkg", runRoot, workingDirectory), "a package of another module");
            Assert.IsFalse(PackageAncestry.TryStageModule(module, ModulePath, ModulePath + "x/pkg", runRoot, workingDirectory),
                "a path that merely STARTS with the module path is not inside it");

            File.Delete(Path.Combine(module, "go.mod"));
            Assert.IsFalse(PackageAncestry.TryStageModule(module, ModulePath, ModulePath, runRoot, workingDirectory), "a root with no go.mod");
            Assert.AreEqual(0, Directory.GetFileSystemEntries(runRoot).Length, "a declined staging leaves the sandbox untouched");
        }
        finally
        {
            Discard(runRoot);
            Discard(module);
        }
    }

    [TestMethod]
    public void AModuleStagedPackageNeedsNoFixtureLinksAndNoGoRoot()
    {
        string runRoot = NewDirectory("run");
        string workingDirectory = WorkingDirectoryFor(runRoot, ModulePath);

        try
        {
            // Without the module flag the same call refuses on the missing GOROOT (StagingWithNoGoRoot...),
            // so this asserts the module path rather than a vacuous no-op.
            Assert.ThrowsException<InvalidOperationException>(() =>
                PackageAncestry.StageFixtureLinks(["testdata/prog"], null, ModulePath, workingDirectory, runRoot));

            PackageAncestry.StageFixtureLinks(["testdata/prog"], null, ModulePath, workingDirectory, runRoot, moduleStaged: true);
        }
        finally
        {
            Discard(runRoot);
        }
    }

    // THE MODULE CACHE IS READ-ONLY: Go marks every file under GOMODCACHE so (0444; ReadOnly on
    // windows), and FileInfo.CopyTo carries the attribute onto the copy. The harness then overwrites a
    // module file it already staged -- CopyFixtures' File.Copy(source, target, true) -- and an
    // overwrite onto a ReadOnly target throws. Measured on the TRAIN L union (2026-10-01):
    // google/uuid@v1.6.0 from GOMODCACHE died in 18 s at `Access to the path '...\uuid\dce.go' is
    // denied` before any test ran, and the failed run left its ReadOnly sandbox behind, since a
    // recursive delete refuses a ReadOnly file too. Every working-copy fixture is writable, which is
    // why no proof met it. Staged here as that shape: a ReadOnly module file, the REAL CopyFixtures
    // overwriting it, and the sandbox deleted afterwards.
    [TestMethod]
    public void AReadOnlyModuleFileIsStagedWritableForTheFixtureOverwriteAndTheCleanup()
    {
        string module = NewModule();
        string runRoot = NewDirectory("run");
        string workingDirectory = WorkingDirectoryFor(runRoot, ModulePath + "/parse");
        string fixtureDirectory = "g2cs-readonly-" + Guid.NewGuid().ToString("N");
        string moduleFile = Path.Combine(module, "parse", fixtureDirectory, "dce.go");
        string hostFixture = Path.Combine(AppContext.BaseDirectory, fixtureDirectory, "dce.go");
        string staged = Path.Combine(workingDirectory, fixtureDirectory, "dce.go");

        try
        {
            Write(moduleFile, "package parse // as the module cache holds it\n");
            File.SetAttributes(moduleFile, File.GetAttributes(moduleFile) | FileAttributes.ReadOnly);
            Write(hostFixture, "package parse // as the host's fixture staging holds it\n");

            Assert.IsTrue(PackageAncestry.TryStageModule(module, ModulePath, ModulePath + "/parse", runRoot, workingDirectory));
            Assert.IsTrue(File.Exists(staged), "control: the module copy must have placed the file the fixture overwrites");

            MethodInfo copyFixtures = typeof(TestHost).GetMethod("CopyFixtures", BindingFlags.NonPublic | BindingFlags.Static)!;

            try
            {
                copyFixtures.Invoke(null, [new List<string> { fixtureDirectory + "/dce.go" }, workingDirectory, runRoot]);
            }
            catch (TargetInvocationException ex)
            {
                Assert.Fail($"the fixture overwrite of a staged module file threw: {ex.InnerException}");
            }

            Assert.AreEqual("package parse // as the host's fixture staging holds it\n", File.ReadAllText(staged), "the fixture must replace the staged copy");
            Assert.AreEqual("package parse // as the module cache holds it\n", File.ReadAllText(moduleFile), "the user's module file must be untouched");
            Assert.AreNotEqual((FileAttributes)0, File.GetAttributes(moduleFile) & FileAttributes.ReadOnly, "the user's module file must keep its own ReadOnly");

            PackageAncestry.Delete(runRoot);
            Assert.IsFalse(Directory.Exists(runRoot), "the sandbox must not be left behind");
        }
        finally
        {
            if (File.Exists(moduleFile))
                File.SetAttributes(moduleFile, File.GetAttributes(moduleFile) & ~FileAttributes.ReadOnly);

            if (File.Exists(staged))
                File.SetAttributes(staged, File.GetAttributes(staged) & ~FileAttributes.ReadOnly);

            try
            {
                Directory.Delete(Path.GetDirectoryName(hostFixture)!, true);
            }
            catch (Exception)
            {
            }

            Discard(runRoot);
            Discard(module);
        }
    }
}

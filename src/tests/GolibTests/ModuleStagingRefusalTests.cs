// ModuleStagingRefusalTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// A MODULE HOST NEVER RUNS IN A SANDBOX THAT IS SILENTLY NOT THE MODULE (COORD 20261120T093651Z, option A of C1's
// 093621Z design). `go test` runs a module's package inside the module tree, and the host stages a full copy of it
// (PackageAncestry.TryStageModule) so a test reads package-root data files and sibling packages' content as Go's does
// (modernc.org/sqlite's TestVendorStamp: vendor.json, and the content of lib/ and vec/). Two paths skipped that copy
// without a word and ran the test in the plain sandbox, where such a test fails and Go's passes:
//   (1) a module host started with no GO2CS_MODULE_ROOT (P5's by-hand sqlite run, 091457Z): now a refusal, exit 2,
//       naming the variable. GO2CS_MODULE_ROOT=none runs unstaged on purpose, and says so;
//   (2) a copy that fails part-way (TryStageModule's catch returned false and "the run continues"): now an
//       IOException naming the module root, which the host reports and exits on.
// A standard-library host and a re-exec'd helper are unchanged (ModuleSourceRemapTests covers both).
[TestClass]
public class ModuleStagingRefusalTests
{
    private const string ModulePath = "example.test/keys";
    private const string ModuleRootVariable = "GO2CS_MODULE_ROOT";
    private const string SandboxMarker = "GO2CS_TEST_SANDBOX";

    private string? m_priorModuleRoot;
    private string? m_priorMarker;
    private static bool s_probeRan;

    [TestInitialize]
    public void StartWithNoModuleRootAndNoMarker()
    {
        m_priorModuleRoot = Environment.GetEnvironmentVariable(ModuleRootVariable);
        m_priorMarker = Environment.GetEnvironmentVariable(SandboxMarker);
        Environment.SetEnvironmentVariable(ModuleRootVariable, null);
        TestHost.PublishEnvironmentVariable(SandboxMarker, null);
        s_probeRan = false;
    }

    [TestCleanup]
    public void Restore()
    {
        Environment.SetEnvironmentVariable(ModuleRootVariable, m_priorModuleRoot);
        TestHost.PublishEnvironmentVariable(SandboxMarker, m_priorMarker);
    }

    private static TestRegistry Registry(string? modulePath) => Registry(modulePath ?? "guard/modstagingrefusal", modulePath);

    private static TestRegistry Registry(string package, string? modulePath)
    {
        TestRegistry registry = new(package, [], modulePath: modulePath);
        registry.Add("TestProbe", _ => s_probeRan = true, "keys_test.go", 1);
        return registry;
    }

    private static string NewDirectory(string tag)
    {
        string path = Path.Combine(Path.GetTempPath(), $"g2cs-modrefuse-{tag}-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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

    private static int RunCapturingStderr(TestRegistry registry, out string stderr)
    {
        TextWriter prior = Console.Error;
        StringWriter captured = new();
        Console.SetError(captured);

        try
        {
            return TestHost.Run(registry, []);
        }
        finally
        {
            Console.SetError(prior);
            stderr = captured.ToString();
        }
    }

    // (1) A module host with no module root refuses before any test runs, naming the variable.
    [TestMethod]
    public void AModuleHostWithNoModuleRootRefusesNamingTheVariable()
    {
        int exit = RunCapturingStderr(Registry(ModulePath), out string stderr);

        Assert.AreEqual(2, exit, $"a module host with no {ModuleRootVariable} ran (exit {exit}); stderr: {stderr}");
        Assert.IsFalse(s_probeRan, "the probe test ran in a sandbox that is not the module");
        StringAssert.Contains(stderr, ModuleRootVariable);
    }

    // (1) The deliberate opt-out: GO2CS_MODULE_ROOT=none runs the tests unstaged, and says so.
    [TestMethod]
    public void AModuleHostWithModuleRootNoneRunsUnstagedAndSaysSo()
    {
        Environment.SetEnvironmentVariable(ModuleRootVariable, "none");

        int exit = RunCapturingStderr(Registry(ModulePath), out string stderr);

        Assert.AreEqual(0, exit, $"GO2CS_MODULE_ROOT=none must run; stderr: {stderr}");
        Assert.IsTrue(s_probeRan, "the probe test did not run");
        StringAssert.Contains(stderr, "not staged");
    }

    // A standard-library host needs no module root, and the refusal never reaches it.
    [TestMethod]
    public void AStandardLibraryHostStillRunsWithNoModuleRoot()
    {
        Assert.AreEqual(0, RunCapturingStderr(Registry(null), out string stderr), stderr);
        Assert.IsTrue(s_probeRan, "the probe test did not run");
    }

    // (2) A copy that fails part-way throws, naming the module root, instead of returning false into a partial sandbox.
    // The failure is made deterministic on every OS: a FILE stands where the copy must create the module's directory.
    [TestMethod]
    public void AModuleCopyThatFailsThrowsNamingTheModuleRoot()
    {
        string module = NewDirectory("module");
        string runRoot = NewDirectory("run");

        try
        {
            File.WriteAllText(Path.Combine(module, "go.mod"), $"module {ModulePath}\n\ngo 1.23\n");
            File.WriteAllText(Path.Combine(module, "keys.go"), "package keys\n");

            string mirror = PackageAncestry.ModuleMirrorRoot(runRoot, ModulePath);
            Directory.CreateDirectory(Path.GetDirectoryName(mirror)!);
            File.WriteAllText(mirror, "a file where the module's directory must go\n");

            IOException thrown = Assert.ThrowsException<IOException>(() =>
                PackageAncestry.TryStageModule(module, ModulePath, ModulePath, runRoot, mirror));

            StringAssert.Contains(thrown.Message, module);
        }
        finally
        {
            Discard(runRoot);
            Discard(module);
        }
    }

    // P3 110324Z, MUST 1.1 (COORD 110437Z): with the module root SET, TryStageModule's remaining "nothing to stage"
    // answers are refusals in a module host too, each naming the precondition that failed.

    // A package that is not under the module path the registry names: nothing of that module can be staged for it.
    [TestMethod]
    public void AModuleHostWhosePackageIsNotInItsModuleRefuses()
    {
        string module = NewDirectory("module");

        try
        {
            File.WriteAllText(Path.Combine(module, "go.mod"), $"module {ModulePath}\n\ngo 1.23\n");
            Environment.SetEnvironmentVariable(ModuleRootVariable, module);

            int exit = RunCapturingStderr(Registry("other.test/pkg", ModulePath), out string stderr);

            Assert.AreEqual(2, exit, $"a package outside its module ran (exit {exit}); stderr: {stderr}");
            Assert.IsFalse(s_probeRan, "the probe test ran in a sandbox that is not the module");
            StringAssert.Contains(stderr, "other.test/pkg");
            StringAssert.Contains(stderr, "not a package of module");
        }
        finally
        {
            Discard(module);
        }
    }

    // A root with no go.mod, and none handed over (GO2CS_MODULE_GOMOD): it is not a module go can read.
    [TestMethod]
    public void AModuleHostWhoseRootHasNoGoModRefuses()
    {
        string module = NewDirectory("nogomod");

        try
        {
            File.WriteAllText(Path.Combine(module, "keys.go"), "package keys\n");
            Environment.SetEnvironmentVariable(ModuleRootVariable, module);

            int exit = RunCapturingStderr(Registry(ModulePath), out string stderr);

            Assert.AreEqual(2, exit, $"a module root with no go.mod ran (exit {exit}); stderr: {stderr}");
            Assert.IsFalse(s_probeRan, "the probe test ran in a sandbox that is not the module");
            StringAssert.Contains(stderr, "go.mod");
        }
        finally
        {
            Discard(module);
        }
    }

    // 'none' is matched ignoring case, so NONE is the opt-out, not a path that silently fails to stage.
    [TestMethod]
    public void TheNoneOptOutIsMatchedIgnoringCase()
    {
        Environment.SetEnvironmentVariable(ModuleRootVariable, "NONE");

        int exit = RunCapturingStderr(Registry(ModulePath), out string stderr);

        Assert.AreEqual(0, exit, $"GO2CS_MODULE_ROOT=NONE must run; stderr: {stderr}");
        Assert.IsTrue(s_probeRan, "the probe test did not run");
        StringAssert.Contains(stderr, "not staged");
    }
}

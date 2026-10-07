// ModuleSourceRemapTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// A FRAME OF THE MODULE UNDER TEST NAMES ITS STAGED COPY (runtime's GoModuleSourceRemap; COORD ruling (ii),
// 2026-10-06). A -recurse module's records name the module-cache file the converter read. `go test` runs the
// package in that directory, so Go's working directory and Caller's directory agree. The test host runs it in
// a staged copy (PackageAncestry.TryStageModule), so it registers module root -> copy, and logrus's
// TestNestedLoggingReportsCorrectCaller (cwd + "/logrus_test.go" against Caller's file) reads the same
// relation Go's does.
//
// The probe arms pin the remap's bounds without touching the process's own state. The host arms run a real
// TestHost.Run in-process: it must register the copy for a module host, register nothing for a standard-
// library host, and withdraw what it registered when the run ends. A process that never runs a host (a
// converted PROGRAM) has no remap, so its frames are unchanged.
[TestClass]
public class ModuleSourceRemapTests
{
    private const string ModulePath = "example.test/keys";
    private const string ModuleRootVariable = "GO2CS_MODULE_ROOT";
    private const string SandboxMarker = "GO2CS_TEST_SANDBOX";

    private const string Cache = "/mod/cache/example.test/keys@v1.0.0";
    private const string Staged = "/tmp/run/src/example.test/keys";

    private string? m_priorModuleRoot;
    private string? m_priorMarker;
    private (string From, string To)? m_priorRemap;

    private static (string From, string To)? s_insideRemap;
    private static string? s_insideDirectory;

    [TestInitialize]
    public void StartWithNoModuleRootAndNoMarker()
    {
        m_priorModuleRoot = Environment.GetEnvironmentVariable(ModuleRootVariable);
        m_priorMarker = Environment.GetEnvironmentVariable(SandboxMarker);
        m_priorRemap = runtime_package.GoModuleSourceRemap;

        Environment.SetEnvironmentVariable(ModuleRootVariable, null);
        TestHost.PublishEnvironmentVariable(SandboxMarker, null);
        s_insideRemap = null;
        s_insideDirectory = null;
    }

    [TestCleanup]
    public void Restore()
    {
        Environment.SetEnvironmentVariable(ModuleRootVariable, m_priorModuleRoot);
        TestHost.PublishEnvironmentVariable(SandboxMarker, m_priorMarker);
        runtime_package.GoModuleSourceRemap = m_priorRemap;
    }

    [TestMethod]
    public void AFileOfTheModuleUnderTestIsAnsweredUnderTheStagedCopy()
    {
        Assert.AreEqual(Staged + "/keys.go", runtime_package.GoRemapModuleSourceProbe(Cache + "/keys.go", Cache, Staged));
        Assert.AreEqual(Staged + "/parse/parse.go", runtime_package.GoRemapModuleSourceProbe(Cache + "/parse/parse.go", Cache, Staged));
    }

    // The two ends arrive as the host spells them (a Windows path from GetFullPath and Path.Combine); the
    // recorded path is forward-slashed, so both ends are too, and a trailing separator changes nothing.
    [TestMethod]
    public void BothEndsAreReadForwardSlashedWithoutATrailingSeparator() =>
        Assert.AreEqual(Staged + "/keys.go", runtime_package.GoRemapModuleSourceProbe(Cache + "/keys.go",
            Cache.Replace('/', '\\') + "\\", Staged.Replace('/', '\\') + "\\"));

    // A DEPENDENCY module sits under its own root, and Go names it in the cache too: unchanged. So is a
    // sibling whose name merely starts with the module root's (the match is at a path boundary).
    [TestMethod]
    public void AFileOutsideTheModuleRootIsUnchanged()
    {
        foreach (string file in new[]
                 {
                     "/mod/cache/example.test/other@v2.0.0/other.go",
                     Cache + "x/keys.go",
                     "/mod/cache/example.test/keys@v1.0.1/keys.go",
                     "runtime/extern.go",
                     "keys.go",
                     Cache,
                 })
        {
            Assert.AreEqual(file, runtime_package.GoRemapModuleSourceProbe(file, Cache, Staged), $"{file} was remapped");
        }
    }

    [TestMethod]
    public void NoRemapAnswersEveryFileAsItIs()
    {
        Assert.AreEqual(Cache + "/keys.go", runtime_package.GoRemapModuleSourceProbe(Cache + "/keys.go", "", Staged));
        Assert.AreEqual(Cache + "/keys.go", runtime_package.GoRemapModuleSourceProbe(Cache + "/keys.go", Cache, ""));
    }

    // One directory is spelled several ways on Windows (drive letter case, above all); elsewhere a path is
    // exact.
    [TestMethod]
    public void TheMatchFollowsTheHostFilesystemsCase()
    {
        string answered = runtime_package.GoRemapModuleSourceProbe("C:/Mod/Cache/keys@v1.0.0/keys.go", "c:/mod/cache/keys@v1.0.0", Staged);

        if (OperatingSystem.IsWindows())
            Assert.AreEqual(Staged + "/keys.go", answered);
        else
            Assert.AreEqual("C:/Mod/Cache/keys@v1.0.0/keys.go", answered);
    }

    // A converted PROGRAM never runs a host, so it has no remap; and no earlier host run in this process left
    // one behind (the host arms below would leave one if the withdrawal were missing).
    [TestMethod]
    public void AProcessOutsideAHostRunHasNoRemap() =>
        Assert.IsNull(m_priorRemap, $"a remap is registered outside any host run: {m_priorRemap}");

    private static TestRegistry Registry(string package, string? modulePath)
    {
        TestRegistry registry = new(package, [], modulePath: modulePath);

        registry.Add("TestProbe", _ =>
        {
            s_insideRemap = runtime_package.GoModuleSourceRemap;
            s_insideDirectory = Environment.CurrentDirectory;
        }, "keys_test.go", 1);

        return registry;
    }

    private static string NewModule()
    {
        string root = Path.Combine(Path.GetTempPath(), "g2cs-modremap-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "go.mod"), $"module {ModulePath}\n\ngo 1.23\n");
        File.WriteAllText(Path.Combine(root, "keys.go"), "package keys\n");
        return root;
    }

    private static string Slashed(string path) => Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');

    [TestMethod]
    public void AModuleHostRegistersItsStagedCopyAndWithdrawsItAfterTheRun()
    {
        string module = NewModule();

        try
        {
            Environment.SetEnvironmentVariable(ModuleRootVariable, module);

            Assert.AreEqual(0, TestHost.Run(Registry(ModulePath, ModulePath), []));
            Assert.IsNotNull(s_insideDirectory, "the probe test did not run");
            Assert.IsNotNull(s_insideRemap, "a module host registered no remap");

            (string from, string to) = s_insideRemap!.Value;
            Assert.AreEqual(Slashed(module), from, "the remap's root is not the module root the run handed over");
            Assert.AreEqual(Slashed(s_insideDirectory!), to, "the remap's copy is not the module root package's working directory");

            Assert.IsNull(runtime_package.GoModuleSourceRemap, "the run's remap outlived it: a later run would answer this module under a deleted sandbox");
        }
        finally
        {
            try
            {
                Directory.Delete(module, true);
            }
            catch (Exception)
            {
            }
        }
    }

    // A RE-EXEC'D HELPER runs the same package's code in the sandbox its parent staged, so its frames name the
    // same copy; it registers from the root it inherited (the sandbox marker), and withdraws it after. A
    // helper whose inherited sandbox holds no copy registers nothing.
    [TestMethod]
    public void AReExecdHelperRegistersItsParentsCopy()
    {
        string module = NewModule();
        string parentRoot = Path.Combine(Path.GetTempPath(), "g2cs-modremap-parent-" + Guid.NewGuid().ToString("N"));

        try
        {
            Environment.SetEnvironmentVariable(ModuleRootVariable, module);
            TestHost.PublishEnvironmentVariable(SandboxMarker, parentRoot);

            Directory.CreateDirectory(parentRoot);
            Assert.AreEqual(0, TestHost.Run(Registry(ModulePath, ModulePath), []));
            Assert.IsNotNull(s_insideDirectory, "the probe test did not run");
            Assert.IsNull(s_insideRemap, $"a helper whose inherited sandbox holds no copy registered a remap: {s_insideRemap}");

            string copy = PackageAncestry.ModuleMirrorRoot(parentRoot, ModulePath);
            Directory.CreateDirectory(copy);
            Assert.AreEqual(0, TestHost.Run(Registry(ModulePath, ModulePath), []));
            Assert.IsNotNull(s_insideRemap, "a re-exec'd helper registered no remap for its parent's copy");
            Assert.AreEqual((Slashed(module), Slashed(copy)), s_insideRemap!.Value);

            Assert.IsNull(runtime_package.GoModuleSourceRemap, "the helper's remap outlived its run");
            Assert.IsTrue(Directory.Exists(copy), "the helper deleted its parent's sandbox");
        }
        finally
        {
            foreach (string root in new[] { module, parentRoot })
            {
                try
                {
                    Directory.Delete(root, true);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    // A standard-library host has no module root: it registers nothing, and its frames are what they were.
    [TestMethod]
    public void AStandardLibraryHostRegistersNothing()
    {
        Assert.AreEqual(0, TestHost.Run(Registry("guard/modremap", null), []));
        Assert.IsNotNull(s_insideDirectory, "the probe test did not run");
        Assert.IsNull(s_insideRemap, $"a standard-library host registered a remap: {s_insideRemap}");
    }
}

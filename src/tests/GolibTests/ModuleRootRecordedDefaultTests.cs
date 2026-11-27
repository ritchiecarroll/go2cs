// ModuleRootRecordedDefaultTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// A BY-HAND RUN OF A MODULE HOST NEEDS NO GO2CS_MODULE_ROOT WHEN THE CONVERSION RECORDED ONE (COORD 20261126T182438Z,
// the T17 seat after S5). A module host stages a copy of its module and refused when the variable was unset
// (ModuleStagingRefusalTests), so a hand `dotnet run` of a host the converter wrote outside a go2cs root failed until
// the user set it. The converter knows the module's directory, records it in the build files beside the host, and the
// SDK writes it into the host's runtimeconfig as go2cs.ModuleRoot. What wins, top to bottom: the variable (any value,
// `none` included), then the recorded root, then today's refusal. A recorded root that is not a module refuses as a
// set variable does, and says where the value came from, so a moved tree names its stale default.
[TestClass]
public class ModuleRootRecordedDefaultTests
{
    private const string ModulePath = "example.test/recorded";
    private const string ModuleRootVariable = "GO2CS_MODULE_ROOT";
    private const string RecordedModuleRootKey = "go2cs.ModuleRoot";
    private const string SandboxMarker = "GO2CS_TEST_SANDBOX";

    private string? m_priorModuleRoot;
    private string? m_priorMarker;
    private static bool s_probeRan;
    private static bool s_probeSawGoMod;

    [TestInitialize]
    public void StartWithNoModuleRootAndNoMarker()
    {
        m_priorModuleRoot = Environment.GetEnvironmentVariable(ModuleRootVariable);
        m_priorMarker = Environment.GetEnvironmentVariable(SandboxMarker);
        Environment.SetEnvironmentVariable(ModuleRootVariable, null);
        TestHost.PublishEnvironmentVariable(SandboxMarker, null);
        AppContext.SetData(RecordedModuleRootKey, null);
        s_probeRan = false;
        s_probeSawGoMod = false;
    }

    [TestCleanup]
    public void Restore()
    {
        AppContext.SetData(RecordedModuleRootKey, null);
        Environment.SetEnvironmentVariable(ModuleRootVariable, m_priorModuleRoot);
        TestHost.PublishEnvironmentVariable(SandboxMarker, m_priorMarker);
    }

    private static TestRegistry Registry()
    {
        TestRegistry registry = new(ModulePath, [], modulePath: ModulePath);

        registry.Add("TestProbe", _ =>
        {
            s_probeRan = true;
            s_probeSawGoMod = File.Exists("go.mod");
        }, "recorded_test.go", 1);

        return registry;
    }

    private static string NewModule(bool withGoMod)
    {
        string path = Path.Combine(Path.GetTempPath(), "g2cs-modrecorded-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);

        if (withGoMod)
            File.WriteAllText(Path.Combine(path, "go.mod"), $"module {ModulePath}\n\ngo 1.23\n");

        File.WriteAllText(Path.Combine(path, "recorded.go"), "package recorded\n");
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

    private static int RunCapturingStderr(out string stderr)
    {
        TextWriter prior = Console.Error;
        StringWriter captured = new();
        Console.SetError(captured);

        try
        {
            return TestHost.Run(Registry(), []);
        }
        finally
        {
            Console.SetError(prior);
            stderr = captured.ToString();
        }
    }

    // The recorded root stages the module when the variable is unset: the probe runs in the module's copy.
    [TestMethod]
    public void TheRecordedRootStagesTheModuleWhenTheVariableIsUnset()
    {
        string module = NewModule(withGoMod: true);

        try
        {
            AppContext.SetData(RecordedModuleRootKey, module);

            int exit = RunCapturingStderr(out string stderr);

            Assert.AreEqual(0, exit, $"a module host with a recorded root did not run (exit {exit}); stderr: {stderr}");
            Assert.IsTrue(s_probeRan, "the probe test did not run");
            Assert.IsTrue(s_probeSawGoMod, "the probe ran outside the module's staged copy");
        }
        finally
        {
            Discard(module);
        }
    }

    // The variable wins over the recorded root, so the pipeline (which always sets it) never reads the record.
    [TestMethod]
    public void TheVariableWinsOverTheRecordedRoot()
    {
        string module = NewModule(withGoMod: true);
        string stale = NewModule(withGoMod: false);

        try
        {
            AppContext.SetData(RecordedModuleRootKey, stale);
            Environment.SetEnvironmentVariable(ModuleRootVariable, module);

            int exit = RunCapturingStderr(out string stderr);

            Assert.AreEqual(0, exit, $"the variable did not win over a stale recorded root (exit {exit}); stderr: {stderr}");
            Assert.IsTrue(s_probeSawGoMod, "the probe ran outside the module's staged copy");
        }
        finally
        {
            Discard(stale);
            Discard(module);
        }
    }

    // `none` in the variable is still the deliberate opt-out, recorded root or not.
    [TestMethod]
    public void NoneInTheVariableWinsOverTheRecordedRoot()
    {
        string module = NewModule(withGoMod: true);

        try
        {
            AppContext.SetData(RecordedModuleRootKey, module);
            Environment.SetEnvironmentVariable(ModuleRootVariable, "none");

            int exit = RunCapturingStderr(out string stderr);

            Assert.AreEqual(0, exit, $"GO2CS_MODULE_ROOT=none must run; stderr: {stderr}");
            Assert.IsTrue(s_probeRan, "the probe test did not run");
            Assert.IsFalse(s_probeSawGoMod, "the module was staged under GO2CS_MODULE_ROOT=none");
            StringAssert.Contains(stderr, "not staged");
        }
        finally
        {
            Discard(module);
        }
    }

    // A recorded root that is no longer a module refuses, naming where the value came from and how to override it.
    [TestMethod]
    public void AStaleRecordedRootRefusesNamingWhereItCameFrom()
    {
        string stale = NewModule(withGoMod: false);

        try
        {
            AppContext.SetData(RecordedModuleRootKey, stale);

            int exit = RunCapturingStderr(out string stderr);

            Assert.AreEqual(2, exit, $"a stale recorded root ran (exit {exit}); stderr: {stderr}");
            Assert.IsFalse(s_probeRan, "the probe test ran in a sandbox that is not the module");
            StringAssert.Contains(stderr, "go.mod");
            StringAssert.Contains(stderr, "recorded when the host was converted");
            StringAssert.Contains(stderr, ModuleRootVariable);
        }
        finally
        {
            Discard(stale);
        }
    }
}

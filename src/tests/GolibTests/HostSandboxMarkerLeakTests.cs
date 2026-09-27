// HostSandboxMarkerLeakTests.cs - Gbtc
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

// A HOST RUN LEAVES NO SANDBOX MARKER BEHIND (i9, 2026-09-27, COORD ruling). TestHost.Run publishes
// GO2CS_TEST_SANDBOX, in the CLR environment and in the converted syscall environment, so that a helper
// it re-executes can recognize itself and keep the directory its parent chose. A real converted test
// binary runs one host per process, so the variable dies with the process. The in-process MSTest tier
// runs many hosts in ONE process, and the variable outlived the run that published it: every later run
// read its predecessor's (already deleted) run root, took itself for a re-exec'd helper, and ran with
// no sandbox and no fixture copy, in the caller's working directory. Measured at 458b56f6e5 through
// BehavioralTests' SetenvTempDirAndFixturesAreIsolated: it passes alone and fails after ANY earlier
// host run, because its fixture write lands on the fixture SOURCE beside the test assembly.
//
// Each arm starts from an absent marker on both sides (a leak from an earlier test in this process must
// not decide the verdict) and restores what it found afterwards. The first run of the second arm is the
// positive control: it must be sandboxed too, or the "sandboxed" reading would measure nothing.
[TestClass]
public class HostSandboxMarkerLeakTests
{
    private const string SandboxMarker = "GO2CS_TEST_SANDBOX";

    private string? m_priorMarker;
    private static string? s_insideDirectory;

    [TestInitialize]
    public void StartFromAnAbsentMarker()
    {
        m_priorMarker = Environment.GetEnvironmentVariable(SandboxMarker);
        TestHost.PublishEnvironmentVariable(SandboxMarker, null);
        Assert.IsFalse(MarkerIsPresent(out string where), $"the precondition did not take: the marker is still set in {where}");
        s_insideDirectory = null;
    }

    [TestCleanup]
    public void RestoreTheMarker() =>
        TestHost.PublishEnvironmentVariable(SandboxMarker, m_priorMarker);

    private static TestRegistry Registry()
    {
        TestRegistry registry = new("guard/sandboxmarker", []);
        registry.Add("TestProbe", _ => s_insideDirectory = Environment.CurrentDirectory, "guard_test.go", 1);
        return registry;
    }

    // Present on EITHER side: the CLR store (what the host's helper gate reads on the way in) or the
    // converted syscall store (what a converted child's environment is built from).
    private static bool MarkerIsPresent(out string where)
    {
        bool clr = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(SandboxMarker));
        (_, bool converted) = syscall_package.Getenv(SandboxMarker);
        where = clr && converted ? "both environments" : clr ? "the CLR environment" : converted ? "the converted environment" : "neither";
        return clr || converted;
    }

    private static string RunAndReportInsideDirectory()
    {
        s_insideDirectory = null;
        Assert.AreEqual(0, TestHost.Run(Registry(), []));
        Assert.IsNotNull(s_insideDirectory, "the probe test did not run");
        return s_insideDirectory!;
    }

    private static bool SameDirectory(string left, string right) =>
        string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

    [TestMethod]
    public void TheSandboxMarkerIsAbsentAfterARun()
    {
        RunAndReportInsideDirectory();
        Assert.IsFalse(MarkerIsPresent(out string where), $"the run's sandbox marker outlived it, in {where}");
    }

    [TestMethod]
    public void ASecondRunInOneProcessIsSandboxed()
    {
        string caller = Environment.CurrentDirectory;

        // The marker readings are recorded and asserted LAST, so the sandboxing assertions are reached
        // (and red on a leak) rather than shadowed by the marker assertion that would fail first.
        string first = RunAndReportInsideDirectory();
        bool leakedAfterFirst = MarkerIsPresent(out string afterFirst);

        string second = RunAndReportInsideDirectory();
        bool leakedAfterSecond = MarkerIsPresent(out string afterSecond);

        Assert.IsFalse(SameDirectory(first, caller), "control: the FIRST run was not sandboxed, so this arm measures nothing");
        Assert.IsFalse(SameDirectory(second, caller), "the second run in this process ran in the caller's directory: it took itself for a re-exec'd helper");
        Assert.IsFalse(SameDirectory(second, first), "the second run reused the first run's sandbox");
        Assert.IsFalse(leakedAfterFirst, $"the first run's sandbox marker outlived it, in {afterFirst}");
        Assert.IsFalse(leakedAfterSecond, $"the second run's sandbox marker outlived it, in {afterSecond}");
    }
}

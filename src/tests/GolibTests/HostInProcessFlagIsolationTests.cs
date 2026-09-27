// HostInProcessFlagIsolationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// EACH IN-PROCESS TestHost.Run IS A FRESH FLAG PARSE (i9, 2026-09-27, COORD ruling). A real converted
// test binary runs TestHost.Run exactly once per process, as Go runs one flag.Parse() per test binary.
// The in-process MSTest tier runs many hosts in ONE process against ONE process-global
// flag.CommandLine, and TestFlagBridge.Parse returns early when flag.Parsed() is already true. So the
// first run to reach m.Run parsed and every later run in the process parsed NOTHING: an undefined flag
// went unrejected (exit 0), a package flag kept its old value, and -h ran the tests. Measured at master
// 1dae85e093 through BehavioralTests' TestingRuntimeTests: -nosuchflag exited 0 where Go exits 2, on
// every host.
//
// Each arm owns an ExitOnError CommandLine (the real binary's default), with one flag registered on it
// BEFORE any run, the way a converted package-level `var x = flag.String(...)` is. Every arm's FIRST run
// is a VALID command line, so the arm is red at master (a later run parses nothing) rather than
// aborting the test host through ExitOnError's os.Exit. The last arm owns a ContinueOnError set, whose
// Go contract is that an undefined flag is IGNORED (flag.Parse drops the error), and the host must
// honour that rather than emulate ExitOnError over it.
[TestClass]
public class HostInProcessFlagIsolationTests
{
    private const string Preregistered = "go2cs-guard-preregistered";

    private static ж<flag_package.FlagSet>? s_original;
    private static ж<@string>? s_preregisteredValue;
    private static bool s_ran;

    [TestInitialize]
    public void OwnAnExitOnErrorCommandLine()
    {
        s_original = flag_package.CommandLine;
        flag_package.CommandLine = flag_package.NewFlagSet("guard", flag_package.ExitOnError);
        s_preregisteredValue = flag_package.String(Preregistered, "default", "registered before any run");
        s_ran = false;
    }

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        if (s_original is not null)
            flag_package.CommandLine = s_original;
    }

    private static TestRegistry Registry()
    {
        TestRegistry registry = new("guard/flagisolation", []);
        registry.Add("TestProbe", _ => s_ran = true, "guard_test.go", 1);
        return registry;
    }

    private static int Run(params string[] args)
    {
        s_ran = false;
        return TestHost.Run(Registry(), args);
    }

    [TestMethod]
    public void AnUndefinedFlagIsRejectedOnALaterRunNotOnlyOnTheFirst()
    {
        Assert.AreEqual(0, Run("-v"));
        Assert.IsTrue(s_ran, "the valid first run must run its test");

        Assert.AreEqual(2, Run("-nosuchflag"),
            "a later in-process run did not parse its command line: Go exits 2 on an undefined flag");
        Assert.IsFalse(s_ran, "a rejected command line must not run any test");
    }

    [TestMethod]
    public void AFlagRegisteredBeforeTheRunParsesOnALaterRunAndResetsAfterIt()
    {
        Assert.AreEqual(0, Run("-v"));

        Assert.AreEqual(0, Run($"-{Preregistered}=set"));
        Assert.AreEqual("set", s_preregisteredValue!.Value.ToString(),
            "a flag registered before the run (a package initializer's) was not parsed on a later run");

        Assert.AreEqual(0, Run());
        Assert.AreEqual("default", s_preregisteredValue!.Value.ToString(),
            "a flag set by an earlier run kept its value: a fresh process starts every flag at its default");
    }

    [TestMethod]
    public void HelpExitsZeroWithoutRunningTestsOnALaterRun()
    {
        Assert.AreEqual(0, Run("-v"));

        Assert.AreEqual(0, Run("-h"), "Go's ExitOnError exits 0 for ErrHelp");
        Assert.IsFalse(s_ran, "-h must not run the tests");
    }

    [TestMethod]
    public void TheCommandLinesErrorHandlingIsRestoredAfterARun()
    {
        Assert.AreEqual(0, Run("-v"));
        Assert.AreEqual(2, Run("-nosuchflag"));

        Assert.AreEqual(flag_package.ExitOnError, flag_package.CommandLine.ErrorHandling(),
            "the run left the process's CommandLine with a different error-handling mode");
    }

    [TestMethod]
    public void AContinueOnErrorCommandLineKeepsGosContractAndIgnoresTheError()
    {
        flag_package.CommandLine = flag_package.NewFlagSet("guard", flag_package.ContinueOnError);

        Assert.AreEqual(0, Run("-v"));
        Assert.AreEqual(0, Run("-nosuchflag"), "flag.Parse ignores the error under ContinueOnError; the host must not emulate ExitOnError over it");
        Assert.IsTrue(s_ran, "under ContinueOnError the run proceeds, as Go's m.Run does");
    }
}

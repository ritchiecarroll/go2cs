// SchedulerProcsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Text.RegularExpressions;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The M:N scheduler's P count follows GOMAXPROCS, as Go's does (COORD's ruling for .5, C1/20261122T141803Z): the
/// GOMAXPROCS environment variable at start, and runtime.GOMAXPROCS(n) while the program runs. GOMAXPROCS(n) returns
/// the previous setting, and n &lt; 1 only reads it. With the scheduler off the setting is still a remembered value that
/// caps nothing, but the scheduler's count follows it all the same, so the count a later start uses is the setting.
/// </summary>
[TestClass]
public class SchedulerProcsTests
{
    [TestMethod]
    public void ThePCountFollowsRuntimeGOMAXPROCS()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GO2CS_MN_PROCS")))
            Assert.Inconclusive("GO2CS_MN_PROCS pins the P count for this run: NOT MEASURED");

        nint before = runtime_package.GOMAXPROCS(0);

        try
        {
            nint target = before == 2 ? 3 : 2;

            Assert.AreEqual(before, runtime_package.GOMAXPROCS(target), "GOMAXPROCS(n) returns the previous setting");
            Assert.AreEqual(target, runtime_package.GOMAXPROCS(0), "n < 1 reads the setting");
            Assert.AreEqual(target, runtime_package.GOMAXPROCS(-1), "and changes nothing");
            Assert.AreEqual((long)target, Procs(), "the scheduler's P count follows the setting");
            Assert.AreEqual(Expected(target), GoScheduler.FreePs, "and so do its P tokens, all free with no step running");

            Assert.AreEqual(target, runtime_package.GOMAXPROCS(target + 1));
            Assert.AreEqual((long)target + 1, Procs(), "upward too");
            Assert.AreEqual(Expected(target + 1), GoScheduler.FreePs);
        }
        finally
        {
            runtime_package.GOMAXPROCS(before);
        }

        Assert.AreEqual((long)before, Procs(), "and back");
        Assert.AreEqual(Expected(before), GoScheduler.FreePs, "a P removed takes its free token with it");
    }

    // With the scheduler off no step holds a P, so every token is free; with it on (GO2CS_MN=1) steps may hold some.
    private static int Expected(nint procs) => GoScheduler.Enabled ? GoScheduler.FreePs : (int)procs;

    // The scheduler's own count, from its one-line reading.
    private static long Procs()
    {
        Match match = Regex.Match(GoScheduler.Reading(), @"\bprocs=(?<value>\d+)");
        Assert.IsTrue(match.Success, $"the reading names procs: {GoScheduler.Reading()}");

        return long.Parse(match.Groups["value"].Value);
    }
}

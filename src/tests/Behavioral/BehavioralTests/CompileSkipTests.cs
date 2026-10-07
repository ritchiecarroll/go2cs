// CompileSkipTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

// CompileCSProject reuses a fixture's executable instead of rebuilding it when ExecutableIsCurrent says so, and the Output
// tier calls it UNFORCED. A fixture's .cs are not its only build inputs: golib, the source generator and every converted
// package it references compile into the program too. Until the converter wrote unchanged sources byte-compared
// (incremental .cs writes), a converter rebuild rewrote every .cs and so forced the build; since then, a golib or
// generator change with an unchanged emission left every .cs older than the executable, and the Output test ran the
// PREVIOUS program. Measured 2026-10-05 on ZeroValueStructVar with a golib module initializer that prints a marker: the
// Output test passed on the stale executable after a golib-only change and after a converter rebuild plus a golib change,
// and failed (correctly, with the marker) only once the fixture's .cs were touched.
[TestClass]
public sealed class CompileSkipTests
{
    private static readonly DateTime s_old = new(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static string Fixture(DateTime exeTime, DateTime csTime)
    {
        string dir = Path.Combine(Path.GetTempPath(), $"go2cs-compile-skip-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);

        string cs = Path.Combine(dir, "main.cs");
        File.WriteAllText(cs, "// a fixture source");
        File.SetLastWriteTimeUtc(cs, csTime);

        string exe = Path.Combine(dir, "main.exe");
        File.WriteAllText(exe, "an earlier build");
        File.SetLastWriteTimeUtc(exe, exeTime);

        return dir;
    }

    [TestMethod]
    public void AnExecutableOlderThanASharedBuildInputIsRebuilt()
    {
        string dir = Fixture(exeTime: s_old.AddHours(2), csTime: s_old.AddHours(1));

        try
        {
            // The fixture's own .cs is older than the executable, but a shared input (golib, the generator, a converted
            // package) changed after it was built.
            Assert.IsFalse(BehavioralTestBase.ExecutableIsCurrent(Path.Combine(dir, "main.exe"), dir, s_old.AddHours(3)),
                "an executable built before a shared build input changed was reused: the Output test would run the previous program");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void AnExecutableNewerThanEveryInputIsReused()
    {
        string dir = Fixture(exeTime: s_old.AddHours(3), csTime: s_old.AddHours(1));

        try
        {
            Assert.IsTrue(BehavioralTestBase.ExecutableIsCurrent(Path.Combine(dir, "main.exe"), dir, s_old.AddHours(2)),
                "an executable newer than its .cs and every shared input must be reused, or the skip is gone and every Output test rebuilds");
            Assert.IsFalse(BehavioralTestBase.ExecutableIsCurrent(Path.Combine(dir, "missing.exe"), dir, s_old),
                "a missing executable can never be current");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}

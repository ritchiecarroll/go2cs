// GodebugUpdateHookTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using godebug = go.@internal.godebug_package;
using os = go.os_package;

namespace GolibTests;

// INTERNAL/GODEBUG'S CACHE IS KEPT BY THE RUNTIME'S UPDATE HOOK (C2, 2026-10-20, COORD ruling (B)
// for claude/c2-godebug-immutable). The hand-own transcribes Go's godebug.go: init registers
// update(def, env) through runtime.godebugSetUpdate, the runtime calls it at once with the startup
// $GODEBUG, and again on every os.Setenv/Unsetenv of GODEBUG (syscall_runtimeSetenv's
// godebugNotify). The hand-own it replaced read the CLR environment instead, which on unix
// os.Setenv never writes (syscall keeps Go's own envs, as Go does without cgo): it saw t.Setenv,
// which writes both stores, and never saw os.Setenv. Go 1.26's TestImmutable was green against
// it only because no os.Setenv in the test reached Value at all.
//
// Arms:
//   1. in process: an os.Setenv of GODEBUG reaches Value, and an os.Unsetenv clears it. Red on
//      unix against the CLR-reading hand-own (the value stays "").
//   2. a child process (one per class; both arms read its record): with no fips140 entry cached,
//      os.Setenv fips140=on then fips140=off leaves the FIRST New("fips140").Value() reading "on".
//      The on parse creates the entry, and the off update finds it Immutable and skips it. Go
//      measured: a program importing only internal/godebug and os prints "on". A Go TEST binary
//      prints "" instead, because it links crypto/internal/fips140, whose init calls
//      godebug.Value("#fips140") and caches the startup value first. That is why this arm runs in
//      a child: a GolibTests process may already hold the entry.
//   3. the same child: a name that only the startup $GODEBUG sets reads its value with no Setenv.
//      That is the runtime's first update, made inside setUpdate, seeing the env the runtime's
//      module initializer stored. Red when the registration does not notify.
//
// The child is this assembly run again by `dotnet test`, filtered to ChildRecordsItsReadings and
// told to run by GO2CS_GODEBUG_CHILD_RECORD (the path it writes its readings to), Go's helper-
// process pattern. A child that never ran writes no record, and the parent fails on the missing
// file rather than passing on an empty one.
[TestClass]
public class GodebugUpdateHookTests
{
    private const string RecordVariable = "GO2CS_GODEBUG_CHILD_RECORD";
    private const string StartupName = "go2csstartup";
    private const string StartupValue = "start";

    [TestMethod]
    public void AnOsSetenvOfGodebugReachesValue()
    {
        (@string previous, bool wasSet) = os.LookupEnv("GODEBUG");
        ж<godebug.Setting> setting = godebug.New("#go2cssetenv");

        try
        {
            os.Setenv("GODEBUG", "go2cssetenv=bar");
            Assert.AreEqual("bar", (string)godebug.Value(setting), "after os.Setenv GODEBUG=go2cssetenv=bar");

            os.Setenv("GODEBUG", "go2cssetenv=baz");
            Assert.AreEqual("baz", (string)godebug.Value(setting), "after os.Setenv GODEBUG=go2cssetenv=baz");

            os.Unsetenv("GODEBUG");
            Assert.AreEqual("", (string)godebug.Value(setting), "after os.Unsetenv GODEBUG");
        }
        finally
        {
            if (wasSet)
                os.Setenv("GODEBUG", previous);
            else
                os.Unsetenv("GODEBUG");
        }
    }

    [TestMethod]
    public void AnImmutableSettingKeepsTheValueAnEarlierSetenvCached()
    {
        Assert.AreEqual("on", Child.Value["intermediate"], "first New(\"fips140\").Value() after os.Setenv fips140=on, then fips140=off");
    }

    [TestMethod]
    public void TheFirstUpdateParsesTheStartupEnvironment()
    {
        Assert.AreEqual(StartupValue, Child.Value["startup"], $"New(\"#{StartupName}\").Value() with GODEBUG={StartupName}={StartupValue} at process start");
    }

    // The child's half. Outside a child it returns at once, as Go's helper processes do.
    [TestMethod]
    public void ChildRecordsItsReadings()
    {
        string? record = Environment.GetEnvironmentVariable(RecordVariable);

        if (string.IsNullOrEmpty(record))
            return;

        // Read before any Setenv: only the registration's update can have cached it.
        string startup = godebug.Value(godebug.New("#" + StartupName));

        os.Setenv("GODEBUG", "fips140=on");
        os.Setenv("GODEBUG", "fips140=off");
        string intermediate = godebug.Value(godebug.New("fips140"));

        File.WriteAllLines(record, [$"startup={startup}", $"intermediate={intermediate}"]);
    }

    private static readonly Lazy<Dictionary<string, string>> Child = new(RunChild);

    private static Dictionary<string, string> RunChild()
    {
        string record = Path.Combine(Path.GetTempPath(), "g2cs-godebug-" + Guid.NewGuid().ToString("N") + ".txt");
        string filter = $"FullyQualifiedName={typeof(GodebugUpdateHookTests).FullName}.{nameof(ChildRecordsItsReadings)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(GodebugUpdateHookTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[RecordVariable] = record;
        start.Environment["GODEBUG"] = $"{StartupName}={StartupValue}";

        try
        {
            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(180_000))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail("the godebug child did not finish in 180 s");
            }

            Assert.AreEqual(0, process.ExitCode, $"godebug child:\n{stdout.Result}{stderr.Result}");
            Assert.IsTrue(File.Exists(record), $"the godebug child wrote no record:\n{stdout.Result}{stderr.Result}");

            Dictionary<string, string> readings = new(StringComparer.Ordinal);

            foreach (string line in File.ReadAllLines(record))
            {
                int eq = line.IndexOf('=');
                readings[line[..eq]] = line[(eq + 1)..];
            }

            return readings;
        }
        finally
        {
            File.Delete(record);
        }
    }
}

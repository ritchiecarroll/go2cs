// DarwinCpuArm64AgainstGoTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// D4's row (DESIGN-darwin-arm64-flavour.md, section 5): the converted <c>internal/cpu.ARM64</c> on a Mac, field by
/// field, against what Go's own <c>internal/cpu</c> reads on the same host (InternalCpuArm64Reading). Each darwin leg
/// runs it in its own flavour: osx-arm64 in the darwin/arm64 flavour on Apple silicon, where Go's osInit reads the
/// hw.optional sysctls and assumes AES, PMULL, SHA1 and SHA2; osx-x64 in the darwin/amd64 flavour on an Intel Mac,
/// where Go fills no ARM64 field and neither may the converted side. Every field is compared but HasDIT, the disclosed
/// departure (the darwin ruling, 2026-11-06: the CLR exposes no PSTATE.DIT control, so the converted side holds it
/// false; DarwinArm64CpuStartupTests asserts that). Darwin only: GolibTests.csproj excludes this file unless
/// $(GoTargetOS) is darwin. Inconclusive only where no pinned Go toolchain resolves, which the legs' expected-skip
/// manifests refuse, since each leg exports GOROOT.
/// </summary>
[TestClass]
public class DarwinCpuArm64AgainstGoTests
{
    private static readonly HashSet<string> s_departures = new(StringComparer.Ordinal) { "HasDIT" };

    [TestMethod]
    public void DarwinCpuArm64MatchesGosOwnReading()
    {
        string godebug = Environment.GetEnvironmentVariable("GODEBUG") ?? "";

        if (godebug.Split(',').Any(kv => kv.Trim().StartsWith("cpu.", StringComparison.Ordinal)))
            Assert.Inconclusive($"GODEBUG sets cpu.* options ({godebug}); the two sides would apply them differently");

        (Dictionary<string, bool>? go, string? skip) = InternalCpuArm64Reading.TakeGoReading();

        if (go is null)
        {
            Assert.Inconclusive(skip);
            return;
        }

        Dictionary<string, bool> converted = InternalCpuArm64Reading.Converted();
        string host = $"{RuntimeInformation.OSDescription.Trim()}, process {RuntimeInformation.ProcessArchitecture}";

        Console.WriteLine($"READING Go internal/cpu.ARM64 ({host}): {Render(go)}");
        Console.WriteLine($"READING converted internal/cpu.ARM64 ({host}): {Render(converted)}");

        List<string> differences = InternalCpuArm64Reading.Differences(go, converted, s_departures);

        Assert.AreEqual(0, differences.Count,
            $"internal/cpu.ARM64 differs from Go's own reading on {host}:\n  {string.Join("\n  ", differences)}");
    }

    private static string Render(Dictionary<string, bool> fields) =>
        string.Join(' ', fields.OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => $"{f.Key}={(f.Value ? 1 : 0)}"));
}

// InternalCpuGodebugTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.@internal;
using X86 = System.Runtime.Intrinsics.X86;

namespace GolibTests;

/// <summary>
/// Guards internal/cpu's start-up GODEBUG handling (cpu_x86_impl.cs): doinit's option table is
/// registered on every OS and GODEBUG's cpu.* options are applied only off Windows, as Go's
/// getGodebugEarly does. An empty or non-cpu GODEBUG leaves every flag alone, and cpu.aes=off clears
/// HasAES and nothing else.
/// </summary>
[TestClass]
public class InternalCpuGodebugTests
{
    [TestMethod]
    public void GodebugIsAppliedOnlyOffWindows()
    {
        Assert.AreEqual(!OperatingSystem.IsWindows(), cpu_package.GoCpuAppliesGodebug);
    }

    [TestMethod]
    public void AnEmptyOrNonCpuGodebugLeavesEveryFlagAlone()
    {
        RequireX86();
        (string name, bool enabled)[] empty = cpu_package.GoCpuOptionsProbe("");

        Assert.AreEqual(20, empty.Length, "doinit's table at GOAMD64 level 1");
        Assert.AreEqual(cpu_package.X86.HasAES, Flag(empty, "aes"));
        Assert.AreEqual(cpu_package.X86.HasSSE41, Flag(empty, "sse41"));
        Assert.AreEqual(cpu_package.X86.HasAVX2, Flag(empty, "avx2"));
        CollectionAssert.AreEqual(empty, cpu_package.GoCpuOptionsProbe("gctrace=0,madvdontneed=1"), "non-cpu keys change nothing");
    }

    [TestMethod]
    public void CpuAesOffClearsHasAesAndNothingElse()
    {
        RequireX86();
        (string name, bool enabled)[] before = cpu_package.GoCpuOptionsProbe("");
        (string name, bool enabled)[] after = cpu_package.GoCpuOptionsProbe("cpu.aes=off");

        Assert.IsFalse(Flag(after, "aes"), "cpu.aes=off clears HasAES");
        CollectionAssert.AreEqual(before.Where(o => o.name != "aes").ToArray(), after.Where(o => o.name != "aes").ToArray(), "every other flag is untouched");
        Assert.AreEqual(X86.Aes.IsSupported, cpu_package.X86.HasAES, "the probe restores the live flag");
    }

    private static bool Flag((string name, bool enabled)[] options, string name) => options.Single(o => o.name == name).enabled;

    private static void RequireX86()
    {
        if (!X86.X86Base.IsSupported)
            Assert.Inconclusive("not an x86 host: internal/cpu registers no x86 options here");
    }
}

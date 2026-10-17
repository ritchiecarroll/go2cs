// RuntimeCpuinitFlagsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.@internal;
using X86 = System.Runtime.Intrinsics.X86;

namespace GolibTests;

/// <summary>
/// Guards the copy half of runtime.cpuinit (cpuflags_impl.cs): the runtime's own x86 feature flags,
/// which Go copies from internal/cpu during schedinit, hold internal/cpu's answers once the runtime
/// assembly is loaded. The runtime's TestHasAVX reads x86HasAVX the same way and failed with it false
/// on an AVX host. Red plant: empty the module initializer's x86 branch (every flag reads false).
/// </summary>
[TestClass]
public class RuntimeCpuinitFlagsTests
{
    [TestMethod]
    public void RuntimeX86FlagsMatchInternalCpu()
    {
        RequireX86();
        (bool avx, bool fma, bool popcnt, bool sse41) flags = runtime_package.GoX86CpuFlags;

        Assert.AreEqual(cpu_package.X86.HasAVX, flags.avx, "x86HasAVX");
        Assert.AreEqual(cpu_package.X86.HasFMA, flags.fma, "x86HasFMA");
        Assert.AreEqual(cpu_package.X86.HasPOPCNT, flags.popcnt, "x86HasPOPCNT");
        Assert.AreEqual(cpu_package.X86.HasSSE41, flags.sse41, "x86HasSSE41");
    }

    [TestMethod]
    public void RuntimeX86HasAVXIsTrueOnAnAVXHost()
    {
        RequireX86();

        if (!X86.Avx.IsSupported)
            Assert.Inconclusive("the host has no AVX, so x86HasAVX is rightly false and the arm cannot tell");

        Assert.IsTrue(runtime_package.GoX86CpuFlags.avx, "x86HasAVX on an AVX host");
    }

    private static void RequireX86()
    {
        if (!X86.X86Base.IsSupported)
            Assert.Inconclusive("not an x86 host: Go copies other flags there");
    }
}

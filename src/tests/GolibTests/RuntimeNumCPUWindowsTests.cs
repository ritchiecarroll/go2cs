// RuntimeNumCPUWindowsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// runtime.getCPUCount on Windows (src/core/runtime/windows/os_windows_impl.cs). Go counts the set bits of
// the process affinity mask; the converted body called it through stdcall, which reaches asmcgocall, and
// the runtime's module initializer reads it for NumCPU and the start-up GOMAXPROCS, so the host crashed
// before the first row. Windows-only by construction: the body lives in runtime's windows/ folder.
[TestClass]
public class RuntimeNumCPUWindowsTests
{
    // Process.ProcessorAffinity is GetProcessAffinityMask's process mask on Windows: the same value read
    // through a second route, counted the way Go counts it.
    [TestMethod]
    public void NumCPUCountsTheProcessAffinityMask()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("the windows flavour on another host: GetProcessAffinityMask is not there to read");
        }

        int allowed = BitOperations.PopCount((ulong)(nint)Process.GetCurrentProcess().ProcessorAffinity);

        Assert.AreEqual((nint)allowed, runtime_package.NumCPU(), "runtime.NumCPU against the process affinity mask");
        Assert.IsTrue(runtime_package.GOMAXPROCS(0) >= 1, "GOMAXPROCS at start-up");
    }
}

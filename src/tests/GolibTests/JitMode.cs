// JitMode.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace GolibTests;

// Which JIT the test process runs under, for arms whose Go-observable answer depends on inlining.
internal static class JitMode
{
    // Whether this process JITs every method fully optimized from its first call: tiered compilation off, or its quick
    // JIT off, by environment (DOTNET_ or the legacy COMPlus_ prefix) or by runtimeconfig switch. That is the banks'
    // configuration of record (Release with TieredCompilation=0); tier-0, which inlines nothing, is the other.
    internal static bool OptimizedFromFirstCall()
    {
        static bool Off(string name) =>
            System.Environment.GetEnvironmentVariable("DOTNET_" + name) == "0" ||
            System.Environment.GetEnvironmentVariable("COMPlus_" + name) == "0";

        if (Off("TieredCompilation") || Off("TC_QuickJit"))
            return true;

        return (System.AppContext.TryGetSwitch("System.Runtime.TieredCompilation", out bool tiered) && !tiered) ||
            (System.AppContext.TryGetSwitch("System.Runtime.TieredCompilation.QuickJit", out bool quick) && !quick);
    }
}

// cpuflags_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's own CPU feature flags (cpuflags.go: x86HasAVX, x86HasFMA, x86HasPOPCNT, x86HasSSE41
// and the arm/arm64/loong64/riscv64 ones) are set in ONE place in Go: cpuinit (proc.go), which
// schedinit calls before alginit. cpuinit does two things: it calls internal/cpu.Initialize, then it
// copies the flags the compiler's generated guards read out of cpu.X86 (or cpu.ARM, ...) into these
// variables.
//
// go2cs never runs schedinit, so cpuinit never ran and every flag stayed false. internal/cpu does not
// need it: cpu_x86_impl.cs fills cpu.X86 from its own module initializer, the stand-in for
// Initialize. The copy half had no stand-in. The runtime's TestHasAVX reads x86HasAVX through
// export_test.go and compares it with what a child process finds; on an AVX host it read false.
//
// The module initializer below is the copy half of cpuinit, line for line. It does not call
// cpu.Initialize: internal/cpu's initializer has already run it (with GODEBUG's cpu.* options off
// Windows) by the time cpu.X86 can be read, and running it again would re-apply the options.
//
// cpuinit itself stays converted in proc.go and unreachable, like the rest of schedinit.

using System.Runtime.CompilerServices;
using cpu = go.@internal.cpu_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛCpuinitFlags()
    {
        var arch = GOARCH;

        if (arch == "386"u8 || arch == "amd64"u8) {
            x86HasAVX = cpu.X86.HasAVX;
            x86HasFMA = cpu.X86.HasFMA;
            x86HasPOPCNT = cpu.X86.HasPOPCNT;
            x86HasSSE41 = cpu.X86.HasSSE41;
        }
        else if (arch == "arm"u8) {
            armHasVFPv4 = cpu.ARM.HasVFPv4;
        }
        else if (arch == "arm64"u8) {
            arm64HasATOMICS = cpu.ARM64.HasATOMICS;
        }
        else if (arch == "loong64"u8) {
            loong64HasLAMCAS = cpu.Loong64.HasLAMCAS;
            loong64HasLAM_BH = cpu.Loong64.HasLAM_BH;
            loong64HasLSX = cpu.Loong64.HasLSX;
        }
        else if (arch == "riscv64"u8) {
            riscv64HasZbb = cpu.RISCV64.HasZbb;
        }
    }

    // TEST SEAM (goenvs_impl.cs's pattern: GolibTests is outside the InternalsVisibleTo grant): the four
    // x86 flags as the module initializer left them.
    public static (bool avx, bool fma, bool popcnt, bool sse41) GoX86CpuFlags =>
        (x86HasAVX, x86HasFMA, x86HasPOPCNT, x86HasSSE41);
}

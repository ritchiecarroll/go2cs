// cpu_arm64_hwcap_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The linux/arm64 flavour's internal/cpu start-up: what Go's runtime does before any package init
// reads a CPU flag, which go2cs never runs (S3 site 0, docs/phase4/DATA-la64-s3-census.md, the
// 2026-11-09 amendment).
//
// IN GO, TWO STEPS, BOTH IN THE RUNTIME. archauxv (runtime/os_linux_arm64.go) copies AT_HWCAP from
// the auxiliary vector into cpu.HWCap, and schedinit's cpuinit calls cpu.Initialize(env), which runs
// doinit (cpu_arm64.go: the option table, then osInit → hwcapInit, which maps HWCap's bits to
// cpu.ARM64) and then processOptions over getGodebugEarly's GODEBUG. schedinit never runs here, so
// nothing would fill HWCap and nothing would call Initialize: every cpu.ARM64 flag would read false,
// and with them the runtime's AES hash selection, the crypto and bytealg CPU branches.
//
// THIS FILE IS THOSE TWO STEPS, AT THE SLOT EVERY OTHER FLAVOUR USES: a module initializer, the stand-in
// for schedinit's slot that cpu_x86_impl.cs and cpu_arm64_process_impl.cs (both amd64/, so neither is in
// this build) explain. It asks libc for the same auxiliary-vector entry, getauxval(AT_HWCAP), stores it in
// the converted HWCap as archauxv does, and calls the CONVERTED Initialize. The mapping is therefore Go's
// own converted hwcapInit (cpu_arm64_hwcap.cs), not a hand copy of it: the copy S4 wrote
// (linux-amd64/cpu_arm64_process_linux_impl.cs) serves an arm64 process running the amd64-converted
// flavour, a path S2 retires at linux/arm64. arm64 reads only AT_HWCAP: Go declares HWCap2 for arm and
// ppc64, not for arm64. .NET's System.Runtime.Intrinsics.Arm classes are a cross-check, never the source.
//
// GODEBUG: getGodebugEarly answers GODEBUG on linux, so cpu.aes=off clears ARM64.HasAES before the
// runtime's hash selection reads it, as in Go.
//
// TWO DEPARTURES FROM GO, both S4's on the same host and for the same causes:
//
//   - HasDIT is cleared after Initialize, the darwin ruling (COORD, 2026-11-06;
//     darwin/cpu_arm64_darwin_impl.cs states the cause). The .NET runtime exposes no control for
//     PSTATE.DIT, so a true flag would be a false security claim. Go sets it from HWCAP_DIT, and the
//     converted hwcapInit does too; no GODEBUG option names it, so clearing it after processOptions
//     loses nothing a user could set.
//
//   - getMIDR reads sysfs. Go's getMIDR executes MRS MIDR_EL1, which the kernel traps and emulates when
//     HWCAP_CPUID is set (hwcapInit calls it only then). Managed code cannot issue that instruction, so
//     the body below reads the same register for cpu0 from
//     /sys/devices/system/cpu/cpu0/regs/identification/midr_el1, which the kernel publishes on every
//     arm64 system since 4.11. Go reads the CPU its thread happens to run on; on a system that mixes
//     core types the two can differ. If the file cannot be read it answers 0, whose implementer byte
//     is not 'A', so IsNeoverse stays false. Only the isNeoverse GODEBUG option reads it in the corpus.
//
// getisar0 and getpfr0 keep their generated stubs: parseARM64SystemRegisters, their only caller, is
// reached from the BSDs' osInit, never from linux's.
//
// LIBC: getauxval is glibc 2.16 and musl 1.1.0. "libc" binds on glibc, as every other libc import in the
// linux flavour does; a musl target is the same open question syscall_linux_impl.cs records.
// UNSAFE: [LibraryImport] requires /unsafe (SYSLIB1062); internal.cpu.csproj already allows it.
[module: go.GoManualConversion]
[module: go.GoRequiresUnsafe]

namespace go.@internal;

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

partial class cpu_package {

// AT_HWCAP, from runtime/os_linux.go (_AT_HWCAP).
private const nuint AT_HWCAP = 16;

// The kernel's MIDR_EL1 for cpu0, as arch/arm64/kernel/cpuinfo.c publishes it ("0x%016llx\n").
private const string midrPath = "/sys/devices/system/cpu/cpu0/regs/identification/midr_el1";

[LibraryImport("libc", EntryPoint = "getauxval")]
private static partial nuint libcGetauxval(nuint type);

/// <summary>
/// Fills <see cref="HWCap"/> as archauxv does, then runs the converted <see cref="Initialize"/>, standing in
/// for the <c>schedinit</c> slot where Go calls <c>cpuinit</c>.
/// </summary>
[ModuleInitializer]
internal static void initLinuxArm64FeatureDetection() {
    HWCap = libcGetauxval(AT_HWCAP);

    Initialize(Environment.GetEnvironmentVariable("GODEBUG") ?? "");

    // The header's first departure.
    ARM64.HasDIT = false;
}

/// <summary>
/// GolibTests' probe (LinuxArm64CpuStartupTests; S3's site 0 reading, COORD ruling 2026-11-10): applies
/// <paramref name="godebug"/> exactly as start-up does. The converted doinit registers cpu_arm64.go's option table and
/// maps HWCap through hwcapInit; processOptions applies the GODEBUG options; HasDIT is cleared as above. The probe
/// returns each option's name and resulting flag, then restores the flags and the table. Re-running doinit gives a
/// fresh table, because processOptions leaves its Specified/Enable marks on the options it read.
/// </summary>
public static (string name, bool enabled)[] GoLinuxArm64OptionsProbe(string godebug) {
    ARM64ᴛ1 saved = ARM64;
    slice<option> savedOptions = options;

    try {
        doinit();
        processOptions(godebug);
        ARM64.HasDIT = false;

        (string, bool)[] result = new (string, bool)[len(options)];

        for (int i = 0; i < result.Length; i++)
            result[i] = (options[i].Name.ToString(), options[i].Feature.Value);

        return result;
    }
    finally {
        ARM64 = saved;
        options = savedOptions;
    }
}

// cpu_arm64.go's getMIDR, from the kernel's copy of the register (the header's second departure).
internal static partial uint64 getMIDR() {
    try {
        string text = File.ReadAllText(midrPath).Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];
        return ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong midr) ? midr : 0;
    }
    catch (IOException) {
        return 0;
    }
    catch (UnauthorizedAccessException) {
        return 0;
    }
}

} // end cpu_package

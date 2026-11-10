// cpu_arm64_process_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// linux's osInit for an arm64 process: cpu_arm64_linux.go's osInit and cpu_arm64_hwcap.go's hwcapInit,
// which the linux/amd64 conversion does not build. cpu_arm64_process_impl.cs calls it, and only on an
// arm64 process; it explains why the corpus needs it. Without a body here the call reaches the
// generated stub, which throws in internal/cpu's module initializer, so every converted program on
// linux-arm64 died before main (calibration run 42727040322: GolibTests 2523 of 2523 failed at init).
//
// THE SOURCE IS THE KERNEL'S WORD, AS IN GO. Go's runtime copies AT_HWCAP from the auxiliary vector
// into cpu.HWCap (runtime/os_linux_arm64.go archauxv), and hwcapInit maps its bits to cpu.ARM64. Here
// schedinit never runs, so nothing fills HWCap: this file asks libc for the same auxiliary-vector
// entry with getauxval(AT_HWCAP), at the point where Go's doinit runs, and applies hwcapInit's mapping
// verbatim. arm64 reads only AT_HWCAP: Go declares HWCap2 for arm and ppc64 and not for arm64, and
// none of the ARM64 flags comes from AT_HWCAP2. .NET's System.Runtime.Intrinsics.Arm classes are a
// GolibTests cross-check (InternalCpuGodebugTests), never the source.
//
// TWO DEPARTURES FROM GO:
//
//   - HasDIT stays FALSE, the darwin ruling (COORD, 2026-11-06; darwin/cpu_arm64_darwin_impl.cs states
//     the cause). The .NET runtime exposes no control for PSTATE.DIT, so a true flag would be a false
//     security claim. Go would set it from HWCAP_DIT.
//
//   - MIDR comes from sysfs. Go's getMIDR executes MRS MIDR_EL1, which the kernel traps and emulates
//     when HWCAP_CPUID is set. Managed code cannot issue that instruction, so this reads the same
//     register for cpu0 from /sys/devices/system/cpu/cpu0/regs/identification/midr_el1, which the
//     kernel publishes on every arm64 system since 4.11. Go reads the CPU its thread happens to run on;
//     on a system that mixes core types the two can differ. If the file cannot be read, IsNeoverse
//     stays false. Only the isNeoverse GODEBUG option reads it in the converted corpus.
//
// HasATOMICS: Go clears it on android (issue 28431). go2cs ships no android RID, so os is "linux" here.
//
// LIBC: getauxval is glibc 2.16 and musl 1.1.0. "libc" binds on glibc, as every other libc import in
// the linux flavour does; a musl target is the same open question syscall_linux_impl.cs records.
// UNSAFE: [LibraryImport] requires /unsafe (SYSLIB1062), so this file declares it and the converter raises
// internal.cpu.csproj's <AllowUnsafeBlocks> to true (projectFileWriter.go allowUnsafeBlocks).
[module: go.GoManualConversion]
[module: go.GoRequiresUnsafe]

namespace go.@internal;

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

partial class cpu_package {

// AT_HWCAP, from runtime/os_linux.go (_AT_HWCAP).
private const nuint AT_HWCAP = 16;

// HWCAP bits. These are exposed by Linux.
// See arch/arm64/include/uapi/asm/hwcap.h.
private const nuint hwcap_AES = 1 << 3;
private const nuint hwcap_PMULL = 1 << 4;
private const nuint hwcap_SHA1 = 1 << 5;
private const nuint hwcap_SHA2 = 1 << 6;
private const nuint hwcap_CRC32 = 1 << 7;
private const nuint hwcap_ATOMICS = 1 << 8;
private const nuint hwcap_CPUID = 1 << 11;
private const nuint hwcap_SHA3 = 1 << 17;
private const nuint hwcap_SHA512 = 1 << 21;
private const nuint hwcap_DIT = 1 << 24;

// The kernel's MIDR_EL1 for cpu0, as arch/arm64/kernel/cpuinfo.c publishes it ("0x%016llx\n").
private const string midrPath = "/sys/devices/system/cpu/cpu0/regs/identification/midr_el1";

[LibraryImport("libc", EntryPoint = "getauxval")]
private static partial nuint libcGetauxval(nuint type);

static partial void osInitArm64() {
    hwcapInit(libcGetauxval(AT_HWCAP));
}

// cpu_arm64_hwcap.go's hwcapInit, with os fixed to "linux".
private static void hwcapInit(nuint hwCap) {
    // HWCap was populated by the runtime from the auxiliary vector.
    // See https://docs.kernel.org/arch/arm64/elf_hwcaps.html.
    // Use HWCap information since reading aarch64 system registers
    // is not supported in user space on older linux kernels.
    ARM64.HasAES = !hwcapIsSet(hwCap, hwcap_AES); // PLANTED RED: inverted
    ARM64.HasPMULL = hwcapIsSet(hwCap, hwcap_PMULL);
    ARM64.HasSHA1 = hwcapIsSet(hwCap, hwcap_SHA1);
    ARM64.HasSHA2 = hwcapIsSet(hwCap, hwcap_SHA2);
    ARM64.HasSHA3 = hwcapIsSet(hwCap, hwcap_SHA3);
    ARM64.HasCRC32 = hwcapIsSet(hwCap, hwcap_CRC32);
    ARM64.HasCPUID = hwcapIsSet(hwCap, hwcap_CPUID);
    ARM64.HasSHA512 = hwcapIsSet(hwCap, hwcap_SHA512);

    // ARM64.HasDIT stays false: the header's ruled departure.

    // The Samsung S9+ kernel reports support for atomics, but not all cores
    // actually support them, resulting in SIGILL. See issue #28431.
    ARM64.HasATOMICS = hwcapIsSet(hwCap, hwcap_ATOMICS);

    // Check to see if executing on a Neoverse core and in order to do that,
    // check the AUXV for the CPUID bit. Go executes MRS MIDR_EL1, which the
    // kernel traps only when the CPUID bit is set; this reads the kernel's
    // copy instead (the header's second departure).
    if (ARM64.HasCPUID && tryReadMIDR(out ulong midr)) {
        ushort part_num = (ushort)((midr >> 4) & 0xfff);
        byte implementer = (byte)((midr >> 24) & 0xff);

        // d0c - NeoverseN1
        // d40 - NeoverseV1
        // d49 - NeoverseN2
        // d4f - NeoverseV2
        // d8e - NeoverseN3
        // d84 - NeoverseV3
        // d83 - NeoverseV3ae
        if (implementer == (byte)'A') {
            switch (part_num) {
            case 0xd0c or 0xd40 or 0xd49 or 0xd4f or 0xd8e or 0xd84 or 0xd83:
                ARM64.IsNeoverse = true;
                break;
            }
        }
    }
}

// cpu_arm64_hwcap.go's isSet. Renamed: cpu_x86.go declares an isSet(uint32, uint32) in this class.
private static bool hwcapIsSet(nuint hwc, nuint value) => (hwc & value) != 0;

private static bool tryReadMIDR(out ulong midr) {
    midr = 0;
    try {
        string text = File.ReadAllText(midrPath).Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];
        return ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out midr);
    }
    catch (IOException) {
        return false;
    }
    catch (UnauthorizedAccessException) {
        return false;
    }
}

} // end cpu_package

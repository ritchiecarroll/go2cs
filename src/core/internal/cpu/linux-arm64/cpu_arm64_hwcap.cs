// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build arm64 && linux
namespace go.@internal;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname

partial class cpu_package {

// HWCap may be initialized by archauxv and
// should not be changed after it was initialized.
//
// Other widely used packages
// access HWCap using linkname as well, most notably:
//   - github.com/klauspost/cpuid/v2
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname HWCap
public static readonly ж<nuint> ᏑHWCap = new StaticBox<nuint>(default(nuint));
public static ref nuint HWCap => ref ((StaticBox<nuint>)ᏑHWCap).ValueSlot;

// HWCAP bits. These are exposed by Linux.
// See arch/arm64/include/uapi/asm/hwcap.h.
internal static UntypedInt hwcap_AES => /* 1 << 3 */ 8;
internal static UntypedInt hwcap_PMULL => /* 1 << 4 */ 16;
internal static UntypedInt hwcap_SHA1 => /* 1 << 5 */ 32;
internal static UntypedInt hwcap_SHA2 => /* 1 << 6 */ 64;
internal static UntypedInt hwcap_CRC32 => /* 1 << 7 */ 128;
internal static UntypedInt hwcap_ATOMICS => /* 1 << 8 */ 256;
internal static UntypedInt hwcap_CPUID => /* 1 << 11 */ 2048;
internal static UntypedInt hwcap_SHA3 => /* 1 << 17 */ 131072;
internal static UntypedInt hwcap_SHA512 => /* 1 << 21 */ 2097152;
internal static UntypedInt hwcap_DIT => /* 1 << 24 */ 16777216;

internal static void hwcapInit(@string os) {
    // HWCap was populated by the runtime from the auxiliary vector.
    // See https://docs.kernel.org/arch/arm64/elf_hwcaps.html.
    // Use HWCap information since reading aarch64 system registers
    // is not supported in user space on older linux kernels.
    ARM64.HasAES = isSet(HWCap, hwcap_AES);
    ARM64.HasPMULL = isSet(HWCap, hwcap_PMULL);
    ARM64.HasSHA1 = isSet(HWCap, hwcap_SHA1);
    ARM64.HasSHA2 = isSet(HWCap, hwcap_SHA2);
    ARM64.HasSHA3 = isSet(HWCap, hwcap_SHA3);
    ARM64.HasCRC32 = isSet(HWCap, hwcap_CRC32);
    ARM64.HasCPUID = isSet(HWCap, hwcap_CPUID);
    ARM64.HasSHA512 = isSet(HWCap, hwcap_SHA512);
    ARM64.HasDIT = isSet(HWCap, hwcap_DIT);

    // The Samsung S9+ kernel reports support for atomics, but not all cores
    // actually support them, resulting in SIGILL. See issue #28431.
    // TODO(elias.naur): Only disable the optimization on bad chipsets on android.
    ARM64.HasATOMICS = isSet(HWCap, hwcap_ATOMICS) && os != "android"u8;

    // Check to see if executing on a Neoverse core and in order to do that,
    // check the AUXV for the CPUID bit. The getMIDR function executes an
    // instruction which would normally be an illegal instruction, but it's
    // trapped by the kernel, the value sanitized and then returned.
    // Without the CPUID bit the kernel will not trap the instruction and the
    // process will be terminated with SIGILL.
    if (ARM64.HasCPUID) {
        var midr = getMIDR();
        var part_num = (uint16)((midr >> 4) & 0xfff);
        var implementer = (byte)((midr >> 24) & 0xff);

        // d0c - NeoverseN1
        // d40 - NeoverseV1
        // d49 - NeoverseN2
        // d4f - NeoverseV2
        // d8e - NeoverseN3
        // d84 - NeoverseV3
        // d83 - NeoverseV3ae
        if (implementer == 'A') {
            switch (part_num) {
            case 0xd0c or 0xd40 or 0xd49 or 0xd4f or 0xd8e or 0xd84 or 0xd83: {
                ARM64.IsNeoverse = true;
                break;
            }}
        }
    }
}

internal static bool isSet(nuint hwc, nuint value) {
    return (hwc & value) != 0;
}

} // end cpu_package

// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

using runtime = runtime_package;

partial class cpu_package {

// cacheLineSize is used to prevent false sharing of cache lines.
// We choose 128 because Apple Silicon, a.k.a. M1, has 128-byte cache line size.
// It doesn't cost much and is much more future-proof.
internal static UntypedInt cacheLineSize => 128;

internal static void initOptions() {
    options = [
        new(Name: "fp", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasFP)),
        new(Name: "asimd", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasASIMD)),
        new(Name: "evstrm", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasEVTSTRM)),
        new(Name: "aes", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasAES)),
        new(Name: "fphp", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasFPHP)),
        new(Name: "jscvt", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasJSCVT)),
        new(Name: "lrcpc", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasLRCPC)),
        new(Name: "pmull", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasPMULL)),
        new(Name: "sha1", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA1)),
        new(Name: "sha2", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA2)),
        new(Name: "sha3", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA3)),
        new(Name: "sha512", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA512)),
        new(Name: "sm3", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSM3)),
        new(Name: "sm4", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSM4)),
        new(Name: "sve", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSVE)),
        new(Name: "sve2", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSVE2)),
        new(Name: "crc32", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCRC32)),
        new(Name: "atomics", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasATOMICS)),
        new(Name: "asimdhp", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasASIMDHP)),
        new(Name: "cpuid", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCPUID)),
        new(Name: "asimrdm", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasASIMDRDM)),
        new(Name: "fcma", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasFCMA)),
        new(Name: "dcpop", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasDCPOP)),
        new(Name: "asimddp", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasASIMDDP)),
        new(Name: "asimdfhm", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasASIMDFHM)),
        new(Name: "dit", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasDIT)),
        new(Name: "i8mm", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasI8MM))
    ];
}

internal static void archInit() {
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "freebsd"u8: {
        readARM64Registers();
        break;
    }
    case {} when exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8: {
        doinit();
        break;
    }
    default: {
        // Many platforms don't seem to allow reading these registers.
        setMinimalFeatures();
        break;
    }}
}

// setMinimalFeatures fakes the minimal ARM64 features expected by
// TestARM64minimalFeatures.
internal static void setMinimalFeatures() {
    ARM64.HasASIMD = true;
    ARM64.HasFP = true;
}

internal static void readARM64Registers() {
    Initialized = true;

    parseARM64SystemRegisters(getisar0(), getisar1(), getpfr0());
}

internal static void parseARM64SystemRegisters(uint64 isar0, uint64 isar1, uint64 pfr0) {
    // ID_AA64ISAR0_EL1
    switch (extractBits(isar0, 4, 7)) {
    case 1: {
        ARM64.HasAES = true;
        break;
    }
    case 2: {
        ARM64.HasAES = true;
        ARM64.HasPMULL = true;
        break;
    }}

    switch (extractBits(isar0, 8, 11)) {
    case 1: {
        ARM64.HasSHA1 = true;
        break;
    }}

    switch (extractBits(isar0, 12, 15)) {
    case 1: {
        ARM64.HasSHA2 = true;
        break;
    }
    case 2: {
        ARM64.HasSHA2 = true;
        ARM64.HasSHA512 = true;
        break;
    }}

    switch (extractBits(isar0, 16, 19)) {
    case 1: {
        ARM64.HasCRC32 = true;
        break;
    }}

    switch (extractBits(isar0, 20, 23)) {
    case 2: {
        ARM64.HasATOMICS = true;
        break;
    }}

    switch (extractBits(isar0, 28, 31)) {
    case 1: {
        ARM64.HasASIMDRDM = true;
        break;
    }}

    switch (extractBits(isar0, 32, 35)) {
    case 1: {
        ARM64.HasSHA3 = true;
        break;
    }}

    switch (extractBits(isar0, 36, 39)) {
    case 1: {
        ARM64.HasSM3 = true;
        break;
    }}

    switch (extractBits(isar0, 40, 43)) {
    case 1: {
        ARM64.HasSM4 = true;
        break;
    }}

    switch (extractBits(isar0, 44, 47)) {
    case 1: {
        ARM64.HasASIMDDP = true;
        break;
    }}

    // ID_AA64ISAR1_EL1
    switch (extractBits(isar1, 0, 3)) {
    case 1: {
        ARM64.HasDCPOP = true;
        break;
    }}

    switch (extractBits(isar1, 12, 15)) {
    case 1: {
        ARM64.HasJSCVT = true;
        break;
    }}

    switch (extractBits(isar1, 16, 19)) {
    case 1: {
        ARM64.HasFCMA = true;
        break;
    }}

    switch (extractBits(isar1, 20, 23)) {
    case 1: {
        ARM64.HasLRCPC = true;
        break;
    }}

    switch (extractBits(isar1, 52, 55)) {
    case 1: {
        ARM64.HasI8MM = true;
        break;
    }}

    // ID_AA64PFR0_EL1
    switch (extractBits(pfr0, 16, 19)) {
    case 0: {
        ARM64.HasFP = true;
        break;
    }
    case 1: {
        ARM64.HasFP = true;
        ARM64.HasFPHP = true;
        break;
    }}

    switch (extractBits(pfr0, 20, 23)) {
    case 0: {
        ARM64.HasASIMD = true;
        break;
    }
    case 1: {
        ARM64.HasASIMD = true;
        ARM64.HasASIMDHP = true;
        break;
    }}

    switch (extractBits(pfr0, 32, 35)) {
    case 1: {
        ARM64.HasSVE = true;

        parseARM64SVERegister(getzfr0());
        break;
    }}

    switch (extractBits(pfr0, 48, 51)) {
    case 1: {
        ARM64.HasDIT = true;
        break;
    }}
}

internal static void parseARM64SVERegister(uint64 zfr0) {
    switch (extractBits(zfr0, 0, 3)) {
    case 1: {
        ARM64.HasSVE2 = true;
        break;
    }}
}

internal static nuint extractBits(uint64 data, nuint start, nuint end) {
    return (nuint)(data.Rsh(start)) & ((((nuint)1).Lsh((end - start + 1))) - 1);
}

} // end cpu_package

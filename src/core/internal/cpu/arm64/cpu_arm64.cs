// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class cpu_package {

// CacheLinePadSize is used to prevent false sharing of cache lines.
// We choose 128 because Apple Silicon, a.k.a. M1, has 128-byte cache line size.
// It doesn't cost much and is much more future-proof.
public static UntypedInt CacheLinePadSize => 128;

internal static void doinit() {
    options = [
        new(Name: "aes", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasAES)),
        new(Name: "pmull", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasPMULL)),
        new(Name: "sha1", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA1)),
        new(Name: "sha2", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA2)),
        new(Name: "sha512", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA512)),
        new(Name: "sha3", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA3)),
        new(Name: "crc32", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCRC32)),
        new(Name: "atomics", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasATOMICS)),
        new(Name: "cpuid", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCPUID)),
        new(Name: "isNeoverse", Feature: ᏑARM64.of(ARM64ᴛ1.ᏑIsNeoverse))
    ];

    // arm64 uses different ways to detect CPU features at runtime depending on the operating system.
    osInit();
}

internal static partial uint64 getisar0();

internal static partial uint64 getpfr0();

internal static partial uint64 getMIDR();

internal static nuint extractBits(uint64 data, nuint start, nuint end) {
    return (nuint)(data.Rsh(start)) & ((((nuint)1).Lsh((end - start + 1))) - 1);
}

internal static void parseARM64SystemRegisters(uint64 isar0, uint64 pfr0) {
    // ID_AA64ISAR0_EL1
    // https://developer.arm.com/documentation/ddi0601/2025-03/AArch64-Registers/ID-AA64ISAR0-EL1--AArch64-Instruction-Set-Attribute-Register-0
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

    switch (extractBits(isar0, 32, 35)) {
    case 1: {
        ARM64.HasSHA3 = true;
        break;
    }}

    switch (extractBits(pfr0, 48, 51)) {
    case 1: {
        ARM64.HasDIT = true;
        break;
    }}
}

} // end cpu_package

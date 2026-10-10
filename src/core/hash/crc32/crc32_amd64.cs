// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// AMD64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted crc32_amd64.go output).
//
// Every Go-level declaration below is the converted output verbatim — the triple-buffer split,
// the CRC(I, ABC) combining identity and the table construction are pure Go logic that converts
// faithfully. Only two things cannot be a literal conversion:
//
//  1. castagnoliSSE42, castagnoliSSE42Triple and ieeeCLMUL have NO Go body: they are declared in
//     crc32_amd64.go and implemented in crc32_amd64.s. A literal conversion emits a bodyless
//     `partial` that PartialStubGenerator fills with a NotImplementedException. They are realized
//     here against System.Runtime.Intrinsics.X86 — the SAME instructions the .s file issues
//     (CRC32B/W/L/Q from SSE4.2, PCLMULQDQ, and PEXTRD from SSE4.1) — so the arch layer is real
//     hardware acceleration on .NET, not a stub. Each port is annotated against the .s labels it
//     transcribes.
//
//  2. The capability guards read internal/cpu's `cpu.X86.HasSSE42` / `HasPCLMULQDQ` / `HasSSE41`.
//     Those flags are GLOBAL: they gate the arch path of every converted package, and the rest of
//     those arch layers are still PartialStubGenerator stubs. Turning them on centrally would trade
//     this package's working portable fallback for a NotImplementedException in the others, so the
//     probe is LOCAL to this file — the `.IsSupported` properties of exactly the instruction sets
//     used here. That keeps the claim precisely true ("these instructions are available to THIS
//     code") and leaves internal/cpu untouched. This is the general recipe for realizing an
//     asm-backed arch layer in managed code; see docs/ConversionStrategies-Reference.md.
//
//  3. The corpus is converted at GOARCH=amd64, so this file is the arch layer an arm64 process runs
//     too. Go on arm64 compiles crc32_arm64.go instead, whose layer is the ARMv8 CRC32 instructions
//     (crc32_arm64.s: CRC32X/W/H/B for IEEE, CRC32CX/CW/CH/CB for Castagnoli) gated on
//     cpu.ARM64.HasCRC32. Without an arm64 arm here, an arm64 process reported no arch layer while
//     Go on the same machine had one, and TestArchIEEE / TestArchCastagnoli skipped in C# where Go
//     ran them (osx-arm64, os-matrix run 38036662620). The arm64 arm transcribes crc32_arm64.go and
//     .s against System.Runtime.Intrinsics.Arm.Crc32, probed locally for the reason in (2). The x86
//     path keeps precedence, so an x64 process is unchanged.
//
// Where no intrinsic is available (a CPU with neither the x86 instructions above nor the ARMv8 CRC32
// instructions), archAvailable* returns false and crc32.go falls back to slicing-by-8 — exactly how
// Go degrades on an architecture with no arch implementation.
[module: go.GoManualConversion]

namespace go.hash;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using ArmCrc32 = System.Runtime.Intrinsics.Arm.Crc32;

partial class crc32_package {

// This file contains the code to call the SSE 4.2 version of the Castagnoli
// and IEEE CRC.

// hasSSE42 replaces cpu.X86.HasSSE42 with the local probe described in the file header. The X64
// form is required because both Castagnoli ports accumulate through CRC32Q (the 8-byte encoding),
// matching this file's GOARCH=amd64 origin; a 32-bit process reports false and takes slicing-by-8.
internal static bool hasSSE42 => Sse42.X64.IsSupported;

// hasCLMUL replaces cpu.X86.HasPCLMULQDQ && cpu.X86.HasSSE41 — ieeeCLMUL needs PCLMULQDQ for the
// folding steps and SSE4.1 for the final PEXTRD.
internal static bool hasCLMUL => Pclmulqdq.IsSupported && Sse41.IsSupported;

// hasArmCRC32 replaces cpu.ARM64.HasCRC32 for the arm64 arm (header, point 3). The Arm64 form is
// required because both arm64 ports consume 8-byte words (CRC32X / CRC32CX), as crc32_arm64.s does.
internal static bool hasArmCRC32 => ArmCrc32.Arm64.IsSupported;

// castagnoliUpdate updates the non-inverted crc with the given data, using the ARMv8 CRC32C
// instructions. Transcribes crc32_arm64.s ·castagnoliUpdate: the .s loop's LDP pairs are two 8-byte
// words in sequence, so an 8-byte loop followed by the 4/2/1-byte tail consumes the same bytes in the
// same order.
internal static uint32 castagnoliUpdate(uint32 crc, slice<byte> p) {
    nint length = len(p);
    if (length == 0) {
        return crc;
    }
    ref byte data = ref MemoryMarshal.GetReference(p.ToSpan());
    nint i = 0;
    for (; length - i >= 8; i += 8) {
        crc = ArmCrc32.Arm64.ComputeCrc32C(crc, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref data, i)));
    }
    if (length - i >= 4) {
        crc = ArmCrc32.ComputeCrc32C(crc, Unsafe.ReadUnaligned<uint32>(ref Unsafe.Add(ref data, i)));
        i += 4;
    }
    if (length - i >= 2) {
        crc = ArmCrc32.ComputeCrc32C(crc, Unsafe.ReadUnaligned<uint16>(ref Unsafe.Add(ref data, i)));
        i += 2;
    }
    if (length - i >= 1) {
        crc = ArmCrc32.ComputeCrc32C(crc, Unsafe.Add(ref data, i));
    }
    return crc;
}

// ieeeUpdate updates the non-inverted crc with the given data, using the ARMv8 CRC32 instructions.
// Transcribes crc32_arm64.s ·ieeeUpdate, in the same shape as castagnoliUpdate above.
internal static uint32 ieeeUpdate(uint32 crc, slice<byte> p) {
    nint length = len(p);
    if (length == 0) {
        return crc;
    }
    ref byte data = ref MemoryMarshal.GetReference(p.ToSpan());
    nint i = 0;
    for (; length - i >= 8; i += 8) {
        crc = ArmCrc32.Arm64.ComputeCrc32(crc, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref data, i)));
    }
    if (length - i >= 4) {
        crc = ArmCrc32.ComputeCrc32(crc, Unsafe.ReadUnaligned<uint32>(ref Unsafe.Add(ref data, i)));
        i += 4;
    }
    if (length - i >= 2) {
        crc = ArmCrc32.ComputeCrc32(crc, Unsafe.ReadUnaligned<uint16>(ref Unsafe.Add(ref data, i)));
        i += 2;
    }
    if (length - i >= 1) {
        crc = ArmCrc32.ComputeCrc32(crc, Unsafe.Add(ref data, i));
    }
    return crc;
}

// castagnoliSSE42 updates the (non-inverted) crc with the given buffer, using the SSE 4.2 CRC32
// instruction. Transcribes crc32_amd64.s ·castagnoliSSE42.
//
// The .s file first walks the buffer to an 8-byte boundary so its CRC32Q loads are aligned. That
// step is a hardware micro-optimization, not part of the result: CRC32 is a pure function of the
// initial value and the byte SEQUENCE, and both forms consume the bytes in the same order. It has
// no managed counterpart either — a slice's backing array can be moved by the GC, so an address
// observed here is not an address the loads would keep — so the port simply runs the CRC32Q loop
// and then the .s file's 4/2/1-byte tail.
internal static uint32 castagnoliSSE42(uint32 crc, slice<byte> p) {
    nint length = len(p);
    if (length == 0) {
        return crc;
    }
    ref byte data = ref MemoryMarshal.GetReference(p.ToSpan());
    // aligned: CRC32Q over each 8-byte chunk.
    uint64 acc = crc;
    nint i = 0;
    for (; length - i >= 8; i += 8) {
        acc = Sse42.X64.Crc32(acc, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref data, i)));
    }
    // less_than_8: the remaining 0-7 bytes as CRC32L, then CRC32W, then CRC32B — the same order
    // the .s file derives from the low three bits of the residual length.
    uint32 c = ((uint32)acc);
    if (length - i >= 4) {
        c = Sse42.Crc32(c, Unsafe.ReadUnaligned<uint32>(ref Unsafe.Add(ref data, i)));
        i += 4;
    }
    if (length - i >= 2) {
        c = Sse42.Crc32(c, Unsafe.ReadUnaligned<uint16>(ref Unsafe.Add(ref data, i)));
        i += 2;
    }
    if (length - i >= 1) {
        c = Sse42.Crc32(c, Unsafe.Add(ref data, i));
    }
    return c;
}

// castagnoliSSE42Triple updates three (non-inverted) crcs with (24*rounds) bytes from each buffer.
// Transcribes crc32_amd64.s ·castagnoliSSE42Triple: each round issues three independent CRC32Q
// chains per buffer so the three dependency chains pipeline against each other.
//
// The .s loop is a do-while driven by DECQ/JNZ, so rounds == 0 would wrap; every caller passes
// K/24 (7 or 56). The managed form is a counted loop, which is identical for all real inputs.
internal static (uint32 retA, uint32 retB, uint32 retC) castagnoliSSE42Triple(uint32 crcA, uint32 crcB, uint32 crcC, slice<byte> a, slice<byte> b, slice<byte> c, uint32 rounds) {
    ref byte pa = ref MemoryMarshal.GetReference(a.ToSpan());
    ref byte pb = ref MemoryMarshal.GetReference(b.ToSpan());
    ref byte pc = ref MemoryMarshal.GetReference(c.ToSpan());
    uint64 accA = crcA;
    uint64 accB = crcB;
    uint64 accC = crcC;
    nint off = 0;
    for (uint32 round = 0; round < rounds; round++) {
        accA = Sse42.X64.Crc32(accA, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pa, off)));
        accB = Sse42.X64.Crc32(accB, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pb, off)));
        accC = Sse42.X64.Crc32(accC, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pc, off)));
        accA = Sse42.X64.Crc32(accA, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pa, off + 8)));
        accB = Sse42.X64.Crc32(accB, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pb, off + 8)));
        accC = Sse42.X64.Crc32(accC, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pc, off + 8)));
        accA = Sse42.X64.Crc32(accA, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pa, off + 16)));
        accB = Sse42.X64.Crc32(accB, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pb, off + 16)));
        accC = Sse42.X64.Crc32(accC, Unsafe.ReadUnaligned<uint64>(ref Unsafe.Add(ref pc, off + 16)));
        off += 24;
    }
    return (((uint32)accA), ((uint32)accB), ((uint32)accC));
}

// CRC32 polynomial data
//
// These constants are lifted from the
// Linux kernel, since they avoid the costly
// PSHUFB 16 byte reversal proposed in the
// original Intel paper.
//
// A 128-bit load of a crc32_amd64.s DATA pair puts the offset-0 quadword in the LOW half, so
// Vector128.Create(offset0, offset8) reproduces the .s register image exactly.
private static readonly Vector128<uint64> s_r2r1 = Vector128.Create(0x154442bd4UL, 0x1c6e41596UL);
private static readonly Vector128<uint64> s_r4r3 = Vector128.Create(0x1751997d0UL, 0x0ccaa009eUL);
private static readonly Vector128<uint64> s_rupoly = Vector128.Create(0x1db710641UL, 0x1f7011641UL);
private static readonly Vector128<uint64> s_r5 = Vector128.Create(0x163cd6124UL, 0UL);

// s_mask32 is the .s file's PCMPEQB X3, X3 followed by PSRLQ $32, X3 — the low 32 bits of each
// quadword. Only the low quadword is ever consumed.
private static readonly Vector128<uint64> s_mask32 = Vector128.Create(0x00000000FFFFFFFFUL, 0x00000000FFFFFFFFUL);

// clmulLoad is the .s file's MOVOU (SI) — an unaligned 128-bit load at a byte offset.
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static Vector128<uint64> clmulLoad(ref byte data, nint offset) {
    return Unsafe.ReadUnaligned<Vector128<uint64>>(ref Unsafe.Add(ref data, offset));
}

// clmulFold advances one 128-bit accumulator by the constant pair k — the .s file's recurring
// PCLMULQDQ $0 / PCLMULQDQ $0x11 / PXOR triple (low x by low k, high x by high k, combined).
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static Vector128<uint64> clmulFold(Vector128<uint64> x, Vector128<uint64> k) {
    return Sse2.Xor(
        Pclmulqdq.CarrylessMultiply(x, k, 0x00),
        Pclmulqdq.CarrylessMultiply(x, k, 0x11));
}

// ieeeCLMUL folds the buffer with PCLMULQDQ as well as SSE 4.1. Transcribes crc32_amd64.s
// ·ieeeCLMUL, label for label. len(p) must be at least 64, and must be a multiple of 16 —
// archUpdateIEEE guarantees both.
//
// Based on https://www.intel.com/content/dam/www/public/us/en/documents/white-papers/fast-crc-computation-generic-polynomials-pclmulqdq-paper.pdf
internal static uint32 ieeeCLMUL(uint32 crc, slice<byte> p) {
    ref byte data = ref MemoryMarshal.GetReference(p.ToSpan());
    nint rem = len(p) - 64;
    nint i = 64;
    // Prime four accumulators with the first 64 bytes; the initial CRC enters the low dword.
    var x1 = Sse2.Xor(clmulLoad(ref data, 0), Vector128.CreateScalar(crc).AsUInt64());
    var x2 = clmulLoad(ref data, 16);
    var x3 = clmulLoad(ref data, 32);
    var x4 = clmulLoad(ref data, 48);
    // loopback64: fold all four by r2r1 and absorb the next 64 bytes.
    for (; rem >= 64; i += 64, rem -= 64) {
        x1 = Sse2.Xor(clmulFold(x1, s_r2r1), clmulLoad(ref data, i));
        x2 = Sse2.Xor(clmulFold(x2, s_r2r1), clmulLoad(ref data, i + 16));
        x3 = Sse2.Xor(clmulFold(x3, s_r2r1), clmulLoad(ref data, i + 32));
        x4 = Sse2.Xor(clmulFold(x4, s_r2r1), clmulLoad(ref data, i + 48));
    }
    // remain64: collapse the four accumulators into x1, folding by r4r3.
    x1 = Sse2.Xor(clmulFold(x1, s_r4r3), x2);
    x1 = Sse2.Xor(clmulFold(x1, s_r4r3), x3);
    x1 = Sse2.Xor(clmulFold(x1, s_r4r3), x4);
    // remain16: absorb each remaining 16-byte block.
    for (; rem >= 16; i += 16, rem -= 16) {
        x1 = Sse2.Xor(clmulFold(x1, s_r4r3), clmulLoad(ref data, i));
    }
    // finish: fold the final 128 bits down to 32 and return them.
    //
    // 128 -> 96: multiply the low half by r4 (r4r3's HIGH quadword) and xor in the high half.
    x1 = Sse2.Xor(Sse2.ShiftRightLogical128BitLane(x1, 8), Pclmulqdq.CarrylessMultiply(s_r4r3, x1, 0x01));
    // 96 -> 64: multiply the low 32 bits by r5 and xor in the value shifted down by 32.
    var shifted = Sse2.ShiftRightLogical128BitLane(x1, 4);
    x1 = Sse2.Xor(Pclmulqdq.CarrylessMultiply(Sse2.And(x1, s_mask32), s_r5, 0x00), shifted);
    // 64 -> 32: Barrett reduction by rupoly — the quotient mu in the HIGH quadword, the polynomial
    // in the low one — then take dword 1 (PEXTRD $1).
    var pending = x1;
    x1 = Pclmulqdq.CarrylessMultiply(Sse2.And(x1, s_mask32), s_rupoly, 0x10);
    x1 = Pclmulqdq.CarrylessMultiply(Sse2.And(x1, s_mask32), s_rupoly, 0x00);
    x1 = Sse2.Xor(x1, pending);
    return Sse41.Extract(x1.AsUInt32(), 1);
}

internal static readonly UntypedInt castagnoliK1 = 168;

internal static readonly UntypedInt castagnoliK2 = 1344;

[GoType("[4]Table")] partial struct sse42Table;

internal static ж<sse42Table> castagnoliSSE42TableK1;

internal static ж<sse42Table> castagnoliSSE42TableK2;

internal static bool archAvailableCastagnoli() {
    return hasSSE42 || hasArmCRC32;
}

internal static void archInitCastagnoli() {
    if (!hasSSE42) {
        // crc32_arm64.go archInitCastagnoli: the arm64 layer needs no tables.
        if (hasArmCRC32) {
            return;
        }
        throw panic("arch-specific Castagnoli not available");
    }
    castagnoliSSE42TableK1 = @new<sse42Table>();
    castagnoliSSE42TableK2 = @new<sse42Table>();
    // See description in updateCastagnoli.
    //    t[0][i] = CRC(i000, O)
    //    t[1][i] = CRC(0i00, O)
    //    t[2][i] = CRC(00i0, O)
    //    t[3][i] = CRC(000i, O)
    // where O is a sequence of K zeros.
    array<byte> tmp = new(1344); /* castagnoliK2 */
    for (nint b = 0; b < 4; b++) {
        for (nint i = 0; i < 256; i++) {
            var val = ((uint32)i).Lsh((uint64)((uint32)(b * 8)));
            castagnoliSSE42TableK1.Value[b][i] = castagnoliSSE42(val, tmp[..(int)(castagnoliK1)]);
            castagnoliSSE42TableK2.Value[b][i] = castagnoliSSE42(val, tmp[..]);
        }
    }
}

// castagnoliShift computes the CRC32-C of K1 or K2 zeroes (depending on the
// table given) with the given initial crc value. This corresponds to
// CRC(crc, O) in the description in updateCastagnoli.
internal static uint32 castagnoliShift(ж<sse42Table> Ꮡtable, uint32 crc) {
    ref var table = ref Ꮡtable.Value;

    return (uint32)((uint32)((uint32)(table[3][(nint)((crc >> (int)(24)))] ^ table[2][(nint)((uint32)(((crc >> (int)(16))) & 0xFF))]) ^ table[1][(nint)((uint32)(((crc >> (int)(8))) & 0xFF))]) ^ table[0][(nint)((uint32)(crc & 0xFF))]);
}

internal static uint32 archUpdateCastagnoli(uint32 crc, slice<byte> p) {
    if (!hasSSE42) {
        // crc32_arm64.go archUpdateCastagnoli.
        if (hasArmCRC32) {
            return ~castagnoliUpdate(~crc, p);
        }
        throw panic("not available");
    }
    // This method is inspired from the algorithm in Intel's white paper:
    //    "Fast CRC Computation for iSCSI Polynomial Using CRC32 Instruction"
    // The same strategy of splitting the buffer in three is used but the
    // combining calculation is different; the complete derivation is explained
    // below.
    //
    // -- The basic idea --
    //
    // The CRC32 instruction (available in SSE4.2) can process 8 bytes at a
    // time. In recent Intel architectures the instruction takes 3 cycles;
    // however the processor can pipeline up to three instructions if they
    // don't depend on each other.
    //
    // Roughly this means that we can process three buffers in about the same
    // time we can process one buffer.
    //
    // The idea is then to split the buffer in three, CRC the three pieces
    // separately and then combine the results.
    //
    // Combining the results requires precomputed tables, so we must choose a
    // fixed buffer length to optimize. The longer the length, the faster; but
    // only buffers longer than this length will use the optimization. We choose
    // two cutoffs and compute tables for both:
    //  - one around 512: 168*3=504
    //  - one around 4KB: 1344*3=4032
    //
    // -- The nitty gritty --
    //
    // Let CRC(I, X) be the non-inverted CRC32-C of the sequence X (with
    // initial non-inverted CRC I). This function has the following properties:
    //   (a) CRC(I, AB) = CRC(CRC(I, A), B)
    //   (b) CRC(I, A xor B) = CRC(I, A) xor CRC(0, B)
    //
    // Say we want to compute CRC(I, ABC) where A, B, C are three sequences of
    // K bytes each, where K is a fixed constant. Let O be the sequence of K zero
    // bytes.
    //
    // CRC(I, ABC) = CRC(I, ABO xor C)
    //             = CRC(I, ABO) xor CRC(0, C)
    //             = CRC(CRC(I, AB), O) xor CRC(0, C)
    //             = CRC(CRC(I, AO xor B), O) xor CRC(0, C)
    //             = CRC(CRC(I, AO) xor CRC(0, B), O) xor CRC(0, C)
    //             = CRC(CRC(CRC(I, A), O) xor CRC(0, B), O) xor CRC(0, C)
    //
    // The castagnoliSSE42Triple function can compute CRC(I, A), CRC(0, B),
    // and CRC(0, C) efficiently.  We just need to find a way to quickly compute
    // CRC(uvwx, O) given a 4-byte initial value uvwx. We can precompute these
    // values; since we can't have a 32-bit table, we break it up into four
    // 8-bit tables:
    //
    //    CRC(uvwx, O) = CRC(u000, O) xor
    //                   CRC(0v00, O) xor
    //                   CRC(00w0, O) xor
    //                   CRC(000x, O)
    //
    // We can compute tables corresponding to the four terms for all 8-bit
    // values.
    crc = ~crc;
    // Go aligns the buffer to an 8 byte boundary here before entering the K2/K1 loops. The managed
    // port drops that step: see the castagnoliSSE42 header — it exists to make the CRC32Q loads
    // aligned, the combining identity below holds for any split of the byte sequence, and a managed
    // slice has no address stable enough to align against.
    // Process 3*K2 at a time.
    while (len(p) >= castagnoliK2 * 3) {
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        var (crcA, crcB, crcC) = castagnoliSSE42Triple(
            crc, 0, 0,
            p, p[(int)(castagnoliK2)..], p[(int)(castagnoliK2 * 2)..],
            castagnoliK2 / 24);
        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        var crcAB = (uint32)(castagnoliShift(castagnoliSSE42TableK2, crcA) ^ crcB);
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = (uint32)(castagnoliShift(castagnoliSSE42TableK2, crcAB) ^ crcC);
        p = p[(int)(castagnoliK2 * 3)..];
    }
    // Process 3*K1 at a time.
    while (len(p) >= castagnoliK1 * 3) {
        // Compute CRC(I, A), CRC(0, B), and CRC(0, C).
        var (crcA, crcB, crcC) = castagnoliSSE42Triple(
            crc, 0, 0,
            p, p[(int)(castagnoliK1)..], p[(int)(castagnoliK1 * 2)..],
            castagnoliK1 / 24);
        // CRC(I, AB) = CRC(CRC(I, A), O) xor CRC(0, B)
        var crcAB = (uint32)(castagnoliShift(castagnoliSSE42TableK1, crcA) ^ crcB);
        // CRC(I, ABC) = CRC(CRC(I, AB), O) xor CRC(0, C)
        crc = (uint32)(castagnoliShift(castagnoliSSE42TableK1, crcAB) ^ crcC);
        p = p[(int)(castagnoliK1 * 3)..];
    }
    // Use the simple implementation for what's left.
    crc = castagnoliSSE42(crc, p);
    return ~crc;
}

internal static bool archAvailableIEEE() {
    return hasCLMUL || hasArmCRC32;
}

internal static ж<slicing8Table> archIeeeTable8;

internal static void archInitIEEE() {
    if (!hasCLMUL) {
        // crc32_arm64.go archInitIEEE: the arm64 layer needs no tables.
        if (hasArmCRC32) {
            return;
        }
        throw panic("not available");
    }
    // We still use slicing-by-8 for small buffers.
    archIeeeTable8 = slicingMakeTable(IEEE);
}

internal static uint32 archUpdateIEEE(uint32 crc, slice<byte> p) {
    if (!hasCLMUL) {
        // crc32_arm64.go archUpdateIEEE.
        if (hasArmCRC32) {
            return ~ieeeUpdate(~crc, p);
        }
        throw panic("not available");
    }
    if (len(p) >= 64) {
        nint left = (nint)(len(p) & 15);
        nint @do = len(p) - left;
        crc = ~ieeeCLMUL(~crc, p[..(int)(@do)]);
        p = p[(int)(@do)..];
    }
    if (len(p) == 0) {
        return crc;
    }
    return slicingUpdate(crc, archIeeeTable8, p);
}

} // end crc32_package

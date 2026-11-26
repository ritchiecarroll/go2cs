// keccakf_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// This file is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - keccakF1600Generic: DERIVED FROM src/crypto/internal/fips140/sha3/keccakf.go, Go 1.26.11, func
//   keccakF1600Generic. Go's rounds, with the state read as uint64 words without the unsafe view.

using System;
using System.Runtime.InteropServices;
using go;
using go.golib;

// The marker every manualConversionFuncs companion carries: keccakF1600Generic is suppressed at
// emission and provided here.
[module: GoManualConversion]

namespace go.crypto.@internal.fips140;

using math;
using bits = math.bits_package;

// Hand-written body for crypto/internal/fips140/sha3's keccakF1600Generic (q97, COORD cc1ef8247).
//
// WHAT WENT WRONG. Go views the sponge state both ways: the [200]byte the sponge absorbs into, and
// the [25]uint64 the permutation computes over. On a little-endian host it takes the view for free:
//
//     a = (*[25]uint64)(unsafe.Pointer(da))
//
// and the converter emitted that literally, as
// `(ж<array<uint64>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡda))`. golib's `array<T>` is a WINDOW
// ON A REAL T[] (array.cs: the indexer reads `Backing[m_low + index]`), so there is no uint64[] for
// a byte[] box to be a window on -- the raw-address route instead materializes an `array<uint64>`
// HEADER out of the buffer's own DATA. That is the same fabrication vendor/…/sha3's xor.cs and
// internal/chacha8rand record, and it is not latent here: `cpu.BigEndian` is a `const bool = false`,
// so the reinterpret is the ONLY REACHABLE branch of this function, on every permutation of every
// SHA-3 and SHAKE call in the corpus.
//
// MEASURED, at the post-fold version tip, in a clean tree with nothing cut -- four reds in
// GolibTests, and the LENGTHS are the tell:
//
//     Sha3ReinterpretVectorTests.FipsVectorsMatch                         index out of range [0] with length 0
//     Sha3ReinterpretVectorTests.ShakeVectorMatches                       index out of range [0] with length 0
//     Sha3ReinterpretVectorTests.MultiBlockAbsorbMatchesTheOsImplementation  ... with length -658924933
//     Sha3ReinterpretVectorTests.UnalignedInputSliceMatchesTheOsImplementation ... with length -540099156
//
// A fresh Digest's state is zeroed, so the first permutation reads a length of 0; the OS-oracle
// vectors hash FILLED patterns, so they read a length off the data bytes -- negative, which is the
// proof the length is content and not metadata. The panic surfaces at the first index
// (golib/array.cs's indexer), not at the cast: the fabricated view is constructed happily and dies
// on read, which is why an arm anchored at the reinterpret would see it succeed.
//
// THE REMEDY IS THE HOUSE ONE, not a new invention: `MemoryMarshal.Cast` over the array's own span,
// which internal/chacha8rand's chacha8_impl.cs takes for the same seam and which
// ArrayShapeReinterpretTests binds directly (TheSpanViewAliasesTheSameStorageAndWritesThrough). A
// span IS a genuine aliasing view of the same backing storage, so every write below lands in the
// caller's [200]byte -- which the sponge REQUIRES, since absorb and squeeze read that same buffer
// between permutations.
//
// ⚠ AND `array<T>` CANNOT CARRY THE RESULT. `array(Span<T>)`, `array(Memory<T>)` and their
// ReadOnly twins all COPY (`AllocationCounter.CopyOf`), and `array<T>.Alias` windows an existing
// T[] of the SAME element type -- neither can express a reinterpret. So the body indexes the SPAN
// directly: 300 `a.Value[i]` sites became `a[i]`, by a checked transform whose counts gated
// (300 -> 0 box indexes, exactly 300 span indexes, line count unmoved, bits. 96 and rc[ 4
// unchanged). The alternative -- reusing the BigEndian copy path Go already ships in this function
// -- is correct but copies 25 uint64 in and 25 out per permutation, a 400-byte round trip per 136
// bytes hashed at SHA3-256's rate; it is recorded here as the option NOT taken.
//
// The GoFrame is gone with the endianness branch: the function has no panic, no throw, and its only
// defer was the BigEndian write-back, which a genuine alias makes unnecessary. byteorder, cpu and
// @unsafe are no longer referenced and are deliberately not carried -- a dead using in a hand-own is
// freight no reconvert will ever clean up.

partial class sha3_package {

// keccakF1600Generic applies the Keccak permutation.
internal static void keccakF1600Generic([GoArrayDims(200)] ж<array<byte>> Ꮡda) {
    Span<uint64> a = MemoryMarshal.Cast<byte, uint64>(Ꮡda.Value.ToSpan());

        // Implementation translated from Keccak-inplace.c
        // in the keccak reference code.
        uint64 t = default!;
        uint64 bc0 = default!;
        uint64 bc1 = default!;
        uint64 bc2 = default!;
        uint64 bc3 = default!;
        uint64 bc4 = default!;
        uint64 d0 = default!;
        uint64 d1 = default!;
        uint64 d2 = default!;
        uint64 d3 = default!;
        uint64 d4 = default!;
        for (nint i = 0; i < 24; i += 4) {
            // Combines the 5 steps in each round into 2 steps.
            // Unrolls 4 rounds per loop and spreads some steps across rounds.
            // Round 1
            bc0 = (uint64)((uint64)((uint64)((uint64)(a[0] ^ a[5]) ^ a[10]) ^ a[15]) ^ a[20]);
            bc1 = (uint64)((uint64)((uint64)((uint64)(a[1] ^ a[6]) ^ a[11]) ^ a[16]) ^ a[21]);
            bc2 = (uint64)((uint64)((uint64)((uint64)(a[2] ^ a[7]) ^ a[12]) ^ a[17]) ^ a[22]);
            bc3 = (uint64)((uint64)((uint64)((uint64)(a[3] ^ a[8]) ^ a[13]) ^ a[18]) ^ a[23]);
            bc4 = (uint64)((uint64)((uint64)((uint64)(a[4] ^ a[9]) ^ a[14]) ^ a[19]) ^ a[24]);
            d0 = (uint64)(bc4 ^ ((uint64)((bc1 << (int)(1)) | (bc1 >> (int)(63)))));
            d1 = (uint64)(bc0 ^ ((uint64)((bc2 << (int)(1)) | (bc2 >> (int)(63)))));
            d2 = (uint64)(bc1 ^ ((uint64)((bc3 << (int)(1)) | (bc3 >> (int)(63)))));
            d3 = (uint64)(bc2 ^ ((uint64)((bc4 << (int)(1)) | (bc4 >> (int)(63)))));
            d4 = (uint64)(bc3 ^ ((uint64)((bc0 << (int)(1)) | (bc0 >> (int)(63)))));
            bc0 = (uint64)(a[0] ^ d0);
            t = (uint64)(a[6] ^ d1);
            bc1 = bits.RotateLeft64(t, 44);
            t = (uint64)(a[12] ^ d2);
            bc2 = bits.RotateLeft64(t, 43);
            t = (uint64)(a[18] ^ d3);
            bc3 = bits.RotateLeft64(t, 21);
            t = (uint64)(a[24] ^ d4);
            bc4 = bits.RotateLeft64(t, 14);
            a[0] = (uint64)((uint64)(bc0 ^ ((uint64)(bc2 & ~bc1))) ^ rc[i]);
            a[6] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[12] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[18] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[24] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[10] ^ d0);
            bc2 = bits.RotateLeft64(t, 3);
            t = (uint64)(a[16] ^ d1);
            bc3 = bits.RotateLeft64(t, 45);
            t = (uint64)(a[22] ^ d2);
            bc4 = bits.RotateLeft64(t, 61);
            t = (uint64)(a[3] ^ d3);
            bc0 = bits.RotateLeft64(t, 28);
            t = (uint64)(a[9] ^ d4);
            bc1 = bits.RotateLeft64(t, 20);
            a[10] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[16] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[22] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[3] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[9] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[20] ^ d0);
            bc4 = bits.RotateLeft64(t, 18);
            t = (uint64)(a[1] ^ d1);
            bc0 = bits.RotateLeft64(t, 1);
            t = (uint64)(a[7] ^ d2);
            bc1 = bits.RotateLeft64(t, 6);
            t = (uint64)(a[13] ^ d3);
            bc2 = bits.RotateLeft64(t, 25);
            t = (uint64)(a[19] ^ d4);
            bc3 = bits.RotateLeft64(t, 8);
            a[20] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[1] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[7] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[13] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[19] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[5] ^ d0);
            bc1 = bits.RotateLeft64(t, 36);
            t = (uint64)(a[11] ^ d1);
            bc2 = bits.RotateLeft64(t, 10);
            t = (uint64)(a[17] ^ d2);
            bc3 = bits.RotateLeft64(t, 15);
            t = (uint64)(a[23] ^ d3);
            bc4 = bits.RotateLeft64(t, 56);
            t = (uint64)(a[4] ^ d4);
            bc0 = bits.RotateLeft64(t, 27);
            a[5] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[11] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[17] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[23] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[4] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[15] ^ d0);
            bc3 = bits.RotateLeft64(t, 41);
            t = (uint64)(a[21] ^ d1);
            bc4 = bits.RotateLeft64(t, 2);
            t = (uint64)(a[2] ^ d2);
            bc0 = bits.RotateLeft64(t, 62);
            t = (uint64)(a[8] ^ d3);
            bc1 = bits.RotateLeft64(t, 55);
            t = (uint64)(a[14] ^ d4);
            bc2 = bits.RotateLeft64(t, 39);
            a[15] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[21] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[2] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[8] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[14] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            // Round 2
            bc0 = (uint64)((uint64)((uint64)((uint64)(a[0] ^ a[5]) ^ a[10]) ^ a[15]) ^ a[20]);
            bc1 = (uint64)((uint64)((uint64)((uint64)(a[1] ^ a[6]) ^ a[11]) ^ a[16]) ^ a[21]);
            bc2 = (uint64)((uint64)((uint64)((uint64)(a[2] ^ a[7]) ^ a[12]) ^ a[17]) ^ a[22]);
            bc3 = (uint64)((uint64)((uint64)((uint64)(a[3] ^ a[8]) ^ a[13]) ^ a[18]) ^ a[23]);
            bc4 = (uint64)((uint64)((uint64)((uint64)(a[4] ^ a[9]) ^ a[14]) ^ a[19]) ^ a[24]);
            d0 = (uint64)(bc4 ^ ((uint64)((bc1 << (int)(1)) | (bc1 >> (int)(63)))));
            d1 = (uint64)(bc0 ^ ((uint64)((bc2 << (int)(1)) | (bc2 >> (int)(63)))));
            d2 = (uint64)(bc1 ^ ((uint64)((bc3 << (int)(1)) | (bc3 >> (int)(63)))));
            d3 = (uint64)(bc2 ^ ((uint64)((bc4 << (int)(1)) | (bc4 >> (int)(63)))));
            d4 = (uint64)(bc3 ^ ((uint64)((bc0 << (int)(1)) | (bc0 >> (int)(63)))));
            bc0 = (uint64)(a[0] ^ d0);
            t = (uint64)(a[16] ^ d1);
            bc1 = bits.RotateLeft64(t, 44);
            t = (uint64)(a[7] ^ d2);
            bc2 = bits.RotateLeft64(t, 43);
            t = (uint64)(a[23] ^ d3);
            bc3 = bits.RotateLeft64(t, 21);
            t = (uint64)(a[14] ^ d4);
            bc4 = bits.RotateLeft64(t, 14);
            a[0] = (uint64)((uint64)(bc0 ^ ((uint64)(bc2 & ~bc1))) ^ rc[i + 1]);
            a[16] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[7] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[23] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[14] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[20] ^ d0);
            bc2 = bits.RotateLeft64(t, 3);
            t = (uint64)(a[11] ^ d1);
            bc3 = bits.RotateLeft64(t, 45);
            t = (uint64)(a[2] ^ d2);
            bc4 = bits.RotateLeft64(t, 61);
            t = (uint64)(a[18] ^ d3);
            bc0 = bits.RotateLeft64(t, 28);
            t = (uint64)(a[9] ^ d4);
            bc1 = bits.RotateLeft64(t, 20);
            a[20] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[11] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[2] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[18] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[9] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[15] ^ d0);
            bc4 = bits.RotateLeft64(t, 18);
            t = (uint64)(a[6] ^ d1);
            bc0 = bits.RotateLeft64(t, 1);
            t = (uint64)(a[22] ^ d2);
            bc1 = bits.RotateLeft64(t, 6);
            t = (uint64)(a[13] ^ d3);
            bc2 = bits.RotateLeft64(t, 25);
            t = (uint64)(a[4] ^ d4);
            bc3 = bits.RotateLeft64(t, 8);
            a[15] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[6] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[22] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[13] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[4] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[10] ^ d0);
            bc1 = bits.RotateLeft64(t, 36);
            t = (uint64)(a[1] ^ d1);
            bc2 = bits.RotateLeft64(t, 10);
            t = (uint64)(a[17] ^ d2);
            bc3 = bits.RotateLeft64(t, 15);
            t = (uint64)(a[8] ^ d3);
            bc4 = bits.RotateLeft64(t, 56);
            t = (uint64)(a[24] ^ d4);
            bc0 = bits.RotateLeft64(t, 27);
            a[10] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[1] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[17] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[8] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[24] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[5] ^ d0);
            bc3 = bits.RotateLeft64(t, 41);
            t = (uint64)(a[21] ^ d1);
            bc4 = bits.RotateLeft64(t, 2);
            t = (uint64)(a[12] ^ d2);
            bc0 = bits.RotateLeft64(t, 62);
            t = (uint64)(a[3] ^ d3);
            bc1 = bits.RotateLeft64(t, 55);
            t = (uint64)(a[19] ^ d4);
            bc2 = bits.RotateLeft64(t, 39);
            a[5] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[21] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[12] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[3] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[19] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            // Round 3
            bc0 = (uint64)((uint64)((uint64)((uint64)(a[0] ^ a[5]) ^ a[10]) ^ a[15]) ^ a[20]);
            bc1 = (uint64)((uint64)((uint64)((uint64)(a[1] ^ a[6]) ^ a[11]) ^ a[16]) ^ a[21]);
            bc2 = (uint64)((uint64)((uint64)((uint64)(a[2] ^ a[7]) ^ a[12]) ^ a[17]) ^ a[22]);
            bc3 = (uint64)((uint64)((uint64)((uint64)(a[3] ^ a[8]) ^ a[13]) ^ a[18]) ^ a[23]);
            bc4 = (uint64)((uint64)((uint64)((uint64)(a[4] ^ a[9]) ^ a[14]) ^ a[19]) ^ a[24]);
            d0 = (uint64)(bc4 ^ ((uint64)((bc1 << (int)(1)) | (bc1 >> (int)(63)))));
            d1 = (uint64)(bc0 ^ ((uint64)((bc2 << (int)(1)) | (bc2 >> (int)(63)))));
            d2 = (uint64)(bc1 ^ ((uint64)((bc3 << (int)(1)) | (bc3 >> (int)(63)))));
            d3 = (uint64)(bc2 ^ ((uint64)((bc4 << (int)(1)) | (bc4 >> (int)(63)))));
            d4 = (uint64)(bc3 ^ ((uint64)((bc0 << (int)(1)) | (bc0 >> (int)(63)))));
            bc0 = (uint64)(a[0] ^ d0);
            t = (uint64)(a[11] ^ d1);
            bc1 = bits.RotateLeft64(t, 44);
            t = (uint64)(a[22] ^ d2);
            bc2 = bits.RotateLeft64(t, 43);
            t = (uint64)(a[8] ^ d3);
            bc3 = bits.RotateLeft64(t, 21);
            t = (uint64)(a[19] ^ d4);
            bc4 = bits.RotateLeft64(t, 14);
            a[0] = (uint64)((uint64)(bc0 ^ ((uint64)(bc2 & ~bc1))) ^ rc[i + 2]);
            a[11] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[22] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[8] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[19] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[15] ^ d0);
            bc2 = bits.RotateLeft64(t, 3);
            t = (uint64)(a[1] ^ d1);
            bc3 = bits.RotateLeft64(t, 45);
            t = (uint64)(a[12] ^ d2);
            bc4 = bits.RotateLeft64(t, 61);
            t = (uint64)(a[23] ^ d3);
            bc0 = bits.RotateLeft64(t, 28);
            t = (uint64)(a[9] ^ d4);
            bc1 = bits.RotateLeft64(t, 20);
            a[15] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[1] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[12] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[23] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[9] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[5] ^ d0);
            bc4 = bits.RotateLeft64(t, 18);
            t = (uint64)(a[16] ^ d1);
            bc0 = bits.RotateLeft64(t, 1);
            t = (uint64)(a[2] ^ d2);
            bc1 = bits.RotateLeft64(t, 6);
            t = (uint64)(a[13] ^ d3);
            bc2 = bits.RotateLeft64(t, 25);
            t = (uint64)(a[24] ^ d4);
            bc3 = bits.RotateLeft64(t, 8);
            a[5] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[16] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[2] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[13] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[24] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[20] ^ d0);
            bc1 = bits.RotateLeft64(t, 36);
            t = (uint64)(a[6] ^ d1);
            bc2 = bits.RotateLeft64(t, 10);
            t = (uint64)(a[17] ^ d2);
            bc3 = bits.RotateLeft64(t, 15);
            t = (uint64)(a[3] ^ d3);
            bc4 = bits.RotateLeft64(t, 56);
            t = (uint64)(a[14] ^ d4);
            bc0 = bits.RotateLeft64(t, 27);
            a[20] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[6] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[17] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[3] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[14] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[10] ^ d0);
            bc3 = bits.RotateLeft64(t, 41);
            t = (uint64)(a[21] ^ d1);
            bc4 = bits.RotateLeft64(t, 2);
            t = (uint64)(a[7] ^ d2);
            bc0 = bits.RotateLeft64(t, 62);
            t = (uint64)(a[18] ^ d3);
            bc1 = bits.RotateLeft64(t, 55);
            t = (uint64)(a[4] ^ d4);
            bc2 = bits.RotateLeft64(t, 39);
            a[10] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[21] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[7] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[18] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[4] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            // Round 4
            bc0 = (uint64)((uint64)((uint64)((uint64)(a[0] ^ a[5]) ^ a[10]) ^ a[15]) ^ a[20]);
            bc1 = (uint64)((uint64)((uint64)((uint64)(a[1] ^ a[6]) ^ a[11]) ^ a[16]) ^ a[21]);
            bc2 = (uint64)((uint64)((uint64)((uint64)(a[2] ^ a[7]) ^ a[12]) ^ a[17]) ^ a[22]);
            bc3 = (uint64)((uint64)((uint64)((uint64)(a[3] ^ a[8]) ^ a[13]) ^ a[18]) ^ a[23]);
            bc4 = (uint64)((uint64)((uint64)((uint64)(a[4] ^ a[9]) ^ a[14]) ^ a[19]) ^ a[24]);
            d0 = (uint64)(bc4 ^ ((uint64)((bc1 << (int)(1)) | (bc1 >> (int)(63)))));
            d1 = (uint64)(bc0 ^ ((uint64)((bc2 << (int)(1)) | (bc2 >> (int)(63)))));
            d2 = (uint64)(bc1 ^ ((uint64)((bc3 << (int)(1)) | (bc3 >> (int)(63)))));
            d3 = (uint64)(bc2 ^ ((uint64)((bc4 << (int)(1)) | (bc4 >> (int)(63)))));
            d4 = (uint64)(bc3 ^ ((uint64)((bc0 << (int)(1)) | (bc0 >> (int)(63)))));
            bc0 = (uint64)(a[0] ^ d0);
            t = (uint64)(a[1] ^ d1);
            bc1 = bits.RotateLeft64(t, 44);
            t = (uint64)(a[2] ^ d2);
            bc2 = bits.RotateLeft64(t, 43);
            t = (uint64)(a[3] ^ d3);
            bc3 = bits.RotateLeft64(t, 21);
            t = (uint64)(a[4] ^ d4);
            bc4 = bits.RotateLeft64(t, 14);
            a[0] = (uint64)((uint64)(bc0 ^ ((uint64)(bc2 & ~bc1))) ^ rc[i + 3]);
            a[1] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[2] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[3] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[4] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[5] ^ d0);
            bc2 = bits.RotateLeft64(t, 3);
            t = (uint64)(a[6] ^ d1);
            bc3 = bits.RotateLeft64(t, 45);
            t = (uint64)(a[7] ^ d2);
            bc4 = bits.RotateLeft64(t, 61);
            t = (uint64)(a[8] ^ d3);
            bc0 = bits.RotateLeft64(t, 28);
            t = (uint64)(a[9] ^ d4);
            bc1 = bits.RotateLeft64(t, 20);
            a[5] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[6] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[7] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[8] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[9] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[10] ^ d0);
            bc4 = bits.RotateLeft64(t, 18);
            t = (uint64)(a[11] ^ d1);
            bc0 = bits.RotateLeft64(t, 1);
            t = (uint64)(a[12] ^ d2);
            bc1 = bits.RotateLeft64(t, 6);
            t = (uint64)(a[13] ^ d3);
            bc2 = bits.RotateLeft64(t, 25);
            t = (uint64)(a[14] ^ d4);
            bc3 = bits.RotateLeft64(t, 8);
            a[10] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[11] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[12] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[13] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[14] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[15] ^ d0);
            bc1 = bits.RotateLeft64(t, 36);
            t = (uint64)(a[16] ^ d1);
            bc2 = bits.RotateLeft64(t, 10);
            t = (uint64)(a[17] ^ d2);
            bc3 = bits.RotateLeft64(t, 15);
            t = (uint64)(a[18] ^ d3);
            bc4 = bits.RotateLeft64(t, 56);
            t = (uint64)(a[19] ^ d4);
            bc0 = bits.RotateLeft64(t, 27);
            a[15] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[16] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[17] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[18] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[19] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
            t = (uint64)(a[20] ^ d0);
            bc3 = bits.RotateLeft64(t, 41);
            t = (uint64)(a[21] ^ d1);
            bc4 = bits.RotateLeft64(t, 2);
            t = (uint64)(a[22] ^ d2);
            bc0 = bits.RotateLeft64(t, 62);
            t = (uint64)(a[23] ^ d3);
            bc1 = bits.RotateLeft64(t, 55);
            t = (uint64)(a[24] ^ d4);
            bc2 = bits.RotateLeft64(t, 39);
            a[20] = (uint64)(bc0 ^ ((uint64)(bc2 & ~bc1)));
            a[21] = (uint64)(bc1 ^ ((uint64)(bc3 & ~bc2)));
            a[22] = (uint64)(bc2 ^ ((uint64)(bc4 & ~bc3)));
            a[23] = (uint64)(bc3 ^ ((uint64)(bc0 & ~bc4)));
            a[24] = (uint64)(bc4 ^ ((uint64)(bc1 & ~bc0)));
        }
}

} // end sha3_package

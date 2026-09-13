// chacha8_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using go;
using go.golib;

// The marker every manualConversionFuncs companion carries: `setup` and `block_generic` are
// suppressed at emission and provided here (see the second header block below). `block` alone
// needed no marker — a bodyless partial has nothing to protect — so this arrives with them.
[module: GoManualConversion]

namespace go.@internal;

// Hand-written body for internal/chacha8rand's `block`, which Go implements in ASSEMBLY on amd64
// and arm64 (chacha8_amd64.s / chacha8_arm64.s), so the converter emits it as a bodyless partial
// and the PartialStubGenerator would otherwise fill it with a throwing stub — taking every
// ChaCha8 consumer (math/rand/v2's ChaCha8 source, and the runtime's per-m generator) down at
// first use.
//
// This is GOARCH-gated in Go, not build-tag gated, so no `-tags` selection can substitute a
// portable file: on amd64 the ONLY Go body is the .s one. Forwarding to the converted
// `block_generic` is also not available — it opens the `*[32]uint64` output buffer as
// `(*[16][4]uint32)(unsafe.Pointer(buf))`, an array-SHAPE reinterpretation that a managed
// nested-array view cannot reconstruct.
//
// So the permutation is written directly here, following chacha8_generic.go exactly: the 4x4
// ChaCha8 matrix is held 4-ways interlaced (16 rows x 4 lanes of uint32, matching Go's
// [16][4]uint32 view of the buffer), setup seeds it, four iterations of eight quarter-rounds each
// give the 8 rounds, rows 4..11 are added back to the original key material, and the uint32 words
// are packed back into the uint64 buffer little-endian. Verified bit-exact against Go by
// internal/chacha8rand's own vectors through math/rand/v2's TestChaCha8* suite.
//
// ── `setup` and `block_generic` join it (2026-08-25) ─────────────────────────────────────────────
// The same reinterpret the paragraph above declines is what the package's OWN `block_generic` and
// `setup` are built on, so the auto conversion of both took the raw-address route and panicked
// `index out of range [0] with length 0` on the zeroed buffer — `TestBlockGeneric`, and the whole
// of this package's 1-of-4 validation gap. Their emission is suppressed
// (`manualConversionFuncs["internal/chacha8rand"]`, whose entry carries the rooting) and the bodies
// live here.
//
// They are NOT a second copy of `block`, and the difference is the point. `block` computes into a
// stackalloc scratch and PACKS the result into the caller's buffer at the end; these two follow
// chacha8_generic.go's own shape and work IN PLACE through a genuine aliasing view of that buffer —
// `MemoryMarshal.Cast` over `array<uint64>.ToSpan()`, the remedy vendor/…/sha3's `xor.cs` and
// crypto/subtle's `xor_generic.cs` already take for this class. `setup` writes the uint64 pairs Go's
// own `(*[16][2]uint64)` view writes; `block_generic` reads and stores through the `[16][4]uint32`
// view. So `TestBlockGeneric` still compares two independently written implementations — what it
// newly proves is that the aliasing view WRITES THROUGH, which is exactly what was broken.
//
// The general repair of the seam is `docs/phase4/DESIGN-native-array-view.md` (RATIFIED; its §3
// emission work is HELD pending the provenance amendment). This package would not be fixed by it
// even so: a native-backed `array<T>` cannot carry `array<uint32>` ELEMENTS in raw bytes, which is
// what a `[16][4]uint32` view needs. The seam's kernel-free golib witness is preserved directly, in
// GolibTests' ArrayShapeReinterpretTests, so routing this package around it costs the arc nothing.
partial class chacha8rand_package
{
    // ChaCha20's "expand 32-byte k" constants, one per matrix row 0..3.
    private static readonly uint[] s_chacha8Constants = [0x61707865u, 0x3320646eu, 0x79622d32u, 0x6b206574u];

    internal static partial void block(ж<array<uint64>> seed, ж<array<uint64>> blocks, uint32 counter)
    {
        // b[row * 4 + lane] — the [16][4]uint32 view of the 32-uint64 output buffer.
        Span<uint> b = stackalloc uint[64];

        Setup(seed, b, counter);

        for (int i = 0; i < 4; i++)
        {
            uint b0 = b[0 * 4 + i], b1 = b[1 * 4 + i], b2 = b[2 * 4 + i], b3 = b[3 * 4 + i];
            uint b4 = b[4 * 4 + i], b5 = b[5 * 4 + i], b6 = b[6 * 4 + i], b7 = b[7 * 4 + i];
            uint b8 = b[8 * 4 + i], b9 = b[9 * 4 + i], b10 = b[10 * 4 + i], b11 = b[11 * 4 + i];
            uint b12 = b[12 * 4 + i], b13 = b[13 * 4 + i], b14 = b[14 * 4 + i], b15 = b[15 * 4 + i];

            // 4 iterations of eight quarter-rounds each is 8 rounds.
            for (int round = 0; round < 4; round++)
            {
                QuarterRound(ref b0, ref b4, ref b8, ref b12);
                QuarterRound(ref b1, ref b5, ref b9, ref b13);
                QuarterRound(ref b2, ref b6, ref b10, ref b14);
                QuarterRound(ref b3, ref b7, ref b11, ref b15);

                QuarterRound(ref b0, ref b5, ref b10, ref b15);
                QuarterRound(ref b1, ref b6, ref b11, ref b12);
                QuarterRound(ref b2, ref b7, ref b8, ref b13);
                QuarterRound(ref b3, ref b4, ref b9, ref b14);
            }

            // Add b4..b11 back to the original key material, like in ChaCha20, to avoid trivial
            // invertibility. There is no entropy in b0..b3 and b12..b15, so those are plain stores.
            b[0 * 4 + i] = b0;
            b[1 * 4 + i] = b1;
            b[2 * 4 + i] = b2;
            b[3 * 4 + i] = b3;
            b[4 * 4 + i] += b4;
            b[5 * 4 + i] += b5;
            b[6 * 4 + i] += b6;
            b[7 * 4 + i] += b7;
            b[8 * 4 + i] += b8;
            b[9 * 4 + i] += b9;
            b[10 * 4 + i] += b10;
            b[11 * 4 + i] += b11;
            b[12 * 4 + i] = b12;
            b[13 * 4 + i] = b13;
            b[14 * 4 + i] = b14;
            b[15 * 4 + i] = b15;
        }

        // Pack the uint32 words back into the uint64 buffer. Go reads the buffer through the same
        // little-endian aliasing the [16][4]uint32 view gave it (its big-endian branch word-swaps
        // to reach this same value), so the low word is the even index.
        ref array<uint64> target = ref blocks.Value;

        for (int j = 0; j < 32; j++)
            target[j] = (uint64)b[2 * j] | ((uint64)b[2 * j + 1] << 32);
    }

    // Fills the interlaced matrix: constant rows, the 4 seed uint64s split into 8 uint32 rows,
    // the per-lane counter row, and three zero rows. Every lane of a row carries the same value
    // except the counter row, whose lane i is counter+i — that is what makes the four
    // simultaneously-computed blocks distinct.
    private static void Setup(ж<array<uint64>> seed, Span<uint> b, uint32 counter)
    {
        ref array<uint64> s = ref seed.Value;

        for (int row = 0; row < 4; row++)
        {
            uint constant = s_chacha8Constants[row];

            for (int lane = 0; lane < 4; lane++)
                b[row * 4 + lane] = constant;
        }

        for (int word = 0; word < 8; word++)
        {
            // Seed word 2k is the low half of seed[k], word 2k+1 the high half.
            uint value = (uint)(s[word / 2] >> (32 * (word % 2)));

            for (int lane = 0; lane < 4; lane++)
                b[(4 + word) * 4 + lane] = value;
        }

        for (int lane = 0; lane < 4; lane++)
            b[12 * 4 + lane] = counter + (uint)lane;

        for (int row = 13; row < 16; row++)
        {
            for (int lane = 0; lane < 4; lane++)
                b[row * 4 + lane] = 0;
        }
    }

    // The ChaCha8 quarter round (chacha8_generic.go qr).
    private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        a += b;
        d ^= a;
        d = d << 16 | d >> 16;
        c += d;
        b ^= c;
        b = b << 12 | b >> 20;
        a += b;
        d ^= a;
        d = d << 8 | d >> 24;
        c += d;
        b ^= c;
        b = b << 7 | b >> 25;
    }

    // chacha8_generic.go's setup, transcribed. Go's own body immediately re-views its `*[16][4]uint32`
    // parameter as `(*[16][2]uint64)` to halve the stores, so THAT view is the parameter here: `b` is
    // the caller's `[32]uint64` buffer read as 16 rows of 2 uint64s (row r, half h => b[r * 2 + h]),
    // which is the same storage under a shape the managed model can hold. The `*[16][4]uint32`
    // parameter Go declares is the un-representable one and has no counterpart; the caller is
    // block_generic, the only caller Go has.
    private static void setup(ж<array<uint64>> Ꮡseed, Span<uint64> b, uint32 counter)
    {
        ref array<uint64> seed = ref Ꮡseed.Value;

        // Constants; same as in ChaCha20: "expand 32-byte k".
        b[0 * 2 + 0] = 0x61707865_61707865UL;
        b[0 * 2 + 1] = 0x61707865_61707865UL;

        b[1 * 2 + 0] = 0x3320646e_3320646eUL;
        b[1 * 2 + 1] = 0x3320646e_3320646eUL;

        b[2 * 2 + 0] = 0x79622d32_79622d32UL;
        b[2 * 2 + 1] = 0x79622d32_79622d32UL;

        b[3 * 2 + 0] = 0x6b206574_6b206574UL;
        b[3 * 2 + 1] = 0x6b206574_6b206574UL;

        // Seed values: rows 4..11, each carrying one 32-bit half of one seed word, duplicated into
        // all four lanes (Go writes the pair `uint64(x)<<32 | uint64(x)` into both halves of a row).
        for (int word = 0; word < 8; word++)
        {
            uint x = (uint)(seed[word / 2] >> (32 * (word % 2)));
            uint64 x64 = ((uint64)x << 32) | x;

            b[(4 + word) * 2 + 0] = x64;
            b[(4 + word) * 2 + 1] = x64;
        }

        // Counters — lane i takes counter+i. Which END of the uint64 pair each lane occupies is the
        // byte order of the uint32 view this row is really written for, so it flips with endianness
        // exactly as Go's goarch.BigEndian branch does.
        if (!BitConverter.IsLittleEndian)
        {
            b[12 * 2 + 0] = ((uint64)(counter + 0) << 32) | (counter + 1);
            b[12 * 2 + 1] = ((uint64)(counter + 2) << 32) | (counter + 3);
        }
        else
        {
            b[12 * 2 + 0] = (counter + 0) | ((uint64)(counter + 1) << 32);
            b[12 * 2 + 1] = (counter + 2) | ((uint64)(counter + 3) << 32);
        }

        // Zeros.
        b[13 * 2 + 0] = 0;
        b[13 * 2 + 1] = 0;
        b[14 * 2 + 0] = 0;
        b[14 * 2 + 1] = 0;
        b[15 * 2 + 0] = 0;
        b[15 * 2 + 1] = 0;
    }

    // chacha8_generic.go's block_generic, transcribed. Go views the caller's `*[32]uint64` output
    // buffer as `(*[16][4]uint32)` and computes IN PLACE through it; `MemoryMarshal.Cast` over the
    // array's own span is that view — a genuine alias of the same backing storage, in the same
    // word order (a uint32 cast of a uint64 span puts word 2k at the same byte offset Go's does, on
    // either endianness), so every store below lands in the caller's buffer.
    internal static void block_generic([GoArrayDims(4)] ж<array<uint64>> Ꮡseed, [GoArrayDims(32)] ж<array<uint64>> Ꮡbuf, uint32 counter)
    {
        Span<uint64> buf = Ꮡbuf.Value.ToSpan();
        Span<uint> b = MemoryMarshal.Cast<uint64, uint>(buf);

        setup(Ꮡseed, buf, counter);

        for (int i = 0; i < 4; i++)
        {
            // Load block i from b[*][i] into local variables.
            uint b0 = b[0 * 4 + i], b1 = b[1 * 4 + i], b2 = b[2 * 4 + i], b3 = b[3 * 4 + i];
            uint b4 = b[4 * 4 + i], b5 = b[5 * 4 + i], b6 = b[6 * 4 + i], b7 = b[7 * 4 + i];
            uint b8 = b[8 * 4 + i], b9 = b[9 * 4 + i], b10 = b[10 * 4 + i], b11 = b[11 * 4 + i];
            uint b12 = b[12 * 4 + i], b13 = b[13 * 4 + i], b14 = b[14 * 4 + i], b15 = b[15 * 4 + i];

            // 4 iterations of eight quarter-rounds each is 8 rounds. `qr` is the package's OWN
            // auto-converted quarter round (chacha8_generic.go's, which carries no reinterpret and
            // so converts faithfully) — deliberately not `block`'s hand-written QuarterRound, so
            // the two implementations TestBlockGeneric compares share no code at all.
            for (int round = 0; round < 4; round++)
            {
                (b0, b4, b8, b12) = qr(b0, b4, b8, b12);
                (b1, b5, b9, b13) = qr(b1, b5, b9, b13);
                (b2, b6, b10, b14) = qr(b2, b6, b10, b14);
                (b3, b7, b11, b15) = qr(b3, b7, b11, b15);

                (b0, b5, b10, b15) = qr(b0, b5, b10, b15);
                (b1, b6, b11, b12) = qr(b1, b6, b11, b12);
                (b2, b7, b8, b13) = qr(b2, b7, b8, b13);
                (b3, b4, b9, b14) = qr(b3, b4, b9, b14);
            }

            // Store block i back into b[*][i]. Rows 4..11 are ADDED back to the original key
            // material, like in ChaCha20, to avoid trivial invertibility; there is no entropy in
            // rows 0..3 and 12..15, so those are plain stores.
            b[0 * 4 + i] = b0;
            b[1 * 4 + i] = b1;
            b[2 * 4 + i] = b2;
            b[3 * 4 + i] = b3;
            b[4 * 4 + i] += b4;
            b[5 * 4 + i] += b5;
            b[6 * 4 + i] += b6;
            b[7 * 4 + i] += b7;
            b[8 * 4 + i] += b8;
            b[9 * 4 + i] += b9;
            b[10 * 4 + i] += b10;
            b[11 * 4 + i] += b11;
            b[12 * 4 + i] = b12;
            b[13 * 4 + i] = b13;
            b[14 * 4 + i] = b14;
            b[15 * 4 + i] = b15;
        }

        if (!BitConverter.IsLittleEndian)
        {
            // On a big-endian system, reading the uint32 pairs as uint64s word-swaps them compared
            // to little-endian, so word-swap here first to make the next swap get the right answer.
            for (int i = 0; i < buf.Length; i++)
                buf[i] = buf[i] >> 32 | buf[i] << 32;
        }
    }
}

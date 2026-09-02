// bits_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// WHY THIS FILE EXISTS. math/bits is the one package in the standard library whose portable Go
// bodies are NEVER the code Go actually runs. Its own doc comment says so:
//
//     "Functions in this package may be implemented directly by the compiler, for better
//      performance. For those functions the code in this package will not be used."
//
// On amd64 the compiler intrinsifies the whole family — Mul64 becomes one MULQ, OnesCount64 one
// POPCNT, LeadingZeros64 one LZCNT, RotateLeft64 one ROL, ReverseBytes64 one BSWAP. go2cs emits the
// portable bodies faithfully, and faithfully is exactly the problem: the emission is correct and it
// is the fallback Go itself never executes. The result is a whole family of hot primitives running
// ten-instruction emulations of single instructions.
//
// WHAT IT COST, measured (2026-09-02, Release + DOTNET_TieredCompilation=0, one host, sequential):
//
//     RSA-2048 PSS signature      Go 0.834 ms   converted 44.5 ms    53x
//     TLS 1.3 handshake, steady   Go 2.59 ms    converted 57.87 ms   22x
//
// and the signature is 79% of the handshake residual ((44.50-0.83) / (57.87-2.59)), because a TLS
// 1.3 handshake performs exactly ONE server CertificateVerify signature. The chain runs
// crypto/rsa -> math/big expNN -> montgomery -> addMulVVW -> mulAddWWW_g -> bits.Mul -> Mul64, and
// math/big compounds it: Go's arith_decl.go declares addMulVVW with NO BODY (hand-written assembly)
// and cmd/compile aliases math/big's own mulWW straight to the Mul64 intrinsic, so the converted
// corpus loses the assembly loop AND the intrinsic under it. This file recovers the second.
//
// WHY IT IS WORTH A HAND-OWN RATHER THAN ACCEPTED. The callers are not incidental. Alias-resolved
// census of the converted corpus (production only): Add64 2,105 call sites, Mul64 1,066, Sub64 193,
// RotateLeft64/32 208. By package, crypto/internal/nistec/fiat holds 3,061 of them and
// crypto/internal/edwards25519 another 240 — the P-256 field arithmetic and the X25519 ECDHE path.
// math/big does not appear in that table only because it calls the word-sized bits.Mul/bits.Add,
// which dispatch here; its handful of sites run thousands of times per signature. Call sites are not
// executions, and both shapes lead to the same primitives.
//
// WHAT IS DELIBERATELY NOT HERE.
//
//   Div64  — .NET has no 128/64 divide primitive, so UInt128 division is software either way, and
//            Go's Div64 carries panic-on-zero and panic-on-overflow semantics worth preserving
//            exactly. The corpus has TWO bits.Div call sites. There is no measurement justifying
//            the risk, and "measured, not assumed" cuts both ways: it stays converted.
//
//   Reverse8/16/32/64 (BIT reversal, not byte reversal) — .NET exposes no primitive for it, so a
//            hand-own could only restate the same shift/mask chain the converter already emits.
//            Nothing to gain; left alone.
//
//   The 8- and 16-bit LeadingZeros/TrailingZeros/OnesCount variants — table lookups already, with
//            negligible call counts. Replacing them would trade correctness risk for nothing.
//
// SEMANTICS. Each replacement below matches Go at the edges, which is where these functions differ
// from their obvious implementations: LeadingZeros64(0) == 64 and TrailingZeros64(0) == 64 (both
// BitOperations counterparts agree, returning the operand width for zero); Len64(0) == 0, which
// 64 - LeadingZeroCount(0) gives; RotateLeft64(x, k) is defined for NEGATIVE k, and both Go and
// BitOperations reduce the shift modulo the width, so the low six bits decide and the two agree.
// math/bits's own banked test row is the gate on all of this and any move there stops the change.
//
// Paired with the manualConversionFuncs["math/bits"] registration in the converter, which replaces
// each body below with a placeholder in the emitted bits.cs. Both halves must land together; the
// converter's manualConversionDestination_test enforces that in both directions.

using System;
using System.Buffers.Binary;
using System.Numerics;

[module: go.GoManualConversion]

namespace go.math;

partial class bits_package
{
    // --- OnesCount: POPCNT ---

    // OnesCount32 returns the number of one bits ("population count") in x.
    public static nint OnesCount32(uint32 x) => BitOperations.PopCount(x);

    // OnesCount64 returns the number of one bits ("population count") in x.
    public static nint OnesCount64(uint64 x) => BitOperations.PopCount(x);

    // --- LeadingZeros / Len: LZCNT ---

    // LeadingZeros32 returns the number of leading zero bits in x; the result is 32 for x == 0.
    public static nint LeadingZeros32(uint32 x) => BitOperations.LeadingZeroCount(x);

    // LeadingZeros64 returns the number of leading zero bits in x; the result is 64 for x == 0.
    public static nint LeadingZeros64(uint64 x) => BitOperations.LeadingZeroCount(x);

    // Len32 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
    public static nint Len32(uint32 x) => 32 - BitOperations.LeadingZeroCount(x);

    // Len64 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
    public static nint Len64(uint64 x) => 64 - BitOperations.LeadingZeroCount(x);

    // --- TrailingZeros: TZCNT ---

    // TrailingZeros32 returns the number of trailing zero bits in x; the result is 32 for x == 0.
    public static nint TrailingZeros32(uint32 x) => BitOperations.TrailingZeroCount(x);

    // TrailingZeros64 returns the number of trailing zero bits in x; the result is 64 for x == 0.
    public static nint TrailingZeros64(uint64 x) => BitOperations.TrailingZeroCount(x);

    // --- RotateLeft: ROL ---
    //
    // Go reduces the shift modulo the width and is defined for negative k ("to rotate x right by k
    // bits, call RotateLeft64(x, -k)"); BitOperations.RotateLeft reduces the same way, so only the
    // low bits of the count matter and the narrowing cast cannot change the answer.

    // RotateLeft32 returns the value of x rotated left by (k mod 32) bits.
    public static uint32 RotateLeft32(uint32 x, nint k) => BitOperations.RotateLeft(x, (int)k);

    // RotateLeft64 returns the value of x rotated left by (k mod 64) bits.
    public static uint64 RotateLeft64(uint64 x, nint k) => BitOperations.RotateLeft(x, (int)k);

    // --- ReverseBytes: BSWAP ---

    // ReverseBytes16 returns the value of x with its bytes in reversed order.
    public static uint16 ReverseBytes16(uint16 x) => BinaryPrimitives.ReverseEndianness(x);

    // ReverseBytes32 returns the value of x with its bytes in reversed order.
    public static uint32 ReverseBytes32(uint32 x) => BinaryPrimitives.ReverseEndianness(x);

    // ReverseBytes64 returns the value of x with its bytes in reversed order.
    public static uint64 ReverseBytes64(uint64 x) => BinaryPrimitives.ReverseEndianness(x);

    // --- Add / Sub with carry: ADC / SBB ---
    //
    // Go reconstructs the carry with bit algebra because it has no carry type; UInt128 states the
    // same arithmetic directly and lets the JIT lower it. Go documents the carry/borrow input as 0
    // or 1 with anything else undefined, so the two agree wherever the contract holds.

    // Add64 returns the sum with carry of x, y and carry: sum = x + y + carry.
    // The carry input must be 0 or 1; otherwise the behavior is undefined.
    public static (uint64 sum, uint64 carryOut) Add64(uint64 x, uint64 y, uint64 carry)
    {
        UInt128 sum = (UInt128)x + y + carry;
        return ((uint64)sum, (uint64)(sum >> 64));
    }

    // Sub64 returns the difference of x, y and borrow: diff = x - y - borrow.
    // The borrow input must be 0 or 1; otherwise the behavior is undefined.
    public static (uint64 diff, uint64 borrowOut) Sub64(uint64 x, uint64 y, uint64 borrow)
    {
        UInt128 diff = (UInt128)x - y - borrow;

        // On borrow the 128-bit subtraction wraps and every high bit is set, so the low bit of the
        // high half is the borrow; without one the high half is zero.
        return ((uint64)diff, (uint64)(diff >> 64) & 1);
    }

    // --- Mul: MULQ ---

    // Mul64 returns the 128-bit product of x and y: (hi, lo) = x * y
    // with the product bits' upper half returned in hi and the lower half in lo.
    public static (uint64 hi, uint64 lo) Mul64(uint64 x, uint64 y)
    {
        uint64 hi = Math.BigMul(x, y, out uint64 lo);
        return (hi, lo);
    }
}

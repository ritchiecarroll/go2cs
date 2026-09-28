// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bits = go.math.bits_package;
using go.math;

partial class netip_package {

// uint128 represents a uint128 using two uint64s.
//
// When the methods below mention a bit number, bit 0 is the most
// significant bit (in hi) and bit 127 is the lowest (lo&1).
[GoType] partial struct uint128 {
    internal uint64 hi;
    internal uint64 lo;
}

// mask6 returns a uint128 bitmask with the topmost n bits of a
// 128-bit number.
internal static uint128 mask6(nint n) {
    return new uint128(~((~(uint64)0).Rsh((int64)(n))), (~(uint64)0).Lsh((int64)((128 - n))));
}

// isZero reports whether u == 0.
//
// It's faster than u == (uint128{}) because the compiler (as of Go
// 1.15/1.16b1) doesn't do this trick and instead inserts a branch in
// its eq alg's generated code.
internal static bool isZero(this uint128 u) {
    return (uint64)(u.hi | u.lo) == 0;
}

// and returns the bitwise AND of u and m (u&m).
internal static uint128 and(this uint128 u, uint128 m) {
    return new uint128((uint64)(u.hi & m.hi), (uint64)(u.lo & m.lo));
}

// xor returns the bitwise XOR of u and m (u^m).
internal static uint128 xor(this uint128 u, uint128 m) {
    return new uint128((uint64)(u.hi ^ m.hi), (uint64)(u.lo ^ m.lo));
}

// or returns the bitwise OR of u and m (u|m).
internal static uint128 or(this uint128 u, uint128 m) {
    return new uint128((uint64)(u.hi | m.hi), (uint64)(u.lo | m.lo));
}

// not returns the bitwise NOT of u.
internal static uint128 not(this uint128 u) {
    return new uint128(~u.hi, ~u.lo);
}

// subOne returns u - 1.
internal static uint128 subOne(this uint128 u) {
    var (lo, borrow) = bits.Sub64(u.lo, 1, 0);
    return new uint128(u.hi - borrow, lo);
}

// addOne returns u + 1.
internal static uint128 addOne(this uint128 u) {
    var (lo, carry) = bits.Add64(u.lo, 1, 0);
    return new uint128(u.hi + carry, lo);
}

// halves returns the two uint64 halves of the uint128.
//
// Logically, think of it as returning two uint64s.
// It only returns pointers for inlining reasons on 32-bit platforms.
internal static array<ж<uint64>> halves(this ж<uint128> Ꮡu) {
    return new ж<uint64>[]{Ꮡu.of(uint128.Ꮡhi), Ꮡu.of(uint128.Ꮡlo)}.array();
}

// bitsSetFrom returns a copy of u with the given bit
// and all subsequent ones set.
internal static uint128 bitsSetFrom(this uint128 u, uint8 bit) {
    return u.or(mask6((nint)bit).not());
}

// bitsClearedFrom returns a copy of u with the given bit
// and all subsequent ones cleared.
internal static uint128 bitsClearedFrom(this uint128 u, uint8 bit) {
    return u.and(mask6((nint)bit));
}

} // end netip_package

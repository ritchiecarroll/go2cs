// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bits = go.math.bits_package;
using go.math;
using static netip_package;

partial class netip_package {

// uint128 represents a uint128 using two uint64s.
//
// When the methods below mention a bit number, bit 0 is the most
// significant bit (in hi) and bit 127 is the lowest (lo&1).
partial struct uint128 {
    internal uint64 hi;
    internal uint64 lo;
}

// mask6 returns a uint128 bitmask with the topmost n bits of a
// 128-bit number.
internal static uint128 mask6(nint n) {
    return new uint128(~((~(uint64)0).Rsh((int64)(n))), (~(uint64)0).Lsh((int64)(128 - n)));
}

} partial class netip_package {

// isZero reports whether u == 0.
//
// It's faster than u == (uint128{}) because the compiler (as of Go
// 1.15/1.16b1) doesn't do this trick and instead inserts a branch in
// its eq alg's generated code.
[GoMethodBody("isZero")] internal static bool isZeroᴍ(uint128 u) { return (u.hi | u.lo) == 0; }

// and returns the bitwise AND of u and m (u&m).
[GoMethodBody("and")] internal static uint128 andᴍ(uint128 u, uint128 m) {
    return new uint128(u.hi & m.hi, u.lo & m.lo);
}

// xor returns the bitwise XOR of u and m (u^m).
[GoMethodBody("xor")] internal static uint128 xorᴍ(uint128 u, uint128 m) {
    return new uint128(u.hi ^ m.hi, u.lo ^ m.lo);
}

// or returns the bitwise OR of u and m (u|m).
[GoMethodBody("or")] internal static uint128 orᴍ(uint128 u, uint128 m) {
    return new uint128(u.hi | m.hi, u.lo | m.lo);
}

// not returns the bitwise NOT of u.
[GoMethodBody("not")] internal static uint128 notᴍ(uint128 u) {
    return new uint128(~u.hi, ~u.lo);
}

// subOne returns u - 1.
[GoMethodBody("subOne")] internal static uint128 subOneᴍ(uint128 u) {
    var (lo, borrow) = bits.Sub64(u.lo, 1, 0);
    return new uint128(u.hi - borrow, lo);
}

// addOne returns u + 1.
[GoMethodBody("addOne")] internal static uint128 addOneᴍ(uint128 u) {
    var (lo, carry) = bits.Add64(u.lo, 1, 0);
    return new uint128(u.hi + carry, lo);
}

// halves returns the two uint64 halves of the uint128.
//
// Logically, think of it as returning two uint64s.
// It only returns pointers for inlining reasons on 32-bit platforms.
[GoMethodBody("halves")] internal static array<ж<uint64>> halvesᴍ(ж<uint128> Ꮡu) {
    return array<ж<uint64>>([Ꮡu.Ꮡhi, Ꮡu.Ꮡlo]);
}

// bitsSetFrom returns a copy of u with the given bit
// and all subsequent ones set.
[GoMethodBody("bitsSetFrom")] internal static uint128 bitsSetFromᴍ(uint128 u, uint8 bit) {
    return u.or(mask6((nint)bit).not());
}

// bitsClearedFrom returns a copy of u with the given bit
// and all subsequent ones cleared.
[GoMethodBody("bitsClearedFrom")] internal static uint128 bitsClearedFromᴍ(uint128 u, uint8 bit) {
    return u.and(mask6((nint)bit));
}

}

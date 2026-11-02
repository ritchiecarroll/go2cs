// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using constanttime = go.crypto.@internal.constanttime_package;
using go.crypto.@internal;

partial class edwards25519_package {

// A dynamic lookup table for variable-base, constant-time scalar muls.
partial struct projLookupTable {
    internal array<projCached> points = new(8);
}

// A precomputed lookup table for fixed-base, constant-time scalar muls.
partial struct affineLookupTable {
    internal array<affineCached> points = new(8);
}

// A dynamic lookup table for variable-base, variable-time scalar muls.
partial struct nafLookupTable5 {
    internal array<projCached> points = new(8);
}

// A precomputed lookup table for fixed-base, variable-time scalar muls.
partial struct nafLookupTable8 {
    internal array<affineCached> points = new(64);
}

// Constructors.

// Builds a lookup table at runtime. Fast.
internal static void FromP3(this ref projLookupTable v, ж<Point> q) {
    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    Ꮡ(v.points, 0).FromP3(q);
    ref var tmpP3 = ref heap<Point>(new Point(), out var ᏑtmpP3);
    ref var tmpP1xP1 = ref heap<projP1xP1>(new projP1xP1(), out var ᏑtmpP1xP1);
    for (nint i = 0; i < 7; i++) {
        // Compute (i+1)*Q as Q + i*Q and convert to a projCached
        // This is needlessly complicated because the API has explicit
        // receivers instead of creating stack objects and relying on RVO
        Ꮡ(v.points, i + 1).FromP3(ᏑtmpP3.fromP1xP1(ᏑtmpP1xP1.Add(q, Ꮡ(v.points, i))));
    }
}

// This is not optimised for speed; fixed-base tables should be precomputed.
internal static void FromP3(this ref affineLookupTable v, ж<Point> q) {
    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    Ꮡ(v.points, 0).FromP3(q);
    ref var tmpP3 = ref heap<Point>(new Point(), out var ᏑtmpP3);
    ref var tmpP1xP1 = ref heap<projP1xP1>(new projP1xP1(), out var ᏑtmpP1xP1);
    for (nint i = 0; i < 7; i++) {
        // Compute (i+1)*Q as Q + i*Q and convert to affineCached
        Ꮡ(v.points, i + 1).FromP3(ᏑtmpP3.fromP1xP1(ᏑtmpP1xP1.AddAffine(q, Ꮡ(v.points, i))));
    }
}

// Builds a lookup table at runtime. Fast.
internal static void FromP3(this ref nafLookupTable5 v, ж<Point> q) {
    // Goal: v.points[i] = (2*i+1)*Q, i.e., Q, 3Q, 5Q, ..., 15Q
    // This allows lookup of -15Q, ..., -3Q, -Q, 0, Q, 3Q, ..., 15Q
    Ꮡ(v.points, 0).FromP3(q);
    ref var q2 = ref heap<Point>(new Point(), out var Ꮡq2);
    Ꮡq2.Add(q, q);
    ref var tmpP3 = ref heap<Point>(new Point(), out var ᏑtmpP3);
    ref var tmpP1xP1 = ref heap<projP1xP1>(new projP1xP1(), out var ᏑtmpP1xP1);
    for (nint i = 0; i < 7; i++) {
        Ꮡ(v.points, i + 1).FromP3(ᏑtmpP3.fromP1xP1(ᏑtmpP1xP1.Add(Ꮡq2, Ꮡ(v.points, i))));
    }
}

// This is not optimised for speed; fixed-base tables should be precomputed.
internal static void FromP3(this ref nafLookupTable8 v, ж<Point> q) {
    Ꮡ(v.points, 0).FromP3(q);
    ref var q2 = ref heap<Point>(new Point(), out var Ꮡq2);
    Ꮡq2.Add(q, q);
    ref var tmpP3 = ref heap<Point>(new Point(), out var ᏑtmpP3);
    ref var tmpP1xP1 = ref heap<projP1xP1>(new projP1xP1(), out var ᏑtmpP1xP1);
    for (nint i = 0; i < 63; i++) {
        Ꮡ(v.points, i + 1).FromP3(ᏑtmpP3.fromP1xP1(ᏑtmpP1xP1.AddAffine(Ꮡq2, Ꮡ(v.points, i))));
    }
}

// Selectors.

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
internal static void SelectInto(this ref projLookupTable v, ж<projCached> dest, int8 x) {
    // Compute xabs = |x|
    var xmask = (int8)((x >> 7));
    var xabs = (uint8)((int8)((x + xmask) ^ xmask));

    dest.Zero();
    for (nint j = 1; j <= 8; j++) {
        // Set dest = j*Q if |x| = j
        nint cond = constanttime.ByteEq(xabs, (uint8)j);
        dest.Select(Ꮡ(v.points, j - 1), dest, cond);
    }
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg((nint)((int8)(xmask & 1)));
}

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
internal static void SelectInto(this ref affineLookupTable v, ж<affineCached> dest, int8 x) {
    // Compute xabs = |x|
    var xmask = (int8)((x >> 7));
    var xabs = (uint8)((int8)((x + xmask) ^ xmask));

    dest.Zero();
    for (nint j = 1; j <= 8; j++) {
        // Set dest = j*Q if |x| = j
        nint cond = constanttime.ByteEq(xabs, (uint8)j);
        dest.Select(Ꮡ(v.points, j - 1), dest, cond);
    }
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg((nint)((int8)(xmask & 1)));
}

// Given odd x with 0 < x < 2^4, return x*Q (in variable time).
internal static void SelectInto(this ref nafLookupTable5 v, ж<projCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.DerefOrNull();

    dest = v.points[(int8)(x / 2)];
}

// Given odd x with 0 < x < 2^7, return x*Q (in variable time).
internal static void SelectInto(this ref nafLookupTable8 v, ж<affineCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.DerefOrNull();

    dest = v.points[(int8)(x / 2)];
}

} // end edwards25519_package

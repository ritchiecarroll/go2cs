// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static go.@internal.dag_package;

partial class dag_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBACADBDCDˢ => ref aBACADBDCDˢᵇ;
private static readonly @string aBACADBDCDˢᵇ = "a->b a->c a->d b->d c->d"u8;

public static void TestTranspose(ж<testing.T> t) {
    var g = mustParse(t, diamond);
    g.Transpose();
    wantEdges(t, g, aBACADBDCDˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dCBAˢ => ref dCBAˢᵇ;
private static readonly @string dCBAˢᵇ = "d c b a"u8;

public static void TestTopo(ж<testing.T> t) {
    var g = mustParse(t, diamond);
    var got = g.Topo();
    // "d" is the root, so it's first.
    //
    // "c" and "b" could be in either order, but Topo is
    // deterministic in reverse node definition order.
    //
    // "a" is a leaf.
    var wantNodes = strings.Fields(dCBAˢ);
    if (!slices.Equal<slice<@string>, @string>(wantNodes, got)) {
        t.Fatalf("want topo sort %v, got %v", wantNodes, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string diamondˢ => ref diamondˢᵇ;
private static readonly @string diamondˢᵇ = "diamond"u8;
internal static ref readonly @string bACADBDCˢ => ref bACADBDCˢᵇ;
private static readonly @string bACADBDCˢᵇ = "b->a c->a d->b d->c"u8;
internal static ref readonly @string chainˢ => ref chainˢᵇ;
private static readonly @string chainˢᵇ = "chain"u8;
internal static ref readonly @string eDDCCBBAˢ => ref eDDCCBBAˢᵇ;
private static readonly @string eDDCCBBAˢᵇ = "e->d d->c c->b b->a"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string chainᶜ => ref chainᶜᵇ;
private static readonly @string chainᶜᵇ = @"NONE < a < b < c < d; a, d < e;"u8;

public static void TestTransitiveReduction(ж<testing.T> t) {
    t.Run(diamondˢ, (ж<testing.T> tΔ1) => {
        var g = mustParse(tΔ1, diamond);
        g.TransitiveReduction();
        wantEdges(tΔ1, g, bACADBDCˢ);
    });
    t.Run(chainˢ, (ж<testing.T> tΔ2) => {
        @string chain = chainᶜ;
        var g = mustParse(tΔ2, chain);
        g.TransitiveReduction();
        wantEdges(tΔ2, g, eDDCCBBAˢ);
    });
}

} // end dag_internal_test_package

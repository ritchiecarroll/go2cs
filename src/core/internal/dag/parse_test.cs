// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static go.@internal.dag_package;

partial class dag_internal_test_package {

internal static ref readonly @string diamond => ref diamondᵇ;
private static readonly @string diamondᵇ = """

NONE < a < b, c < d;

"""u8.Raw();

internal static ж<global::go.@internal.dag_package.Graph> mustParse(ж<testing.T> t, @string dag) {
    t.Helper();
    var (g, err) = Parse(dag);
    if (err != nil) {
        t.Fatal(err);
    }
    return g;
}

internal static void wantEdges(ж<testing.T> t, ж<global::go.@internal.dag_package.Graph> Ꮡg, @string edges) {
    ref var g = ref Ꮡg.DerefOrNull();

    t.Helper();

    var wantEdges = strings.Fields(edges);
    var wantEdgeMap = new map<@string, bool>();
    foreach (var (_, e) in range(wantEdges)) {
        wantEdgeMap[e] = true;
    }

    foreach (var (_, n1) in range(g.Nodes)) {
        foreach (var (_, n2) in range(g.Nodes)) {
            var got = g.HasEdge(n1, n2);
            var want = wantEdgeMap[n1 + "->"u8 + n2];
            if (got && want) {
                t.Logf("%s->%s", n1, n2);
            } else if (got && !want) {
                t.Errorf("%s->%s present but not expected", n1, n2);
            } else if (want && !got) {
                t.Errorf("%s->%s missing but expected", n1, n2);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBCDˢ => ref aBCDˢᵇ;
private static readonly @string aBCDˢᵇ = "a b c d"u8;
internal static ref readonly @string bACADADBDCˢ => ref bACADADBDCˢᵇ;
private static readonly @string bACADADBDCˢᵇ = "b->a c->a d->a d->b d->c"u8;

public static void TestParse(ж<testing.T> t) {
    // Basic smoke test for graph parsing.
    var g = mustParse(t, diamond);

    var wantNodes = strings.Fields(aBCDˢ);
    if (!slices.Equal<slice<@string>, @string>(wantNodes, g.Nodes)) {
        t.Fatalf("want nodes %v, got %v", wantNodes, g.Nodes);
    }

    // Parse returns the transitive closure, so it adds d->a.
    wantEdges(t, g, bACADADBDCˢ);
}

} // end dag_internal_test_package

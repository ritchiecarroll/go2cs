// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static go.@internal.dag_package;

partial class dag_internal_test_package {

internal static readonly @string diamond = """

NONE < a < b, c < d;

"""u8.Raw();

internal static ж<global::go.@internal.dag_package.Graph> mustParse(ж<testing.T> Ꮡt, @string dag) {
    Ꮡt.Helper();
    var (g, err) = Parse(dag);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return g;
}

internal static void wantEdges(ж<testing.T> Ꮡt, ж<global::go.@internal.dag_package.Graph> Ꮡg, @string edges) {
    ref var g = ref Ꮡg.DerefOrNull();

    Ꮡt.Helper();

    var wantEdges = strings.Fields(edges);
    var wantEdgeMap = new map<@string, bool>();
    foreach (var (_, e) in wantEdges) {
        wantEdgeMap[e] = true;
    }

    foreach (var (_, n1) in g.Nodes) {
        foreach (var (_, n2) in g.Nodes) {
            var got = g.HasEdge(n1, n2);
            var want = wantEdgeMap[n1 + "->"u8 + n2];
            if (got && want){
                Ꮡt.Logf("%s->%s"u8, n1, n2);
            } else 
            if (got && !want){
                Ꮡt.Errorf("%s->%s present but not expected"u8, n1, n2);
            } else 
            if (want && !got) {
                Ꮡt.Errorf("%s->%s missing but expected"u8, n1, n2);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCDˢ = "a b c d"u8;
internal static readonly @string bACADADBDCˢ = "b->a c->a d->a d->b d->c"u8;

public static void TestParse(ж<testing.T> Ꮡt) {
    // Basic smoke test for graph parsing.
    var g = mustParse(Ꮡt, diamond);

    var wantNodes = strings.Fields(aBCDˢ);
    if (!slices.Equal<slice<@string>, @string>(wantNodes, (~g).Nodes)) {
        Ꮡt.Fatalf("want nodes %v, got %v"u8, wantNodes, (~g).Nodes);
    }

    // Parse returns the transitive closure, so it adds d->a.
    wantEdges(Ꮡt, g, bACADADBDCˢ);
}

} // end dag_internal_test_package

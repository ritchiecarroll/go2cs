// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class dag_package {

// Transpose reverses all edges in g.
public static void Transpose(this ref Graph g) {
    var old = g.edges;
    g.edges = new map<@string, map<@string, bool>>();
    foreach (var (_, n) in g.Nodes) {
        g.edges[n] = new map<@string, bool>();
    }
    foreach (var (from, tos) in old) {
        foreach (var (to, _) in tos) {
            g.edges[to].Set(from, true);
        }
    }
}

// Topo returns a topological sort of g. This function is deterministic.
public static slice<@string> Topo(this ж<Graph> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();

    ref var topo = ref heap<slice<@string>>(out var Ꮡtopo);
    topo = new slice<@string>(0, len(g.Nodes));
    var marks = new map<@string, bool>();
    ref var visit = ref heap<Action<@string>>(out var Ꮡvisit);
    var marksʗ1 = marks;
    visit = (@string n) => {
        if (marksʗ1[n]) {
            return;
        }
        foreach (var (_, to) in Ꮡg.Edges(n)) {
            Ꮡvisit.ValueSlot(to);
        }
        marksʗ1[n] = true;
        Ꮡtopo.ValueSlot = append(Ꮡtopo.ValueSlot, n);
    };
    foreach (var (_, root) in g.Nodes) {
        visit(root);
    }
    for ((nint i, nint j) = (0, len(topo) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (topo[i], topo[j]) = (topo[j], topo[i]);
    }
    return topo;
}

// TransitiveReduction removes edges from g that are transitively
// reachable. g must be transitively closed.
public static void TransitiveReduction(this ref Graph g) {
    // For i -> j -> k, if i -> k exists, delete it.
    foreach (var (_, i) in g.Nodes) {
        foreach (var (_, j) in g.Nodes) {
            if (g.HasEdge(i, j)) {
                foreach (var (_, k) in g.Nodes) {
                    if (g.HasEdge(j, k)) {
                        g.DelEdge(i, k);
                    }
                }
            }
        }
    }
}

} // end dag_package

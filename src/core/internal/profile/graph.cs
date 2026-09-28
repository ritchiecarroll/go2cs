// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Package profile represents a pprof profile as a directed graph.
//
// This package is a simplified fork of github.com/google/pprof/internal/graph.
namespace go.@internal;

using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;

partial class profile_package {

// Options encodes the options for constructing a graph
[GoType] partial struct Options {
    public Func<slice<int64>, int64> SampleValue; // Function to compute the value of a sample
    public Func<slice<int64>, int64> SampleMeanDivisor; // Function to compute the divisor for mean graphs, or nil
    public bool DropNegative; // Drop nodes with overall negative values
    public NodeSet KeptNodes; // If non-nil, only use nodes in this set
}

[GoType("[]ж<Node>")] partial struct Nodes;

// Node is an entry on a profiling report. It represents a unique
// program location.
[GoType] partial struct Node {
    // Info describes the source location associated to this node.
    public NodeInfo Info;
    // Function represents the function that this node belongs to. On
    // graphs with sub-function resolution (eg line number or
    // addresses), two nodes in a NodeMap that are part of the same
    // function have the same value of Node.Function. If the Node
    // represents the whole function, it points back to itself.
    public ж<Node> Function;
    // Values associated to this node. Flat is exclusive to this node,
    // Cum includes all descendents.
    public int64 Flat, FlatDiv, Cum, CumDiv;
    // In and out Contains the nodes immediately reaching or reached by
    // this node.
    public EdgeMap In, Out;
}

// Graph summarizes a performance profile into a format that is
// suitable for visualization.
[GoType] partial struct Graph {
    public Nodes Nodes;
}

// FlatValue returns the exclusive value for this node, computing the
// mean if a divisor is available.
[GoRecv] public static int64 FlatValue(this ref Node n) {
    if (n.FlatDiv == 0) {
        return n.Flat;
    }
    return quo(n.Flat, n.FlatDiv);
}

// CumValue returns the inclusive value for this node, computing the
// mean if a divisor is available.
[GoRecv] public static int64 CumValue(this ref Node n) {
    if (n.CumDiv == 0) {
        return n.Cum;
    }
    return quo(n.Cum, n.CumDiv);
}

// AddToEdge increases the weight of an edge between two nodes. If
// there isn't such an edge one is created.
public static void AddToEdge(this ж<Node> Ꮡn, ж<Node> Ꮡto, int64 v, bool residual, bool inline) {
    Ꮡn.AddToEdgeDiv(Ꮡto, 0, v, residual, inline);
}

// AddToEdgeDiv increases the weight of an edge between two nodes. If
// there isn't such an edge one is created.
public static void AddToEdgeDiv(this ж<Node> Ꮡn, ж<Node> Ꮡto, int64 dv, int64 v, bool residual, bool inline) {
    ref var n = ref Ꮡn.DerefOrNull();
    ref var to = ref Ꮡto.DerefOrNull();

    {
        var e = n.Out.FindTo(Ꮡto); if (e != nil) {
            e.Value.WeightDiv += dv;
            e.Value.Weight += v;
            if (residual) {
                e.Value.Residual = true;
            }
            if (!inline) {
                e.Value.Inline = false;
            }
            return;
        }
    }
    var info = Ꮡ(new Edge(Src: Ꮡn, Dest: Ꮡto, WeightDiv: dv, Weight: v, Residual: residual, Inline: inline));
    n.Out.Add(info);
    to.In.Add(info);
}

// NodeInfo contains the attributes for a node.
[GoType] partial struct NodeInfo {
    public @string Name;
    public uint64 Address;
    public nint StartLine, Lineno;
}

// PrintableName calls the Node's Formatter function with a single space separator.
[GoRecv] public static @string PrintableName(this ref NodeInfo i) {
    return strings.Join(i.NameComponents(), " "u8);
}

// NameComponents returns the components of the printable name to be used for a node.
[GoRecv] public static slice<@string> NameComponents(this ref NodeInfo i) {
    slice<@string> name = default!;
    if (i.Address != 0) {
        name = append(name, fmt.Sprintf("%016x"u8, i.Address));
    }
    {
        @string fun = i.Name; if (fun != ""u8) {
            name = append(name, fun);
        }
    }
    switch (ᐧ) {
    case {} when i.Lineno is not 0: {
        name = append(name, // User requested line numbers, provide what we have.
 fmt.Sprintf(":%d"u8, i.Lineno));
        break;
    }
    case {} when i.Name != ""u8: {
        break;
    }
    default: {
        name = append(name, // User requested function name. It was already included.
 // Do not leave it empty if there is no information at all.
 "<unknown>"u8);
        break;
    }}

    return name;
}

[GoType("map[NodeInfo, ж<Node>]")] partial struct NodeMap;

[GoType("map[NodeInfo, bool]")] partial struct NodeSet;

[GoType("map[ж<Node>, bool]")] partial struct NodePtrSet;

// FindOrInsertNode takes the info for a node and either returns a matching node
// from the node map if one exists, or adds one to the map if one does not.
// If kept is non-nil, nodes are only added if they can be located on it.
public static ж<Node> FindOrInsertNode(this NodeMap nm, NodeInfo info, NodeSet kept) {
    if (kept != default!) {
        {
            var (_, ok) = kept[info, ꟷ]; if (!ok) {
                return default!;
            }
        }
    }
    {
        var (nΔ1, ok) = nm[info, ꟷ]; if (ok) {
            return nΔ1;
        }
    }
    var n = Ꮡ(new Node(
        Info: info
    ));
    nm[info] = n;
    if (info.Address == 0 && info.Lineno == 0) {
        // This node represents the whole function, so point Function
        // back to itself.
        n.Value.Function = n;
        return n;
    }
    // Find a node that represents the whole function.
    info.Address = 0;
    info.Lineno = 0;
    n.Value.Function = nm.FindOrInsertNode(info, default!);
    return n;
}

[GoType("[]ж<Edge>")] partial struct EdgeMap;

public static ж<Edge> FindTo(this EdgeMap em, ж<Node> Ꮡn) {
    foreach (var (_, e) in em) {
        if ((~e).Dest == Ꮡn) {
            return e;
        }
    }
    return default!;
}

[GoRecv] public static void Add(this ref EdgeMap em, ж<Edge> Ꮡe) {
    em = append(em, Ꮡe);
}

[GoRecv] public static void Delete(this ref EdgeMap em, ж<Edge> Ꮡe) {
    foreach (var (i, edge) in em) {
        if (edge == Ꮡe) {
            (em)[i] = (em)[len(em) - 1];
            em = (em)[..(int)(len(em) - 1)];
            return;
        }
    }
}

// Edge contains any attributes to be represented about edges in a graph.
[GoType] partial struct Edge {
    public ж<Node> Src, Dest;
    // The summary weight of the edge
    public int64 Weight, WeightDiv;
    // residual edges connect nodes that were connected through a
    // separate node, which has been removed from the report.
    public bool Residual;
    // An inline edge represents a call that was inlined into the caller.
    public bool Inline;
}

// WeightValue returns the weight value for this edge, normalizing if a
// divisor is available.
[GoRecv] public static int64 WeightValue(this ref Edge e) {
    if (e.WeightDiv == 0) {
        return e.Weight;
    }
    return quo(e.Weight, e.WeightDiv);
}

// NewGraph computes a graph from a profile.
public static ж<Graph> NewGraph(ж<Profile> Ꮡprof, ж<Options> Ꮡo) {
    ref var prof = ref Ꮡprof.DerefOrNull();
    ref var o = ref Ꮡo.DerefOrNull();

    var (nodes, locationMap) = CreateNodes(Ꮡprof, Ꮡo);
    var seenNode = new map<ж<Node>, bool>();
    var seenEdge = new map<nodePair, bool>();
    foreach (var (_, sample) in prof.Sample) {
        int64 w = default!;
        int64 dw = default!;
        w = o.SampleValue((~sample).Value);
        if (o.SampleMeanDivisor != default!) {
            dw = o.SampleMeanDivisor((~sample).Value);
        }
        if (dw == 0 && w == 0) {
            continue;
        }
        foreach (var (k, _) in seenNode) {
            delete(seenNode, k);
        }
        foreach (var (k, _) in seenEdge) {
            delete(seenEdge, k);
        }
        ж<Node> parent = default!;
        // A residual edge goes over one or more nodes that were not kept.
        var residual = false;
        // Group the sample frames, based on a global map.
        // Count only the last two frames as a call edge. Frames higher up
        // the stack are unlikely to be repeated calls (e.g. runtime.main
        // calling main.main). So adding weights to call edges higher up
        // the stack may be not reflecting the actual call edge weights
        // in the program. Without a branch profile this is just an
        // approximation.
        nint i = 1;
        {
            nint last = len((~sample).Location) - 1; if (last < i) {
                i = last;
            }
        }
        for (; i >= 0; i--) {
            var l = (~sample).Location[i];
            var locNodes = locationMap.get((~l).ID);
            for (nint ni = len(locNodes) - 1; ni >= 0; ni--) {
                var n = locNodes[ni];
                if (n == nil) {
                    residual = true;
                    continue;
                }
                // Add cum weight to all nodes in stack, avoiding double counting.
                var (_, sawNode) = seenNode[n, ꟷ];
                if (!sawNode) {
                    seenNode[n] = true;
                    n.addSample(dw, w, false);
                }
                // Update edge weights for all edges in stack, avoiding double counting.
                if ((!sawNode || !seenEdge[new nodePair(n, parent)]) && parent != nil && n != parent) {
                    seenEdge[new nodePair(n, parent)] = true;
                    parent.AddToEdgeDiv(n, dw, w, residual, ni != len(locNodes) - 1);
                }
                parent = n;
                residual = false;
            }
        }
        if (parent != nil && !residual) {
            // Add flat weight to leaf node.
            parent.addSample(dw, w, true);
        }
    }
    return selectNodesForGraph(nodes, o.DropNegative);
}

internal static ж<Graph> selectNodesForGraph(Nodes nodes, bool dropNegative) {
    // Collect nodes into a graph.
    var gNodes = new Nodes(0, len(nodes));
    foreach (var (_, n) in nodes) {
        if (n == nil) {
            continue;
        }
        if ((~n).Cum == 0 && (~n).Flat == 0) {
            continue;
        }
        if (dropNegative && isNegative(ref (n).DerefOrNull())) {
            continue;
        }
        gNodes = append(gNodes, n);
    }
    return Ꮡ(new Graph(gNodes));
}

[GoType] partial struct nodePair {
    internal ж<Node> src, dest;
}

// isNegative returns true if the node is considered as "negative" for the
// purposes of drop_negative.
internal static bool isNegative(ref Node n) {
    switch (ᐧ) {
    case {} when n.Flat is < 0: {
        return true;
    }
    case {} when n.Flat == 0 && n.Cum < 0: {
        return true;
    }
    default: {
        return false;
    }}

}

[GoType] public partial struct locationMap {
    internal slice<Nodes> s;     // a slice for small sequential IDs
    internal map<uint64, Nodes> m; // fallback for large IDs (unlikely)
}

[GoRecv] internal static void add(this ref locationMap l, uint64 id, Nodes n) {
    if (id < (uint64)len(l.s)){
        l.s[(nint)(id)] = n;
    } else {
        l.m[id] = n;
    }
}

internal static Nodes get(this locationMap l, uint64 id) {
    if (id < (uint64)len(l.s)){
        return l.s[(nint)(id)];
    } else {
        return l.m[id];
    }
}

// CreateNodes creates graph nodes for all locations in a profile. It
// returns set of all nodes, plus a mapping of each location to the
// set of corresponding nodes (one per location.Line).
public static (Nodes, locationMap) CreateNodes(ж<Profile> Ꮡprof, ж<Options> Ꮡo) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    var locations = new locationMap(new slice<Nodes>(len(prof.Location) + 1), new map<uint64, Nodes>());
    var nm = new NodeMap(len(prof.Location));
    foreach (var (_, l) in prof.Location) {
        var lines = l.Value.Line;
        if (len(lines) == 0) {
            lines = new Line[]{new()}.slice(); // Create empty line to include location info.
        }
        var nodes = new Nodes(len(lines));
        foreach (var (ln, _) in lines) {
            nodes[ln] = nm.findOrInsertLine(l, lines[ln], Ꮡo);
        }
        locations.add((~l).ID, nodes);
    }
    return (nm.nodes(), locations);
}

internal static Nodes nodes(this NodeMap nm) {
    var nodes = new Nodes(0, len(nm));
    foreach (var (_, n) in nm) {
        nodes = append(nodes, n);
    }
    return nodes;
}

internal static ж<Node> findOrInsertLine(this NodeMap nm, ж<Location> Ꮡl, Line li, ж<Options> Ꮡo) {
    ref var l = ref Ꮡl.DerefOrNull();
    ref var o = ref Ꮡo.DerefOrNull();

    @string objfile = default!;
    {
        var m = l.Mapping; if (m != nil && (~m).File != ""u8) {
            objfile = m.Value.File;
        }
    }
    {
        var ni = nodeInfo(ref (Ꮡl).DerefOrNull(), li, objfile, ref (Ꮡo).DerefOrNull()); if (ni != nil) {
            return nm.FindOrInsertNode(ni.Value, o.KeptNodes);
        }
    }
    return default!;
}

internal static ж<NodeInfo> nodeInfo(ref Location l, Line line, @string objfile, ref Options o) {
    if (line.Function == nil) {
        return Ꮡ(new NodeInfo(Address: l.Address));
    }
    var ni = Ꮡ(new NodeInfo(
        Address: l.Address,
        Lineno: (nint)line.ΔLine,
        Name: (~line.Function).Name
    ));
    ni.Value.StartLine = (nint)(~line.Function).StartLine;
    return ni;
}

// Sum adds the flat and cum values of a set of nodes.
public static (int64 flat, int64 cum) Sum(this Nodes ns) {
    int64 flat = default!;
    int64 cum = default!;

    foreach (var (_, n) in ns) {
        flat += n.Value.Flat;
        cum += n.Value.Cum;
    }
    return (flat, cum);
}

[GoRecv] internal static void addSample(this ref Node n, int64 dw, int64 w, bool flat) {
    // Update sample value
    if (flat){
        n.FlatDiv += dw;
        n.Flat += w;
    } else {
        n.CumDiv += dw;
        n.Cum += w;
    }
}

// String returns a text representation of a graph, for debugging purposes.
[GoRecv] public static @string String(this ref Graph g) {
    slice<@string> s = default!;
    var nodeIndex = new map<ж<Node>, nint>(len(g.Nodes));
    foreach (var (i, n) in g.Nodes) {
        nodeIndex[n] = i + 1;
    }
    foreach (var (i, n) in g.Nodes) {
        @string name = n.of(Node.ᏑInfo).PrintableName();
        slice<nint> @in = default!;
        slice<nint> @out = default!;
        foreach (var (_, from) in (~n).In) {
            @in = append(@in, nodeIndex[(~from).Src]);
        }
        foreach (var (_, to) in (~n).Out) {
            @out = append(@out, nodeIndex[(~to).Dest]);
        }
        s = append(s, fmt.Sprintf("%d: %s[flat=%d cum=%d] %x -> %v "u8, i + 1, name, (~n).Flat, (~n).Cum, @in, @out));
    }
    return strings.Join(s, "\n"u8);
}

// Sort returns a slice of the edges in the map, in a consistent
// order. The sort order is first based on the edge weight
// (higher-to-lower) and then by the node names to avoid flakiness.
public static slice<ж<Edge>> Sort(this EdgeMap em) {
    var el = new edgeList(0, len(em));
    foreach (var (_, w) in em) {
        el = append(el, w);
    }
    sort.Sort(el);
    return el;
}

// Sum returns the total weight for a set of nodes.
public static int64 Sum(this EdgeMap em) {
    int64 ret = default!;
    foreach (var (_, edge) in em) {
        ret += edge.Value.Weight;
    }
    return ret;
}

[GoType("[]ж<Edge>")] partial struct edgeList;

internal static nint Len(this edgeList el) {
    return len(el);
}

internal static bool Less(this edgeList el, nint i, nint j) {
    if ((~el[i]).Weight != (~el[j]).Weight) {
        return abs64((~el[i]).Weight) > abs64((~el[j]).Weight);
    }
    @string from1 = (~el[i]).Src.of(Node.ᏑInfo).PrintableName();
    @string from2 = (~el[j]).Src.of(Node.ᏑInfo).PrintableName();
    if (from1 != from2) {
        return from1 < from2;
    }
    @string to1 = (~el[i]).Dest.of(Node.ᏑInfo).PrintableName();
    @string to2 = (~el[j]).Dest.of(Node.ᏑInfo).PrintableName();
    return to1 < to2;
}

internal static void Swap(this edgeList el, nint i, nint j) {
    (el[i], el[j]) = (el[j], el[i]);
}

internal static int64 abs64(int64 i) {
    if (i < 0) {
        return -i;
    }
    return i;
}

} // end profile_package

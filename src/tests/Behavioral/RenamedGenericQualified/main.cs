global using pairAlias = go.RenamedGenericQualified.coll_package.Pair<go.@string, nint>;

namespace go;

using fmt = fmt_package;
using coll = RenamedGenericQualified.coll_package;
using RenamedGenericQualified;

partial class main_package {

partial struct holder {
    internal coll.Set<@string> seen;
    internal slice<coll.Pair<@string, nint>> pairs;
    internal map<@string, ж<coll.Set<nint>>> byKey;
    internal coll.Bag<coll.Set<@string>> bag;
}

internal static nint count(ж<coll.Set<@string>> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.Len();
}

internal static coll.Pair<@string, nint> makePair(@string k, nint v) { return new coll.Pair<@string, nint>(Key: k, Val: v); }

internal partial struct main_local /*dyn*/ {
    internal coll.Set<nint> s;
}

internal static void main() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    h.seen.Add((@string)"a");
    h.seen.Add((@string)"a");
    h.seen.Add((@string)"b");
    fmt.Println(h.seen.Len(), count(Ꮡh.Ꮡseen), h.seen.Has((@string)"b"), h.seen.Has((@string)"z"));

    h.pairs = append(h.pairs, makePair("x", 1), new coll.Pair<@string, nint>(Key: "y", Val: 2));
    h.byKey = new map<@string, ж<coll.Set<nint>>>{["odd"] = coll.NewSet((nint)1, (nint)3, (nint)5)};
    fmt.Println(len(h.pairs), h.pairs[1].Key, h.pairs[1].Val, h.byKey["odd"].Len());

    coll.OrderedMap<@string, nint> o = default;
    o.Set((@string)"k", 7);
    var e = new coll.Entry();
    var p = e.Pair();
    pairAlias pa = p;
    fmt.Println(pa.Key == ""u8, pa.Val);

    h.bag.Put(new coll.Set<@string>());
    fmt.Println(h.bag.Count());

    any anyv = makePair("z", 26);
    var (q, ok) = anyv._<coll.Pair<@string, nint>>(ᐧ); if (ok) {
        fmt.Println("asserted", q.Key, q.Val);
    }
    switch (anyv.type()) {
    case coll.Set<@string> v: {
        fmt.Println("set", v.Len());
        break;
    }
    case coll.Pair<@string, nint> v: {
        fmt.Println("pair", v.Val);
        break;
    }}
    ref var l = ref heap(new main_local(), out var Ꮡl);
    l.s.Add(4);
    var ps = slice<ж<coll.Set<nint>>>([Ꮡl.Ꮡs]);
    fmt.Println(ps[0].Len(), (coll.Kind)3, e.Kind());
}

} // end main_package

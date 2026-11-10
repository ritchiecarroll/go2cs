namespace go.RenamedGenericQualified;

partial class coll_package {

partial struct ΔSet<T> {
    internal map<T, EmptyStruct> m;
}

public static void Add<T>(this ref ΔSet<T> s, T v) {
    if (s.m == nil) {
        s.m = new map<T, EmptyStruct>{};
    }
    s.m[v] = new EmptyStruct();
}

public static bool Has<T>(this ref ΔSet<T> s, T v) {
    var (_, ok) = s.m[v, ꟷ];
    return ok;
}

public static nint Len<T>(this ref ΔSet<T> s) { return len(s.m); }

public static ж<ΔSet<T>> NewSet<T>(params Span<T> vsʗp) {
    var vs = vsʗp.sslice();

    var s = Ꮡ(new ΔSet<T>());
    foreach (var (_, v) in range(vs)) {
        s.Add(v);
    }
    return s;
}

partial struct ΔPair<K, V> {
    public K Key;
    public V Val;
}

partial struct OrderedMap<K, V> {
    internal slice<K> keys;
    internal map<K, V> m;
}

public static void Set<K, V>(this ref OrderedMap<K, V> o, K k, V v) {
    if (o.m == nil) {
        o.m = new map<K, V>{};
    }
    var (_, ok) = o.m[k, ꟷ]; if (!ok) {
        o.keys = append(o.keys, k);
    }
    o.m[k] = v;
}

partial struct Entry {
    internal @string k;
}

public static ΔPair<@string, nint> Pair(this Entry e) { return new ΔPair<@string, nint>(Key: e.k, Val: len(e.k)); }

public static ΔKind Kind(this Entry e) { return (ΔKind)(byte)len(e.k); }

partial struct ΔKind /*num:byte*/;

partial struct Bag<T> {
    internal slice<T> items;
}

public static void Put<T>(this ref Bag<T> b, T v) { b.items = append(b.items, v); }

public static nint Count<T>(this ref Bag<T> b) { return len(b.items); }

} // end coll_package

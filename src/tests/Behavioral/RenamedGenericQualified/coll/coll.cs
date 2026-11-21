namespace go.RenamedGenericQualified;

partial class coll_package {

partial struct Set<T> {
    internal map<T, EmptyStruct> m;
}

internal static void Add⁔<T>(/*this*/ ref Set<T> s, T v) {
    if (s.m == nil) {
        s.m = new map<T, EmptyStruct>{};
    }
    s.m[v] = new EmptyStruct();
}

internal static bool Has⁔<T>(/*this*/ ref Set<T> s, T v) {
    var (_, ok) = s.m[v, ꟷ];
    return ok;
}

internal static nint Len⁔<T>(/*this*/ ref Set<T> s) { return len(s.m); }

public static ж<Set<T>> NewSet<T>(params Span<T> vsʗp) {
    var vs = vsʗp.sslice();

    var s = Ꮡ(new Set<T>());
    foreach (var (_, v) in range(vs)) {
        s.Add(v);
    }
    return s;
}

partial struct Pair<K, V> {
    public K Key;
    public V Val;
}

partial struct OrderedMap<K, V> {
    internal slice<K> keys;
    internal map<K, V> m;
}

internal static void Set⁔<K, V>(/*this*/ ref OrderedMap<K, V> o, K k, V v) {
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

internal static Pair<@string, nint> Pair⁔(/*this*/ Entry e) { return new Pair<@string, nint>(Key: e.k, Val: len(e.k)); }

internal static Kind Kind⁔(/*this*/ Entry e) { return (Kind)(byte)len(e.k); }

partial struct Kind /*num:byte*/;

partial struct Bag<T> {
    internal slice<T> items;
}

internal static void Put⁔<T>(/*this*/ ref Bag<T> b, T v) { b.items = append(b.items, v); }

internal static nint Count⁔<T>(/*this*/ ref Bag<T> b) { return len(b.items); }

} // end coll_package

global using Pairs = go.slice<go.main_package.Pair<go.@string, nint>>;

namespace go;

using fmt = fmt_package;
using lib = GenericTypeAliasLib_package;
using GenericTypeAliasLib = GenericTypeAliasLib_package;

partial class main_package {

partial struct Pair<K, V> {
    public K Key;
    public V Val;
}
// type P[K comparable, V any] = Pair[K, V]
// type StrPair[V any] = Pair[string, V]
// type SP[V any] = StrPair[V]
// type IntMap[V any] = map[int]V

internal static Pair<T, T> swap<T>(Pair<T, T> p) {
    return new Pair<T, T>(Key: p.Val, Val: p.Key);
}

internal static lib.Box<T> wrap<T>(T v) {
    lib.Box<T> a = lib.NewBox<T>(v);
    var b = new lib.Box<T>(V: a.Get());
    return b;
}

internal static void Main() {
    Pair<@string, nint> p = new Pair<@string, nint>(Key: "a", Val: 1);
    var sp = new Pair<@string, bool>(Key: "b", Val: true);
    Pair<@string, bool> sp2 = sp;
    var m = new map<nint, @string>{[1] = "one"};
    fmt.Println("local:", p, sp, sp2, m[1], swap<nint>(new Pair<nint, nint>(Key: 1, Val: 2)));

    Pair<@string, nint> pair = p;
    p = pair;
    any pi = p;
    var (_, isPair) = pi._<Pair<@string, nint>>(ᐧ);
    fmt.Println("identity:", pair, isPair);

    lib.Box<nint> a = new lib.Box<nint>(V: 5);
    a.Set(6);
    ref var b = ref heap(new lib.Box<nint>(), out var Ꮡb);

    b = a;
    var s = new map<@string, EmptyStruct>{["x"] = new()};
    var l = new nint[]{1, 2, 3}.slice();
    Func<nint, nint> f = (nint x) => x * 2;
    ж<lib.Box<nint>> ptr = Ꮡb;
    ptr.Set(7);
    var n = new lib.Box<float64>(V: 1.5D);
    fmt.Println("cross:", a.Get(), b.Get(), len(s), l, f(4), lib.Twice(f, 3), n.Get());
    fmt.Println("generic:", lib.Sum<nint>(l), lib.Sum<float64>(new float64[]{0.5D, 0.25D}.slice()), wrap((@string)"w").Get());

    Pairs ps = new Pair<@string, nint>[]{new(Key: "k", Val: 9)}.slice();
    GenericTypeAliasLibꓸIntBox ib = lib.NewBox<nint>(8);
    fmt.Println("plain:", ps, lib.Label("w"), lib.Unbox(ib));

    aliasedImport();
}

} // end main_package

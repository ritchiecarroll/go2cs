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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localˢ = (@string)"local:"u8;
private static readonly object identityˢ = (@string)"identity:"u8;
private static readonly object crossˢ = (@string)"cross:"u8;
private static readonly object genericˢ = (@string)"generic:"u8;
private static readonly object plainˢ = (@string)"plain:"u8;

internal static void Main() {
    Pair<@string, nint> p = new Pair<@string, nint>(Key: "a"u8, Val: 1);
    var sp = new Pair<@string, bool>(Key: "b"u8, Val: true);
    Pair<@string, bool> sp2 = sp;
    var m = new map<nint, @string>{[1] = "one"u8};
    fmt.Println(localˢ, p, sp, sp2, m[1], swap<nint>(new Pair<nint, nint>(Key: 1, Val: 2)));
    Pair<@string, nint> pair = p;
    p = pair;
    any pi = p;
    var (_, isPair) = pi._<Pair<@string, nint>>(ᐧ);
    fmt.Println(identityˢ, pair, isPair);
    lib.Box<nint> a = new lib.Box<nint>(V: 5);
    a.Set(6);
    ref var b = ref heap(new lib.Box<nint>(), out var Ꮡb);

    b = a;
    var s = new map<@string, EmptyStruct>{["x"u8] = new()};
    var l = new nint[]{1, 2, 3}.slice();
    Func<nint, nint> f = (nint x) => x * 2;
    ж<lib.Box<nint>> ptr = Ꮡb;
    ptr.Set(7);
    var n = new lib.Box<float64>(V: 1.5D);
    fmt.Println(crossˢ, a.Get(), b.Get(), len(s), l, f(4), lib.Twice(f, 3), n.Get());
    fmt.Println(genericˢ, lib.Sum<nint>(l), lib.Sum<float64>(new float64[]{0.5D, 0.25D}.slice()), wrap((@string)"w").Get());
    Pairs ps = new Pair<@string, nint>[]{new(Key: "k"u8, Val: 9)}.slice();
    GenericTypeAliasLibꓸIntBox ib = lib.NewBox<nint>(8);
    fmt.Println(plainˢ, ps, lib.Label("w"u8), lib.Unbox(ib));
    aliasedImport();
}

} // end main_package

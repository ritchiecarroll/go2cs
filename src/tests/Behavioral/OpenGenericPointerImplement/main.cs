namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Iter {
    (any, bool) Next();
}

partial struct sliceIter<T> /*[]T*/;

internal static (any, bool) Next<T>(this ref sliceIter<T> it) {
    if (len(it) == 0) {
        return (default, false);
    }
    var v = (it)[0];
    it = (it)[1..];
    return ((v).OrTypedNilOf(), true);
}

public static Iter NewIter<T>(params Span<T> valuesʗp) {
    var values = valuesʗp.slice();

    ref var it = ref heap<sliceIter<T>>((sliceIter<T>)values, out var Ꮡit);
    return new sliceIterжIter<T>(Ꮡit);
}

partial interface Namer {
    @string Name();
}

partial struct pair<K, V> {
    internal K k;
    internal V v;
}

internal static @string Name<K, V>(this pair<K, V> p) { return fmt.Sprint((p.k).OrTypedNilOf(), "=", (p.v).OrTypedNilOf()); }

internal static Namer asNamer<K, V>(K k, V v) { return new pair<K, V>(k, v); }

partial struct counter<T> {
    internal nint n;
    internal T last;
}

internal static (any, bool) Next<T>(this ref counter<T> c) {
    if (c.n == 2) {
        return (default, false);
    }
    c.n++;
    return (fmt.Sprint((c.last).OrTypedNilOf(), "#", c.n), true);
}

internal static Iter innerIter<T>(T v) { return new counterжIter<T>(Ꮡ(new counter<T>(last: v))); }

internal static Iter outer<T>(T v) { return innerIter(v); }

internal static void drain(@string name, Iter it) {
    fmt.Print(name, ":");
    for (var (v, ok) = it.Next(); ok; (v, ok) = it.Next()) {
        fmt.Print(" ", v);
    }
    fmt.Println();
}

internal static void main() {
    drain("ints", NewIter<nint>(1, 2, 3));
    drain("strings", NewIter((@string)"a", (@string)"b"));
    drain("empty", NewIter<float64>());
    fmt.Println(asNamer<@string, nint>((@string)"x", 1).Name(), asNamer<nint, float64>(2, 3.5D).Name());
    drain("chain", outer(true));

    ref var closed = ref heap<sliceIter<nint>>(new sliceIter<nint>(new nint[]{7, 8}.slice()), out var Ꮡclosed);
    Iter it = new sliceIterжIter<nint>(Ꮡclosed);
    drain("closed", it);
}

} // end main_package

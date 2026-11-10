namespace go;

using fmt = fmt_package;
using maps = maps_package;
using slices = slices_package;
using sort = sort_package;

partial class main_package {

public static map<K, V> CopyMapInto<M1, M2, K, V>(M1 dst, M2 src)
    where M1 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M1>, new()
    where M2 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M2>, new()
{
    if (dst.IsNil) {
        return maps.Clone<M2, K, V>(src).AsMap();
    }
    maps.Copy<M1, M2, K, V>(dst, src);
    return dst.AsMap();
}

public static map<K, V> MapBack<M, K, V>(M m)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{ return m.AsMap(); }

public static map<K, V> MapVar<M, K, V>(M m)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    map<K, V> r = m.AsMap();
    return r;
}

public static map<K, V> MapAssign<M, K, V>(M m)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    map<K, V> r = default;
    r = m.AsMap();
    return r;
}

internal static nint takesMap(map<@string, nint> m) { return len(m); }

public static nint MapArg<M>(M m)
    where M : /* ~map[string]int */ IMap<@string, nint>, ISupportMake<M>, new()
{ return takesMap(m.AsMap()); }

public static slice<E> SliceBack<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{ return new slice<E>(s); }

public static slice<E> SliceClone<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{ return new slice<E>(slices.Clone<S, E>(s)); }

public static slice<E> SliceVar<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    slice<E> r = new slice<E>(s);
    return r;
}

public static slice<E> SliceAssign<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    slice<E> r = default;
    r = new slice<E>(s);
    return r;
}

internal static nint takesSlice(slice<nint> s) { return len(s); }

public static nint SliceArg<S>(S s)
    where S : /* ~[]int */ ISlice<nint>, ISupportMake<S>, ISliceWrap<S, nint>, new()
{ return takesSlice(new slice<nint>(s)); }

partial struct Named /*map[@string, nint]*/;

partial struct Ints /*[]nint*/;

partial struct Tree /*map[@string, Tree]*/;

internal static slice<@string> keys<K, V>(map<K, V> m) {
    slice<@string> @out = default;
    foreach (var (k, _) in m) {
        @out = append(@out, fmt.Sprint((k).OrTypedNilOf()));
    }
    sort.Strings(@out);
    return @out;
}

internal static void Main() {
    map<@string, nint> a = default;
    a = CopyMapInto<map<@string, nint>, map<@string, nint>, @string, nint>(a, new map<@string, nint>{["x"] = 1});
    var b = CopyMapInto<map<@string, nint>, map<@string, nint>, @string, nint>(a, new map<@string, nint>{["y"] = 2});
    b["z"] = 3;
    fmt.Println(len(a), len(b), keys(a));

    var n = new Named(new map<@string, nint>{["n"] = 1});
    foreach (var (i, f) in range(slice<Func<Named, map<@string, nint>>>([MapBack<Named, @string, nint>, MapVar<Named, @string, nint>, MapAssign<Named, @string, nint>]))) {
        var r = f(n);
        r[fmt.Sprint("w", i)] = i;
    }
    fmt.Println(len(n), keys<@string, nint>(n), MapArg(n));

    var t = new Tree(new map<@string, Tree>{["leaf"] = default});
    var tr = MapBack<Tree, @string, Tree>(t);
    tr["added"] = new Tree(new map<@string, Tree>{});
    fmt.Println(len(t), keys<@string, Tree>(t));

    var s = new Ints(new nint[]{1, 2, 3}.slice());
    foreach (var (i, f) in range(slice<Func<Ints, slice<nint>>>([SliceBack<Ints, nint>, SliceVar<Ints, nint>, SliceAssign<Ints, nint>]))) {
        var r = f(s);
        r[i] = 10 * (i + 1);
    }
    var c = SliceClone<Ints, nint>(s);
    c[0] = -1;
    fmt.Println(s, c, SliceArg(s));
}

} // end main_package

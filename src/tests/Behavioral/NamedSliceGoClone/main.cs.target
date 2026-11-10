namespace go;

using fmt = fmt_package;
using slices = slices_package;

partial class main_package {

partial struct V /*[]byte*/;

public static V Clone(this V v) {
    var r = new V(len(v) + 1);
    copy(r, v);
    r[len(v)] = (byte)'!';
    return r;
}

partial struct W /*[]nint*/;

public static (W, error) Clone(this ref W w) {
    if (w == nil) {
        return (default, fmt.Errorf("nil W"u8));
    }
    return (appendꓸꓸꓸ(new W(new nint[]{-1}.slice()), (w)), default);
}

partial interface Cloner {
    V Clone();
}

partial struct GS<T> /*[]T*/;

public static GS<T> Clone<T>(this GS<T> g) { return appendꓸꓸꓸ(appendꓸꓸꓸ(new GS<T>(new T[]{}.slice()), g), g); }

partial struct M /*map[@string, nint]*/;

public static M Clone(this M m) {
    var r = new M(new map<@string, nint>{["cloned"] = 1});
    foreach (var (k, v) in m) {
        r[k] = v;
    }
    return r;
}

partial struct U /*[]@string*/;

internal static void Main() {
    ref var v = ref heap<V>((V)slice<byte>((@string)"ab"u8), out var Ꮡv);
    var c = v.Clone();
    fmt.Println((@string)(slice<byte>)c, len(c), len(v));

    any a = v.Clone();
    fmt.Printf("%T %s\n"u8, a, a);
    fmt.Println(a);

    var p = Ꮡv;
    fmt.Println((@string)(slice<byte>)p.Clone());

    Cloner i = v;
    fmt.Println((@string)(slice<byte>)i.Clone());
    
    var vʗ1 = v;
    var f = () => vʗ1.Clone();
    fmt.Println((@string)(slice<byte>)f());

    var w = new W(new nint[]{1, 2}.slice());
    var (wc, err) = w.Clone();
    fmt.Println(wc, err);
    W wn = default;
    (_, err) = wn.Clone();
    fmt.Println(err);

    var g = new GS<nint>(new nint[]{3}.slice());
    fmt.Println(g.Clone(), len(g));

    var m = new M(new map<@string, nint>{["x"] = 2});
    fmt.Println(m.Clone()["cloned"], m.Clone()["x"], len(m));

    var u = new U(new @string[]{"q"}.slice());
    var uc = slices.Clone<U, @string>(u);
    uc[0] = "r";
    fmt.Printf("%T %v %v\n"u8, uc, u, uc);
}

} // end main_package

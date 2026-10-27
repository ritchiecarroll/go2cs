namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint id;
}

internal static void Main() {
    var s = new slice<nint>(4, 8);
    foreach (var i in range(s)) {
        s[i] = i * 10;
    }

    fmt.Println("self       :", Ꮡelem(s, 0) == Ꮡelem(s, 0));
    fmt.Println("self mid   :", Ꮡelem(s, 2) == Ꮡelem(s, 2));
    fmt.Println("distinct   :", Ꮡelem(s, 0) == Ꮡelem(s, 1));

    var t = s[1..];
    fmt.Println("reslice    :", Ꮡelem(t, 0) == Ꮡelem(s, 1));
    fmt.Println("reslice x  :", Ꮡelem(t, 0) == Ꮡelem(s, 0));
    fmt.Println("reslice 2  :", Ꮡelem(t, 2) == Ꮡelem(s, 3));

    var u = t[1..];
    fmt.Println("reslice^2  :", Ꮡelem(u, 0) == Ꮡelem(s, 2));

    var w = append(s.slice(0, 0, 1), (nint)99);
    fmt.Println("append same:", Ꮡelem(w, 0) == Ꮡelem(s, 0));

    ref var a = ref heap(new array<nint>(4), out var Ꮡa);
    var v = a[..];
    fmt.Println("arr self   :", Ꮡa.Ꮡelem<nint>(1) == Ꮡa.Ꮡelem<nint>(1));
    fmt.Println("arr vs slc :", Ꮡelem(v, 2) == Ꮡa.Ꮡelem<nint>(2));
    fmt.Println("arr vs slc2:", Ꮡelem(v, 0) == Ꮡa.Ꮡelem<nint>(1));

    var ns = new slice<node>(3);
    fmt.Println("struct elem:", Ꮡelem(ns, 1) == Ꮡelem(ns, 1), Ꮡelem(ns, 1) == Ꮡelem(ns, 2));

    var p = Ꮡ(s, 1);
    p.Value = 42;
    fmt.Println("write thru :", s[1], t[0]);

    var m = new map<ж<nint>, @string>{};
    m[Ꮡ(s, 2)] = "two";
    m[Ꮡ(s, 3)] = "three";
    fmt.Println("map lookup :", m[Ꮡ(s, 2)], m[Ꮡ(s, 3)]);
    fmt.Println("map len    :", len(m));
    m[Ꮡ(s, 2)] = "TWO";
    fmt.Println("map len 2  :", len(m));
    fmt.Println("map reslice:", m[Ꮡ(t, 1)], m[Ꮡ(t, 2)]);
    var (_, miss) = m[Ꮡ(s, 0), ꟷ];
    fmt.Println("map miss   :", miss);

    var z = new slice<nint>(4);
    fmt.Println("other back :", Ꮡelem(z, 0) == Ꮡelem(s, 0));
}

} // end main_package

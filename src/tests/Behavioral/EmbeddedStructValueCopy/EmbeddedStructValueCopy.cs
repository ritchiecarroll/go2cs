namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint n;
    internal @string tag;
}

partial struct mid {
    /*embed*/ internal inner inner;
    internal nint extra;
}

partial struct deep {
    /*embed*/ internal mid mid;
    internal @string label;
}

partial struct ptrHolder {
    /*embed*/ internal ж<inner> inner;
    internal @string name;
}

internal static mid byValue(mid m) {
    m.n = 99;
    m.tag = "byValue";
    m.extra = 90;
    return m;
}

internal static deep derefCopy(ref deep p) {
    var c = p;
    c.n = 55;
    c.tag = "deref";
    c.label = "copy";
    return c;
}

internal static void Main() {
    var a = new mid(inner: new inner(n: 1, tag: "a"), extra: 10);
    var b = a;
    b.n = 2;
    b.tag = "b";
    b.extra = 20;
    fmt.Println("assign a:", a.n, a.tag, a.extra);
    fmt.Println("assign b:", b.n, b.tag, b.extra);

    var src = new mid(inner: new inner(n: 3, tag: "src"), extra: 30);
    var got = byValue(src);
    fmt.Println("call src:", src.n, src.tag, src.extra);
    fmt.Println("call got:", got.n, got.tag, got.extra);

    var d = Ꮡ(new deep(mid: new mid(inner: new inner(n: 4, tag: "d"), extra: 40), label: "orig"));
    var c = derefCopy(ref (d).DerefOrNull());
    fmt.Println("deref d:", (~d).n, (~d).tag, d.label);
    fmt.Println("deref c:", c.n, c.tag, c.label);

    var shared = Ꮡ(new inner(n: 7, tag: "shared"));
    var h1 = new ptrHolder(inner: shared, name: "h1");
    var h2 = h1;
    h2.inner = Ꮡ(new inner(n: 8, tag: "other"));
    fmt.Println("ptr h1:", h1.n, h1.tag, h1.name);
    fmt.Println("ptr h2:", h2.n, h2.tag, h2.name);

    var h3 = h1;
    h3.n = 70;
    fmt.Println("ptr shared h1:", h1.n);
    fmt.Println("ptr shared h3:", h3.n);

    var arr = new mid[]{new(inner: new inner(n: 11, tag: "e0"), extra: 1), new(inner: new inner(n: 12, tag: "e1"), extra: 2)}.slice();
    var e = arr[0];
    e.n = 111;
    e.tag = "elem";
    fmt.Println("slice arr[0]:", arr[0].n, arr[0].tag);
    fmt.Println("slice copy:", e.n, e.tag);
}

} // end main_package

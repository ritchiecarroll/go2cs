namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint n;
    internal @string tag;
}

partial struct mid {
    internal partial ref inner inner { get; }
    internal nint extra;
}

partial struct deep {
    internal partial ref mid mid { get; }
    internal @string label;
}

partial struct ptrHolder {
    internal partial ref ж<inner> inner { get; }
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string byValueˢ = "byValue"u8;

internal static mid byValue(mid m) {
    m.n = 99;
    m.tag = byValueˢ;
    m.extra = 90;
    return m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string derefˢ = "deref"u8;
private static readonly @string copyˢ = "copy"u8;

internal static deep derefCopy(ref deep p) {
    var c = p;
    c.n = 55;
    c.tag = derefˢ;
    c.label = copyˢ;
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assignAˢ = (@string)"assign a:"u8;
private static readonly object assignBˢ = (@string)"assign b:"u8;
private static readonly object callSrcˢ = (@string)"call src:"u8;
private static readonly object callGotˢ = (@string)"call got:"u8;
private static readonly object derefDˢ = (@string)"deref d:"u8;
private static readonly object derefCˢ = (@string)"deref c:"u8;
private static readonly object ptrH1ˢ = (@string)"ptr h1:"u8;
private static readonly object ptrH2ˢ = (@string)"ptr h2:"u8;
private static readonly object ptrSharedH1ˢ = (@string)"ptr shared h1:"u8;
private static readonly object ptrSharedH3ˢ = (@string)"ptr shared h3:"u8;
private static readonly @string elemˢ = "elem"u8;
private static readonly object sliceArr0ˢ = (@string)"slice arr[0]:"u8;
private static readonly object sliceCopyˢ = (@string)"slice copy:"u8;

internal static void Main() {
    var a = new mid(inner: new inner(n: 1, tag: "a"u8), extra: 10);
    var b = a;
    b.n = 2;
    b.tag = "b"u8;
    b.extra = 20;
    fmt.Println(assignAˢ, a.n, a.tag, a.extra);
    fmt.Println(assignBˢ, b.n, b.tag, b.extra);
    var src = new mid(inner: new inner(n: 3, tag: "src"u8), extra: 30);
    var got = byValue(src);
    fmt.Println(callSrcˢ, src.n, src.tag, src.extra);
    fmt.Println(callGotˢ, got.n, got.tag, got.extra);
    var d = Ꮡ(new deep(mid: new mid(inner: new inner(n: 4, tag: "d"u8), extra: 40), label: "orig"u8));
    var c = derefCopy(ref (d).DerefOrNull());
    fmt.Println(derefDˢ, (~d).n, (~d).tag, (~d).label);
    fmt.Println(derefCˢ, c.n, c.tag, c.label);
    var shared = Ꮡ(new inner(n: 7, tag: "shared"u8));
    var h1 = new ptrHolder(inner: shared, name: "h1"u8);
    var h2 = h1;
    h2.inner = Ꮡ(new inner(n: 8, tag: "other"u8));
    fmt.Println(ptrH1ˢ, h1.n, h1.tag, h1.name);
    fmt.Println(ptrH2ˢ, h2.n, h2.tag, h2.name);
    var h3 = h1;
    h3.n = 70;
    fmt.Println(ptrSharedH1ˢ, h1.n);
    fmt.Println(ptrSharedH3ˢ, h3.n);
    var arr = new mid[]{new(inner: new inner(n: 11, tag: "e0"u8), extra: 1), new(inner: new inner(n: 12, tag: "e1"u8), extra: 2)}.slice();
    var e = arr[0];
    e.n = 111;
    e.tag = elemˢ;
    fmt.Println(sliceArr0ˢ, arr[0].n, arr[0].tag);
    fmt.Println(sliceCopyˢ, e.n, e.tag);
}

} // end main_package

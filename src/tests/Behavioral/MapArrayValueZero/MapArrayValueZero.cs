namespace go;

using fmt = fmt_package;

partial class main_package {

internal static map<@string, array<rune>> entity2 = new map<@string, array<rune>>{
    ["NotEqualTilde;"] = new rune[]{(rune)'≂', (rune)'̸'}.array()
};

internal static void mapMiss() {
    var x = entity2["notthere", () => new array<rune>(2)].Clone();
    fmt.Println("miss:", len(x), x[0], x[1]);

    var y = entity2["NotEqualTilde;", () => new array<rune>(2)].Clone();
    fmt.Println("hit:", len(y), y[0], y[1]);

    var (z, ok) = entity2["alsomissing", () => new array<rune>(2), ꟷ];
    fmt.Println("commaok:", ok, len(z), z[0], z[1]);

    var (w, ok2) = entity2["NotEqualTilde;", () => new array<rune>(2), ꟷ];
    fmt.Println("commaok hit:", ok2, len(w), w[0], w[1]);
}

internal static map<@string, array<array<nint>>> nested = new map<@string, array<array<nint>>>{["a"] = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()};

internal static void mapMissNested() {
    var n = nested["zzz", () => new array<array<nint>>(2, () => new(3))].Clone();
    fmt.Println("nested miss:", len(n), len(n[0]), len(n[1]), n[0][2], n[1][0]);

    var h = nested["a", () => new array<array<nint>>(2, () => new(3))].Clone();
    fmt.Println("nested hit:", len(h), len(h[0]), h[0][2], h[1][0]);
}

partial struct quadMap /*map[nint, array<byte>]*/;

internal static void namedMap() {
    quadMap nilMap = default!;
    var v = nilMap[7, () => new array<byte>(4)].Clone();
    fmt.Println("nil named:", len(v), v[0], v[3]);

    var empty = new quadMap(0);
    var e = empty[9, () => new array<byte>(4)].Clone();
    fmt.Println("empty named:", len(e), e[0], e[3]);

    empty[9] = new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'d'}.array();
    var (got, ok) = empty[9, () => new array<byte>(4), ꟷ];
    fmt.Println("named hit:", ok, len(got), got[0], got[3]);
}

internal static void plainNilAndEmpty() {
    map<@string, array<rune>> nilMap = default!;
    var a = nilMap["x", () => new array<rune>(2)].Clone();
    fmt.Println("nil plain:", len(a), a[0], a[1]);

    var emptyMap = new map<@string, array<rune>>{};
    var b = emptyMap["x", () => new array<rune>(2)].Clone();
    fmt.Println("empty plain:", len(b), b[0], b[1]);
}

internal static void storeThenRead() {
    var m = new map<@string, array<rune>>();
    m["k"] = new rune[]{(rune)'q', (rune)'r'}.array();
    fmt.Println("stored:", len(m["k", () => new array<rune>(2)]), m["k", () => new array<rune>(2)][0], m["k", () => new array<rune>(2)][1]);
    fmt.Println("other:", len(m["other", () => new array<rune>(2)]), m["other", () => new array<rune>(2)][0]);
}

internal static void Main() {
    mapMiss();
    mapMissNested();
    namedMap();
    plainNilAndEmpty();
    storeThenRead();
}

} // end main_package

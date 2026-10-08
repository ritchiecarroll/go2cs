namespace go;

using fmt = fmt_package;

partial class main_package {

internal static map<@string, array<rune>> entity2 = new map<@string, array<rune>>{
    ["NotEqualTilde;"u8] = new rune[]{(rune)'≂', (rune)'̸'}.array()
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notthereˢ = "notthere"u8;
private static readonly object missˢ = (@string)"miss:"u8;
private static readonly @string notEqualTildeˢ = "NotEqualTilde;"u8;
private static readonly object hitˢ = (@string)"hit:"u8;
private static readonly @string alsomissingˢ = "alsomissing"u8;
private static readonly object commaokˢ = (@string)"commaok:"u8;
private static readonly object commaokHitˢ = (@string)"commaok hit:"u8;

internal static void mapMiss() {
    var x = entity2[notthereˢ, () => new array<rune>(2)].Clone();
    fmt.Println(missˢ, len(x), x[0], x[1]);
    var y = entity2[notEqualTildeˢ, () => new array<rune>(2)].Clone();
    fmt.Println(hitˢ, len(y), y[0], y[1]);
    var (z, ok) = entity2[alsomissingˢ, () => new array<rune>(2), ꟷ];
    fmt.Println(commaokˢ, ok, len(z), z[0], z[1]);
    var (w, ok2) = entity2[notEqualTildeˢ, () => new array<rune>(2), ꟷ];
    fmt.Println(commaokHitˢ, ok2, len(w), w[0], w[1]);
}

internal static map<@string, array<array<nint>>> nested = new map<@string, array<array<nint>>>{["a"u8] = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zzzˢ = "zzz"u8;
private static readonly object nestedMissˢ = (@string)"nested miss:"u8;
private static readonly object nestedHitˢ = (@string)"nested hit:"u8;

internal static void mapMissNested() {
    var n = nested[zzzˢ, () => new array<array<nint>>(2, () => new(3))].Clone();
    fmt.Println(nestedMissˢ, len(n), len(n[0]), len(n[1]), n[0][2], n[1][0]);
    var h = nested["a"u8, () => new array<array<nint>>(2, () => new(3))].Clone();
    fmt.Println(nestedHitˢ, len(h), len(h[0]), h[0][2], h[1][0]);
}

partial struct quadMap /*map[nint, array<byte>]*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilNamedˢ = (@string)"nil named:"u8;
private static readonly object emptyNamedˢ = (@string)"empty named:"u8;
private static readonly object namedHitˢ = (@string)"named hit:"u8;

internal static void namedMap() {
    quadMap nilMap = default!;
    var v = nilMap[7, () => new array<byte>(4)].Clone();
    fmt.Println(nilNamedˢ, len(v), v[0], v[3]);
    var empty = new quadMap(0);
    var e = empty[9, () => new array<byte>(4)].Clone();
    fmt.Println(emptyNamedˢ, len(e), e[0], e[3]);
    empty[9] = new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'d'}.array();
    var (got, ok) = empty[9, () => new array<byte>(4), ꟷ];
    fmt.Println(namedHitˢ, ok, len(got), got[0], got[3]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilPlainˢ = (@string)"nil plain:"u8;
private static readonly object emptyPlainˢ = (@string)"empty plain:"u8;

internal static void plainNilAndEmpty() {
    map<@string, array<rune>> nilMap = default!;
    var a = nilMap["x"u8, () => new array<rune>(2)].Clone();
    fmt.Println(nilPlainˢ, len(a), a[0], a[1]);
    var emptyMap = new map<@string, array<rune>>{};
    var b = emptyMap["x"u8, () => new array<rune>(2)].Clone();
    fmt.Println(emptyPlainˢ, len(b), b[0], b[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object storedˢ = (@string)"stored:"u8;
private static readonly object otherˢ = (@string)"other:"u8;
private static readonly @string otherˢ2 = "other"u8;

internal static void storeThenRead() {
    var m = new map<@string, array<rune>>();
    m["k"u8] = new rune[]{(rune)'q', (rune)'r'}.array();
    fmt.Println(storedˢ, len(m["k"u8, () => new array<rune>(2)]), m["k"u8, () => new array<rune>(2)][0], m["k"u8, () => new array<rune>(2)][1]);
    fmt.Println(otherˢ, len(m[otherˢ2, () => new array<rune>(2)]), m[otherˢ2, () => new array<rune>(2)][0]);
}

internal static void Main() {
    mapMiss();
    mapMissNested();
    namedMap();
    plainNilAndEmpty();
    storeThenRead();
}

} // end main_package

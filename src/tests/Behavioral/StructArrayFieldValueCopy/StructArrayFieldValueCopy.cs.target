namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct digest {
    internal array<nint> h = new(4);
    internal array<byte> x = new(2);
    internal nint nx;
}

[GoRecv] internal static void bump(this ref digest d) {
    foreach (var (i, _) in d.h) {
        d.h[i]++;
    }
    d.x[0]++;
    d.nx++;
}

internal static @string show(this digest d) {
    d = d.ΔClone();

    return fmt.Sprintf("%v %v %d"u8, d.h, d.x, d.nx);
}

[GoType] partial struct nest {
    internal digest d;
    internal nint tag;
}

internal static @string takeValue(digest c) {
    c = c.ΔClone();

    c.bump();
    return c.show();
}

internal static digest returnField(nest n) {
    n = n.ΔClone();

    return n.d.ΔClone();
}

internal static digest newDigest() {
    return new digest(h: new nint[]{1, 2, 3, 4}.array(), x: new byte[]{5, 6}.array(), nx: 7);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object derefˢ = (@string)"deref:      "u8;

internal static void derefCopy() {
    ref var d = ref heap<digest>(out var Ꮡd);
    d = newDigest();
    var p = Ꮡd;
    var d0 = p.Value.ΔClone();
    d0.bump();
    fmt.Println(derefˢ, (~p).show(), (@string)"|"u8, d0.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object identˢ = (@string)"ident:      "u8;

internal static void identCopy() {
    var d = newDigest();
    var c = d.ΔClone();
    c.bump();
    fmt.Println(identˢ, d.show(), (@string)"|"u8, c.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object selectorˢ = (@string)"selector:   "u8;

internal static void selectorCopy() {
    var n = new nest(d: newDigest(), tag: 1);
    var e = n.d.ΔClone();
    e.bump();
    fmt.Println(selectorˢ, n.d.show(), (@string)"|"u8, e.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object compositeˢ = (@string)"composite:  "u8;

internal static void compositeElement() {
    var d = newDigest();
    var n = new nest(d: d.ΔClone(), tag: 2);
    n.d.bump();
    fmt.Println(compositeˢ, d.show(), (@string)"|"u8, n.d.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nestedˢ = (@string)"nested:     "u8;

internal static void nestedStructCopy() {
    var n = new nest(d: newDigest(), tag: 3);
    var n2 = n.ΔClone();
    n2.d.bump();
    n2.tag = 4;
    fmt.Println(nestedˢ, n.d.show(), n.tag, (@string)"|"u8, n2.d.show(), n2.tag);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object paramˢ = (@string)"param:      "u8;

internal static void paramByValue() {
    var d = newDigest();
    @string got = takeValue(d);
    fmt.Println(paramˢ, d.show(), (@string)"|"u8, got);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object returnˢ = (@string)"return:     "u8;

internal static void returnValue() {
    var n = new nest(d: newDigest(), tag: 5);
    var r = returnField(n);
    r.bump();
    fmt.Println(returnˢ, n.d.show(), (@string)"|"u8, r.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object indexˢ = (@string)"index:      "u8;
private static readonly object sliceIndexˢ = (@string)"sliceIndex: "u8;

internal static void indexCopy() {
    var d = newDigest();
    var arr = new digest[]{d.ΔClone(), d.ΔClone()}.array();
    var f = arr[0].ΔClone();
    f.bump();
    fmt.Println(indexˢ, arr[0].show(), (@string)"|"u8, f.show());
    var s = new digest[]{d.ΔClone()}.slice();
    var g = s[0].ΔClone();
    g.bump();
    fmt.Println(sliceIndexˢ, s[0].show(), (@string)"|"u8, g.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeˢ = (@string)"range:      "u8;

internal static void rangeCopy() {
    var d = newDigest();
    var arr = new digest[]{d.ΔClone(), d.ΔClone()}.array();
    foreach (var (_, vᴛ1) in arr.ΔRangeSnapshot()) {
        var g = vᴛ1.ΔClone();

        g.bump();
    }
    fmt.Println(rangeˢ, arr[0].show(), arr[1].show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mapˢ = (@string)"map:        "u8;

internal static void mapValueCopy() {
    var m = new map<@string, digest>{["k"u8] = newDigest()};
    var mv = m["k"u8].ΔClone();
    mv.bump();
    var stored = m["k"u8].ΔClone();
    fmt.Println(mapˢ, stored.show(), (@string)"|"u8, mv.show());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object receiverˢ = (@string)"receiver:   "u8;

internal static void valueReceiver() {
    var d = newDigest();
    @string before = d.show();
    @string after = d.show();
    fmt.Println(receiverˢ, before, (@string)"|"u8, after);
}

internal static void Main() {
    derefCopy();
    identCopy();
    selectorCopy();
    compositeElement();
    nestedStructCopy();
    paramByValue();
    returnValue();
    indexCopy();
    rangeCopy();
    mapValueCopy();
    valueReceiver();
}

} // end main_package

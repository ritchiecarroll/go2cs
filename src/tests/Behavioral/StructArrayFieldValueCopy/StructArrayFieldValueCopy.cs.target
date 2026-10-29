namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct digest {
    internal array<nint> h = new(4);
    internal array<byte> x = new(2);
    internal nint nx;
}

internal static void bump(this ref digest d) {
    foreach (var i in range(d.h)) {
        d.h[i]++;
    }
    d.x[0]++;
    d.nx++;
}

internal static @string show(this digest d) {
    d = d.ΔClone();

    return fmt.Sprintf("%v %v %d"u8, d.h, d.x, d.nx);
}

partial struct nest {
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

internal static void derefCopy() {
    ref var d = ref heap<digest>(out var Ꮡd);
    d = newDigest();
    var p = Ꮡd;
    var d0 = p.Value.ΔClone();
    d0.bump();
    fmt.Println("deref:      ", (~p).show(), "|", d0.show());
}

internal static void identCopy() {
    var d = newDigest();
    var c = d.ΔClone();
    c.bump();
    fmt.Println("ident:      ", d.show(), "|", c.show());
}

internal static void selectorCopy() {
    var n = new nest(d: newDigest(), tag: 1);
    var e = n.d.ΔClone();
    e.bump();
    fmt.Println("selector:   ", n.d.show(), "|", e.show());
}

internal static void compositeElement() {
    var d = newDigest();
    var n = new nest(d: d.ΔClone(), tag: 2);
    n.d.bump();
    fmt.Println("composite:  ", d.show(), "|", n.d.show());
}

internal static void nestedStructCopy() {
    var n = new nest(d: newDigest(), tag: 3);
    var n2 = n.ΔClone();
    n2.d.bump();
    n2.tag = 4;
    fmt.Println("nested:     ", n.d.show(), n.tag, "|", n2.d.show(), n2.tag);
}

internal static void paramByValue() {
    var d = newDigest();
    @string got = takeValue(d);
    fmt.Println("param:      ", d.show(), "|", got);
}

internal static void returnValue() {
    var n = new nest(d: newDigest(), tag: 5);
    var r = returnField(n);
    r.bump();
    fmt.Println("return:     ", n.d.show(), "|", r.show());
}

internal static void indexCopy() {
    var d = newDigest();
    var arr = new digest[]{d.ΔClone(), d.ΔClone()}.array();
    var f = arr[0].ΔClone();
    f.bump();
    fmt.Println("index:      ", arr[0].show(), "|", f.show());

    var s = slice<digest>([d.ΔClone()]);
    var g = s[0].ΔClone();
    g.bump();
    fmt.Println("sliceIndex: ", s[0].show(), "|", g.show());
}

internal static void rangeCopy() {
    var d = newDigest();
    var arr = new digest[]{d.ΔClone(), d.ΔClone()}.array();

    foreach (var (_, vᴛ1) in arr.ΔRangeSnapshot()) {
        var g = vᴛ1.ΔClone();

        g.bump();
    }

    fmt.Println("range:      ", arr[0].show(), arr[1].show());
}

internal static void mapValueCopy() {
    var m = new map<@string, digest>{["k"] = newDigest()};
    var mv = m["k"].ΔClone();
    mv.bump();
    var stored = m["k"].ΔClone();
    fmt.Println("map:        ", stored.show(), "|", mv.show());
}

internal static void valueReceiver() {
    var d = newDigest();
    @string before = d.show();
    @string after = d.show();
    fmt.Println("receiver:   ", before, "|", after);
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

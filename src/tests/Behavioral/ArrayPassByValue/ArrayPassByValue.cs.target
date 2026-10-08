namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "Hello"u8;
private static readonly @string worldˢ = "World"u8;

internal static void Main() {
    ref var a = ref heap(new array<@string>(2), out var Ꮡa);
    a[0] = helloˢ;
    a[1] = worldˢ;
    var p = Ꮡa.at<@string>(0);
    test(a);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    a[0] = helloˢ;
    test2(ref a);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    a[0] = helloˢ;
    test3(a[..]);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
    fmt.Println(primes);
    fmt.Println(a[0]);
    stest(ref (p).DerefOrNull());
    fmt.Println(a[0]);
    assignCopies();
}

internal static array<nint> garr = new nint[]{1, 2, 3}.array();

partial struct arrHolder {
    internal array<nint> arr = new(3);
}

internal static void assignCopies() {
    ref var src = ref heap<array<nint>>(out var Ꮡsrc);
    src = new nint[]{1, 2, 3}.array();
    var cpy = src.Clone();
    cpy[0] = 99;
    fmt.Println(src[0], cpy[0]);
    var d = garr.Clone();
    d[0] = 77;
    fmt.Println(garr[0], d[0]);
    array<nint> e = garr.Clone();
    e[1] = 88;
    fmt.Println(garr[1], e[1]);
    e = src.Clone();
    e[2] = 66;
    fmt.Println(src[2], e[2]);
    var h = new arrHolder(arr: new nint[]{4, 5, 6}.array());
    var f = h.arr.Clone();
    f[0] = 55;
    fmt.Println(h.arr[0], f[0]);
    h.arr = src.Clone();
    h.arr[1] = 44;
    fmt.Println(src[1], h.arr[1]);
    var m = new array<nint>[]{new nint[]{7, 8, 9}.array(), new nint[]{10, 11, 12}.array()}.array();
    var row = m[1].Clone();
    row[0] = 33;
    fmt.Println(m[1][0], row[0]);
    var q = Ꮡsrc;
    var g = q.Value.Clone();
    g[0] = 22;
    fmt.Println(src[0], g[0]);
    var (x, y) = (new nint[]{1, 2}.array(), new nint[]{3, 4}.array());
    (x, y) = (y.Clone(), x.Clone());
    x[0] = 11;
    fmt.Println(x[0], x[1], y[0], y[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ2 = "hello"u8;

internal static void stest(ref @string p) {
    p = helloˢ2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goodbyeˢ = "Goodbye"u8;

internal static void test(/*[2]*/ array<@string> a) {
    a = a.Clone();

    fmt.Println(a[0], a[1]);
    a[0] = goodbyeˢ;
    fmt.Println(a[0], a[1]);
}

internal static void test2(ref array<@string> a) {
    fmt.Println(a[0], a[1]);
    a[0] = goodbyeˢ;
    fmt.Println(a[0], a[1]);
}

internal static void test3(slice<@string> a) {
    fmt.Println(a[0], a[1]);
    a[0] = goodbyeˢ;
    fmt.Println(a[0], a[1]);
}

} // end main_package

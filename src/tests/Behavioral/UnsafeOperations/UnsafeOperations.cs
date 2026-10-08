namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct T1 {
    internal int32 a;
}

partial struct T2 {
    internal int32 a;
}

partial struct Inner {
    internal int32 p;
    internal int64 q;
}

partial struct Outer {
    internal byte head;
    internal Inner @in;
}

internal static ж<Outer> ᏑgOuter = new StandardBox<Outer>(default(Outer));
internal static ref Outer gOuter => ref ᏑgOuter.Value;

partial struct Padded {
    internal bool flag;
    internal int64 count;
    internal byte tag;
    internal @string name;
    internal slice<byte> data;
    internal int32 code;
}

partial struct Embedded {
    internal byte lead;
    public partial ref Padded Padded { get; }
    internal int16 trail;
}

partial struct Arrays {
    internal int16 head;
    internal array<int32> cells = new(5);
    internal byte tail;
}

public static uint64 Float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}

public static float64 Float64frombits(uint64 b) {
    return bitcast<uint64, float64>(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueOfTheNextElementˢ = (@string)"Value of the next element:"u8;
private static readonly object valueOfT2Aˢ = (@string)"Value of t2.a:"u8;

internal partial struct main_x /*dyn*/ {
    internal int64 a;
    internal bool b;
    internal @string c;
}

internal static void Main() {
    var b = new byte[]{}.slice();
    for (nint ch = 32; ch < 80; ch++) {
        b = append(b, ((@string)(rune)ch).ꓸꓸꓸ);
    }
    @string str = @unsafe.String(Ꮡ(b, 0), len(b));
    fmt.Println(str);
    var strptr = @unsafe.StringData(str);
    fmt.Println(@unsafe.String(strptr, len(str)));
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3, 4}.array();
    var arrptr = Ꮡarr.at<nint>(0);
    @unsafe.Pointer nextPtr = (@unsafe.Pointer)((uintptr)arrptr + /* unsafe.Sizeof(arr[0]) */ (uintptr)8);
    fmt.Println(valueOfTheNextElementˢ, ~(ж<nint>)(uintptr)(nextPtr));
    ref var t1 = ref heap(new T1(), out var Ꮡt1);
    t1.a = 42;
    var t2 = ~Ꮡt1.Reinterpret<T1, T2>();
    fmt.Println(valueOfT2Aˢ, t2.a);
    int8 i = -1;
    int16 j = (int16)i;
    fmt.Println(i, j);
    uint8 k = bitcast<int8, uint8>(i);
    fmt.Println(k);
    main_x x = default!;
    uintptr M = /* unsafe.Sizeof(x.c) */ 16;
    uintptr N = /* unsafe.Sizeof(x) */ 32;
    fmt.Println(M, N);
    fmt.Println(/* unsafe.Alignof(x.a) */ (uintptr)8);
    fmt.Println(/* unsafe.Alignof(x.b) */ (uintptr)1);
    fmt.Println(/* unsafe.Alignof(x.c) */ (uintptr)8);
    fmt.Println(/* unsafe.Offsetof(x.a) */ (uintptr)0);
    fmt.Println(/* unsafe.Offsetof(x.b) */ (uintptr)8);
    fmt.Println(/* unsafe.Offsetof(x.c) */ (uintptr)16);
    fmt.Println(/* unsafe.Alignof(uint32(0)) */ (uintptr)4);
    fmt.Println(/* unsafe.Alignof(float64(0)) */ (uintptr)8);
    fmt.Println(/* unsafe.Alignof(arr[0]) */ (uintptr)8);
    var op = ᏑgOuter;
    fmt.Println(/* unsafe.Alignof(op.in.q) */ (uintptr)8);
    fmt.Println(/* unsafe.Offsetof(gOuter.in.q) */ (uintptr)8);
    fmt.Println(/* unsafe.Offsetof(op.in) */ (uintptr)8);
    Padded p = default!;
    Embedded e = new(nil);
    Arrays a = new();
    fmt.Println(/* unsafe.Sizeof(p) */ (uintptr)72, /* unsafe.Alignof(p) */ (uintptr)8, /* unsafe.Offsetof(p.name) */ (uintptr)24);
    fmt.Println(/* unsafe.Sizeof(p) */ (uintptr)72 + /* unsafe.Sizeof(a) */ (uintptr)28, /* unsafe.Offsetof(p.code) */ (uintptr)64 - /* unsafe.Offsetof(p.count) */ (uintptr)8);
    fmt.Println(/* unsafe.Sizeof(a.cells) */ (uintptr)20 / /* unsafe.Sizeof(a.cells[0]) */ (uintptr)4);
    fmt.Println(/* unsafe.Sizeof(p.name) */ (uintptr)16 * 2 + /* unsafe.Alignof(p.count) */ (uintptr)8);
    fmt.Println(/* unsafe.Sizeof(p) */ (uintptr)72 > /* unsafe.Sizeof(a) */ (uintptr)28, /* unsafe.Alignof(p.flag) */ (uintptr)1 == /* unsafe.Alignof(p.tag) */ (uintptr)1);
    var sz = /* unsafe.Sizeof(e) */ (uintptr)88;
    sz += /* unsafe.Offsetof(e.trail) */ (uintptr)80;
    fmt.Println(sz);
    var buf = new slice<byte>((nint)(/* unsafe.Sizeof(p) */ (uintptr)72));
    fmt.Println(len(buf), cap(buf));
    fmt.Println(/* unsafe.Sizeof(e) */ (uintptr)88, /* unsafe.Offsetof(e.Padded) */ (uintptr)8, /* unsafe.Offsetof(e.trail) */ (uintptr)80, /* unsafe.Offsetof(e.count) */ (uintptr)16);
    fmt.Println(/* unsafe.Sizeof(a) */ (uintptr)28, /* unsafe.Alignof(a.cells) */ (uintptr)4, /* unsafe.Offsetof(a.cells) */ (uintptr)4, /* unsafe.Offsetof(a.tail) */ (uintptr)24);
    var i2 = Float64bits(9.5D);
    var f2 = Float64frombits(i2);
    fmt.Println(i2);
    fmt.Println(f2);
}

} // end main_package

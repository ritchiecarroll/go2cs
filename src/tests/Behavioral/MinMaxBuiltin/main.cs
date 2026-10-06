namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt limit => /* 128 << 10 */ 131072;

internal static UntypedInt floor => 16;

internal static partial uintptr clampU(uintptr n) {
    return min(n, (uintptr)(limit));
}

internal static partial int32 clampI(int32 d) {
    return max(d, (int32)(floor));
}

[GoType("num:uint16")] partial struct fieldElement;

[GoType("num:float64")] partial struct ratio;

[GoType("num:int8")] partial struct delta;

internal static partial fieldElement spread(fieldElement a, fieldElement b) {
    return min((fieldElement)(a - b), (fieldElement)(b - a), (fieldElement)(a - b + 3329), (fieldElement)(b - a + 3329));
}

internal static void Main() {
    fmt.Println(min((nint)(3), (nint)(7)));
    fmt.Println(max((nint)(3), (nint)(7)));
    fmt.Println(min((nint)(5), (nint)(2), (nint)(9), (nint)(1), (nint)(4)));
    fmt.Println(max((nint)(5), (nint)(2), (nint)(9), (nint)(1), (nint)(4)));
    fmt.Println(min((nint)(42)));
    fmt.Println(min(2.5D, 1.5D));
    fmt.Println(max(2.5D, 1.5D));
    fmt.Println(min((@string)("banana"u8), (@string)("apple"u8), (@string)("cherry"u8)));
    fmt.Println(max((@string)("banana"u8), (@string)("apple"u8), (@string)("cherry"u8)));
    var x = new byte[]{1, 2, 3}.slice();
    var y = new byte[]{1, 2, 3, 4, 5}.slice();
    nint n = min(len(x), len(y));
    fmt.Println(n);
    fmt.Println(clampU(999999), clampU(7));
    fmt.Println(clampI(3), clampI(100));
    uintptr big = 200000;
    fmt.Println(min(big, (uintptr)(limit), (uintptr)(500)));
    var (a, b, c, d) = (((fieldElement)10), ((fieldElement)3329), ((fieldElement)7), ((fieldElement)500));
    fmt.Println(min(a, b, c, d), max(a, b, c, d));
    fmt.Println(min(a, c), max(a, c));
    fmt.Println(spread(10, 3));
    var (p, q, r) = (((ratio)2.5D), ((ratio)(-1.25D)), ((ratio)8D));
    fmt.Println(min(p, q, r), max(p, q, r));
    fmt.Println(min(p, q), max(p, q));
    var (i, j, k) = (((delta)(-5)), ((delta)3), ((delta)(-100)));
    fmt.Println(min(i, j, k), max(i, j, k));
    fmt.Println(min(i, j), max(i, j));
    untypedConstTypes();
}

internal static void untypedConstTypes() {
    int8 i8 = -100;
    uint8 u8 = 200;
    int16 i16 = -300;
    uint16 u16 = 60000;
    int32 i32 = -70000;
    uint32 u32 = 4000000000U;
    int64 i64 = -5000000000L;
    uint64 u64 = 18000000000000000000UL;
    uintptr up = 4096;
    nint n = 9;
    nuint un = 11;
    float32 f32 = 0.5F;
    float64 f64 = 0.25D;
    @string s = "m"u8;
    var (fe, rt, dl) = (((fieldElement)10), ((ratio)2.5D), ((delta)(-5)));
    fmt.Printf("%T %v | %T %v\n"u8, max(i8, (int8)(1)), max(i8, (int8)(1)), min(i8, (int8)(1)), min(i8, (int8)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(u8, (uint8)(1)), max(u8, (uint8)(1)), min(u8, (uint8)(1)), min(u8, (uint8)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(i16, (int16)(1)), max(i16, (int16)(1)), min(i16, (int16)(1)), min(i16, (int16)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(u16, (uint16)(1)), max(u16, (uint16)(1)), min(u16, (uint16)(1)), min(u16, (uint16)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(i32, 1), max(i32, 1), min(i32, 1), min(i32, 1));
    fmt.Printf("%T %v | %T %v\n"u8, max(u32, (uint32)(1)), max(u32, (uint32)(1)), min(u32, (uint32)(1)), min(u32, (uint32)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(i64, 1), max(i64, 1), min(i64, 1), min(i64, 1));
    fmt.Printf("%T %v | %T %v\n"u8, max(u64, (uint64)(1)), max(u64, (uint64)(1)), min(u64, (uint64)(1)), min(u64, (uint64)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(up, (uintptr)(1)), max(up, (uintptr)(1)), min(up, (uintptr)(1)), min(up, (uintptr)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(n, 1), max(n, 1), min(un, (nuint)(1)), min(un, (nuint)(1)));
    fmt.Printf("%T %v | %T %v\n"u8, max(f32, 1F), max(f32, 1F), min(f64, 1D), min(f64, 1D));
    fmt.Printf("%T %v\n"u8, max(f32, 1F, 2.5F), max(f32, 1F, 2.5F));
    fmt.Printf("%T %v | %T %v\n"u8, min(s, (@string)("a"u8)), min(s, (@string)("a"u8)), max(s, (@string)("z"u8)), max(s, (@string)("z"u8)));
    fmt.Printf("%T %v | %T %v | %T %v\n"u8, max(fe, (fieldElement)(1)), max(fe, (fieldElement)(1)), min(rt, (ratio)(1D)), min(rt, (ratio)(1D)), max(dl, (delta)(1)), max(dl, (delta)(1)));
    fmt.Printf("%T %v | %T %v | %T %v\n"u8, max((nint)(1), (nint)(2)), max((nint)(1), (nint)(2)), max(1D, 2.5D), max(1D, 2.5D), min((rune)'a', (rune)'b'), min((rune)'a', (rune)'b'));
    var x = max(u8, (uint8)(1));
    x += 100;
    fmt.Printf("%T %v\n"u8, x, x);
}

} // end main_package

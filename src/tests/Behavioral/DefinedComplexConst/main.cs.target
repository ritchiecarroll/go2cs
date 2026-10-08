namespace go;

using fmt = fmt_package;
using ꓸꓸꓸC128 = Span<main_package.C128>;

partial class main_package {

partial struct C128 /*num:complex128*/;

partial struct C64 /*num:complex64*/;

partial struct C128b /*num:complex128*/;

partial struct F64 /*num:float64*/;

partial struct F32 /*num:float32*/;

partial struct F32b /*num:float32*/;

partial struct pair {
    internal C128 a;
    internal C64 b;
}

internal static UntypedInt intConst => 7;
internal static UntypedFloat floatConst => 1.5;
internal static UntypedComplex cplxConst => /* 2i */ 2D.i();

internal static C128 typedC128 => /* 1 */ 1D + 0D.i();
internal static C128 typedC128r => /* 0.25 */ 0.25D + 0D.i();
internal static C64 typedC64 => /* 0.25 */ 0.25F + 0F.i();
internal static C64 typedC64n => /* 3 */ 3F + 0F.i();
internal static C64 typedC64c => /* 1 + 2i */ 1F + 2F.i();
internal static C128b typedC128b => /* 0.5 */ 0.5D + 0D.i();

internal static F32 typedF32 => 0.25f;
internal static F32 typedF32n => 3f;
internal static F32 typedF32t => 0.1f;
internal static F32b typedF32b => 1.5f;
internal static F64 typedF64 => 0.1;
internal const float32 plainF32 = 0.1f;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroˢ = "zero"u8;
private static readonly @string smallˢ = "small"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string classify(C128 c) {
    var exprᴛ1 = c;
    if (exprᴛ1 == (C128)(0D)) {
        return zeroˢ;
    }
    if (exprᴛ1 == (C128)(1D) || exprᴛ1 == (C128)(2.5D)) {
        return smallˢ;
    }

    return otherˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredˢ = (@string)"deferred:"u8;

internal static void deferred() {
    GoFrame ᒐ = default;
    try {
        defer((ᴛ1, ᴛ2, ᴛ3) => fmt.Println(ᴛ1, ᴛ2, ᴛ3), deferredˢ, takeC128(4D), takeC64(0.5F), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static C128 takeC128(C128 c) {
    return c;
}

internal static C64 takeC64(C64 c) {
    return c;
}

internal static C128 takeMany(params ꓸꓸꓸC128 csʗp) {
    var cs = csʗp.sslice();

    C128 sum = default!;
    foreach (var (_, c) in cs) {
        sum += c;
    }
    return sum;
}

internal static C128 retC128() {
    return 3D;
}

internal static C64 retC64() {
    return 2.5F;
}

internal static (C128, C64) retMulti() {
    return (1D, 2F);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assignˢ = (@string)"assign:"u8;
private static readonly @string twoˢ = "two"u8;
private static readonly object mapˢ = (@string)"map:"u8;
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object compositeˢ = (@string)"composite:"u8;
private static readonly object binaryˢ = (@string)"binary:"u8;
private static readonly object namedˢ = (@string)"named:"u8;
private static readonly object conversionˢ = (@string)"conversion:"u8;
private static readonly object typedˢ = (@string)"typed:"u8;
private static readonly object channelˢ = (@string)"channel:"u8;
private static readonly object floatTypedˢ = (@string)"float typed:"u8;
private static readonly object controlsˢ = (@string)"controls:"u8;

internal static void Main() {
    C128 a = 0D;
    a = 1D;
    C64 b = 4F;
    b = 0.5F;
    C128b ab = 6D;
    fmt.Println(assignˢ, a, b, ab);
    var m = new map<C128, @string>{[0D] = "zero"u8, [1.5D] = "one-half"u8};
    m[2D] = twoˢ;
    fmt.Println(mapˢ, m[0D], m[1.5D], m[2D], len(m));
    var (x, y) = retMulti();
    fmt.Println(callˢ, takeC128(3D), takeC64(5F), takeMany(1D, 2D, 3D), retC128(), retC64(), x, y);
    var s = new C128[]{1D, 2D, 3D.i()}.slice();
    var arr = new C64[]{1F, 2F}.array();
    var p = new pair(a: 4D, b: 5F);
    var mv = new map<@string, C64>{["k"u8] = 9F};
    fmt.Println(compositeˢ, s, arr, p, mv["k"u8]);
    var c = ((C128)2D);
    c = c * 2D;
    c = 1D + c;
    c += 2D;
    c -= 0.5D;
    fmt.Println(binaryˢ, c, c == 6.5D, c == 0D, c != 0D, b * 2F, 1F - b);
    C128 ni = intConst;
    C64 nf = floatConst;
    C128 nc = cplxConst;
    C128 ci = 2D.i();
    C128 neg = -1D;
    C64 expr = 7F + 0F.i();
    fmt.Println(namedˢ, ni, nf, nc, ci, neg, expr, takeC128(intConst), takeC64(floatConst));
    fmt.Println(conversionˢ, ((C64)2F), ((C128)(-3D)), ((C128b)1.25D), ((C128)complex(0D, 0D)));
    C64 localC64 = /* 1.5 */ 1.5F + 0F.i();
    C128b localC128b = /* -2 */ -2D + 0D.i();
    F32 localF32 = -2.5f;
    F32b localF32b = 0.75f;
    F64 localF64 = 2.5;
    fmt.Println(typedˢ, typedC128, typedC128r, typedC64, typedC64n, typedC64c, typedC128b, localC64, localC128b, 0.5F + 0F.i(), 2D + 0D.i(), classify(0D), classify(2.5D), classify(3D));
    deferred();
    var ch = new channel<C64>(1);
    ch.ᐸꟷ(8F);
    fmt.Println(channelˢ, ᐸꟷ(ch));
    complex128 pc = 3D;
    complex64 pc64 = 4F;
    F64 df = 5D;
    fmt.Println(floatTypedˢ, typedF32, typedF32n, typedF32t, typedF32b, typedF64, plainF32, localF32, localF32b, localF64, typedF32 * 2F, (float32)typedF32t == plainF32);
    fmt.Println(controlsˢ, pc, pc64, df);
}

} // end main_package

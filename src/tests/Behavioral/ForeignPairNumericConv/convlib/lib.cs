namespace go.ForeignPairNumericConv;

partial class convlib_package {

partial struct Handle /*num:uintptr*/;

partial struct Token /*num:uintptr*/;

public static Token NewToken(uintptr v) {
    return ((Token)v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string handleˢ = "handle"u8;

public static @string String(this Handle h) {
    return handleˢ;
}

public static uintptr Raw(this Token t) {
    return (uintptr)t;
}

} // end convlib_package

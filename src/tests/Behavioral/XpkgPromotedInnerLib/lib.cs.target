namespace go;

using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class XpkgPromotedInnerLib_package {

[GoType] partial struct Token {
    public nint N;
}

[GoType] partial struct Inner {
    public nint X;
}

public static Token Tok(this Inner i) {
    return new Token(i.X);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ = "inner"u8;

public static @string Name(this Inner i) {
    return innerˢ;
}

public static (nint, error) Pair(this Inner i) {
    return (i.X, default!);
}

[GoRecv] public static void Set(this ref Inner i, nint v) {
    i.X = v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ2 = "Inner!"u8;

public static @string String(this Inner i) {
    return innerˢ2;
}

public static partial bool Where(this Inner i) {
    var (_, _, _, ok) = runtime.Caller(1);
    return ok;
}

} // end XpkgPromotedInnerLib_package

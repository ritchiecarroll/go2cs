namespace go;

using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class XpkgPromotedInnerLib_package {

partial struct Token {
    public nint N;
}

partial struct Inner {
    public nint X;
}

public static Token Tok(this Inner i) => new Token(i.X);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ = "inner"u8;

public static @string Name(this Inner i) => innerˢ;
public static (nint, error) Pair(this Inner i) => (i.X, default!);
public static void Set(this ref Inner i, nint v) => i.X = v;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ2 = "Inner!"u8;

public static @string String(this Inner i) => innerˢ2;

public static partial bool Where(this Inner i) {
    var (_, _, _, ok) = runtime.Caller(1);

    return ok;
}

} // end XpkgPromotedInnerLib_package

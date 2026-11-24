namespace go;

using runtime = runtime_package;

partial class XpkgPromotedInnerLib_package {

partial struct Token {
    public nint N;
}

partial struct Inner {
    public nint X;
}

internal static Token Tok⁔(/*this*/ Inner i) { return new Token(i.X); }
internal static @string Name⁔(/*this*/ Inner i) { return "inner"; }
internal static (nint, error) Pair⁔(/*this*/ Inner i) { return (i.X, default); }
internal static void Set⁔(/*this*/ ref Inner i, nint v) { i.X = v; }
internal static @string String⁔(/*this*/ Inner i) { return "Inner!"; }

internal static partial bool Where⁔(/*this*/ Inner i) {
    var (_, _, _, ok) = runtime.Caller(1);

    return ok;
}

} // end XpkgPromotedInnerLib_package

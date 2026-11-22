namespace go.GlyphLibraryFrameNames;

using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class framelib_package {

partial struct Counter {
    internal nint n;
}

public static partial @string Here() {
    var (pc, _, _, _) = runtime.Caller(1);
    return runtime.FuncForPC(pc).Name();
}

internal static partial @string Name⁔(/*this*/ Counter c) {
    return Here();
}

internal static @string Closure⁔(/*this*/ Counter c) {
    [MethodImpl(MethodImplOptions.NoInlining)] @string f() => Here();

    return f();
}

internal static @string Bump⁔(/*this*/ ref Counter c) {
    c.n++;

    [MethodImpl(MethodImplOptions.NoInlining)] @string f() => Here();

    return f();
}

} // end framelib_package

namespace go.GlyphLibraryFrameNames;

using runtime = runtime_package;

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

internal static partial @string Bump⁔(/*this*/ ref Counter c) {
    c.n++;

    return Here();
}

} // end framelib_package

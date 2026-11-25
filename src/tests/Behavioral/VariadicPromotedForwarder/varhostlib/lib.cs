namespace go.go;

using fmt = fmt_package;
using ꓸꓸꓸany = variadic<any>;

partial class varhost_package {

partial interface Host {
    @string Name();
    @string Trace(@string format, params ꓸꓸꓸany argsʗp);
}

partial struct Base {
    public @string Tag;
}

internal static @string Name⁔(/*this*/ ref Base b) { return "base:"u8 + b.Tag; }
internal static @string Trace⁔(/*this*/ ref Base b, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp;

    var evalᴛ1 = fmt.Sprintf(format, args.ꓸꓸꓸ);
    return "trace["u8 + b.Tag + "] "u8 + evalᴛ1;
}

public static @string Use(Host h) {
    return h.Name() + " | "u8 + h.Trace("%s=%d", "n", (nint)2) + " | "u8 + h.Trace("plain");
}

} // end varhost_package

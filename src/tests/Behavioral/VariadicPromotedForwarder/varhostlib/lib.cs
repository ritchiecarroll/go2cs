namespace go.go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using static varhost_package;

partial class varhost_package {

partial interface Host {
    @string Name();
    @string Trace(@string format, params ꓸꓸꓸany argsʗp);
}

partial struct Base {
    public @string Tag;
}

} partial class varhostᴛBase {

public static @string Name(this ref Base b) { return "base:"u8 + b.Tag; }
public static @string Trace(this ref Base b, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return "trace["u8 + b.Tag + "] "u8 + fmt.Sprintf(format, args.ꓸꓸꓸ);
}

} partial class varhost_package {

public static @string Use(Host h) {
    return h.Name() + " | "u8 + h.Trace("%s=%d", "n", (nint)2) + " | "u8 + h.Trace("plain");
}

} // end varhost_package

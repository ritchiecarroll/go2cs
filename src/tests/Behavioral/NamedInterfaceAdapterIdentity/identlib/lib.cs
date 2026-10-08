namespace go.NamedInterfaceAdapterIdentity;

using fmt = fmt_package;

partial class identlib_package {

partial interface Greeter {
    @string Greet();
}

public static (Greeter, bool) TryGreet(any v) {
    var (g, ok) = v._<Greeter>(ᐧ);
    return (g, ok);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string otherˢ = "other"u8;
private static readonly @string stringˢ = "string"u8;
private static readonly @string greeterˢ = "greeter"u8;

public static @string Describe(Greeter g, any original) {
    any v = g;
    var (_, again) = v._<Greeter>(ᐧ);
    @string kind = otherˢ;
    switch (v.type()) {
    case @string: {
        kind = stringˢ;
        break;
    }
    case {} ᴛ0 when ᴛ0._<Greeter>(out var _): {
        kind = greeterˢ;
        break;
    }}

    return fmt.Sprintf("%T again=%v kind=%s equal=%v text=%s"u8, v, again, kind, AreEqual(v, original), g.Greet());
}

} // end identlib_package

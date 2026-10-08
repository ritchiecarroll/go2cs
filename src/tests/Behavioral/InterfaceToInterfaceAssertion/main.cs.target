namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Stringish {
    @string Str();
}

partial interface Marshaler {
    @string Marshal();
}

partial struct widget {
    internal nint n;
}

internal static @string Str(this ref widget w) {
    return fmt.Sprintf("widget(%d)"u8, w.n);
}

internal static @string Marshal(this ref widget w) {
    return fmt.Sprintf("<%d>"u8, w.n);
}

partial struct other {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string otherˢ = "other"u8;

internal static @string Str(this ref other o) {
    return otherˢ;
}

internal static Stringish newStringish(nint n) {
    return new widgetжStringish(Ꮡ(new widget(n)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unexpectedOtherIsˢ = (@string)"unexpected: other is Marshaler"u8;
private static readonly object otherIsNotMarshalerˢ = (@string)"other is not Marshaler"u8;

internal static void Main() {
    var s = newStringish(7);
    fmt.Println(s.Str());
    var m = s._<Marshaler>();
    fmt.Println(m.Marshal());
    {
        var (m2, ok) = s._<Marshaler>(ᐧ); if (ok) {
            fmt.Println((@string)"ok"u8, m2.Marshal());
        }
    }
    Stringish s2 = new otherжStringish(Ꮡ(new other(nil)));
    {
        var (_, ok) = s2._<Marshaler>(ᐧ); if (ok){
            fmt.Println(unexpectedOtherIsˢ);
        } else {
            fmt.Println(otherIsNotMarshalerˢ);
        }
    }
}

} // end main_package

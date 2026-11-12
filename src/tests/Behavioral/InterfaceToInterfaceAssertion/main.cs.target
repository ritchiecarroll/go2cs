namespace go;

using fmt = fmt_package;
using static main_package;

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

} partial class mainᴛwidget {

internal static @string Str(this ref widget w) { return fmt.Sprintf("widget(%d)"u8, w.n); }
internal static @string Marshal(this ref widget w) { return fmt.Sprintf("<%d>"u8, w.n); }

} partial class main_package {

partial struct other {
}

} partial class mainᴛother {

internal static @string Str(this ref other o) { return "other"; }

} partial class main_package {

internal static Stringish newStringish(nint n) { return new widgetжStringish(Ꮡ(new widget(n))); }

internal static void main() {
    var s = newStringish(7);
    fmt.Println(s.Str());

    var m = s._<Marshaler>();
    fmt.Println(m.Marshal());

    if (s._<Marshaler>(out var m2)) {
        fmt.Println("ok", m2.Marshal());
    }

    Stringish s2 = new otherжStringish(Ꮡ(new other()));
    if (s2._<Marshaler>(out _)) {
        fmt.Println("unexpected: other is Marshaler");
    } else {
        fmt.Println("other is not Marshaler");
    }
}

} // end main_package

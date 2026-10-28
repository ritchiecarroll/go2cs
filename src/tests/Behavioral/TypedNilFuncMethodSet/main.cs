namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate nint Fn();

public static @string String(this Fn fn) {
    return "String(fn)";
}

partial interface Getter {
    nint Get();
}

public delegate nint G();

public static nint Get(this G g) {
    return 7;
}

partial interface Both :
    fmt.Stringer
{
    nint Get();
}

internal static void Main() {
    Fn fn = default!;

    fmt.Printf("%v\n"u8, (fn).OrTypedNilFunc());
    fmt.Println((fn).OrTypedNilFunc());
    fmt.Printf("%v\n"u8, new Fn[]{fn}.array());
    fmt.Printf("%s|%T\n"u8, (fn).OrTypedNilFunc(), (fn).OrTypedNilFunc());

    any x = (fn).OrTypedNilFunc();

    var (s, ok) = x._<fmt.Stringer>(ᐧ);
    fmt.Println("stringer", ok, s != default!);

    if (ok) {
        fmt.Println("string  ", s.String());
    }

    (var f2, ok) = x._<Fn>(ᐧ);
    fmt.Println("concrete", ok, f2 == default!);

    switch (x.type()) {
    case {} Δv when Δv._<fmt.Stringer>(out var v): {
        fmt.Println("switch   Stringer", v.String());
        break;
    }
    default: {
        var v = x;
        fmt.Println("switch   default");
        break;
    }}

    G g = default!;
    any y = (g).OrTypedNilFunc();

    {
        var (gg, okΔ1) = y._<Getter>(ᐧ); if (okΔ1) {
            fmt.Println("getter  ", gg.Get());
        } else {
            fmt.Println("getter   miss");
        }
    }

    (_, ok) = y._<fmt.Stringer>(ᐧ);
    fmt.Println("G->Stringer", ok);

    (_, ok) = x._<Both>(ᐧ);
    fmt.Println("Fn->Both   ", ok);

    Func<nint> plain = default!;
    any z = (plain).OrTypedNilFunc();

    (_, ok) = z._<fmt.Stringer>(ᐧ);
    fmt.Println("plain      ", ok, z != default!);
    fmt.Printf("%v|%T\n"u8, (plain).OrTypedNilFunc(), (plain).OrTypedNilFunc());
}

} // end main_package

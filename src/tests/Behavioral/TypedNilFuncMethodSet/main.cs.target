namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate nint Fn();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string stringFnˢ = "String(fn)"u8;

public static @string String(this Fn fn) {
    return stringFnˢ;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object stringerˢ = (@string)"stringer"u8;
private static readonly object stringˢ = (@string)"string  "u8;
private static readonly object concreteˢ = (@string)"concrete"u8;
private static readonly object switchStringerˢ = (@string)"switch   Stringer"u8;
private static readonly object switchDefaultˢ = (@string)"switch   default"u8;
private static readonly object getterˢ = (@string)"getter  "u8;
private static readonly object getterMissˢ = (@string)"getter   miss"u8;
private static readonly object gStringerˢ = (@string)"G->Stringer"u8;
private static readonly object fnBothˢ = (@string)"Fn->Both   "u8;
private static readonly object plainˢ = (@string)"plain      "u8;

internal static void Main() {
    Fn fn = default!;
    fmt.Printf("%v\n"u8, (fn).OrTypedNilFunc());
    fmt.Println((fn).OrTypedNilFunc());
    fmt.Printf("%v\n"u8, new Fn[]{fn}.array());
    fmt.Printf("%s|%T\n"u8, (fn).OrTypedNilFunc(), (fn).OrTypedNilFunc());
    any x = (fn).OrTypedNilFunc();
    var (s, ok) = x._<fmt.Stringer>(ᐧ);
    fmt.Println(stringerˢ, ok, s != default!);
    if (ok) {
        fmt.Println(stringˢ, s.String());
    }
    (var f2, ok) = x._<Fn>(ᐧ);
    fmt.Println(concreteˢ, ok, f2 == default!);
    switch (x.type()) {
    case {} Δv when Δv._<fmt.Stringer>(out var v): {
        fmt.Println(switchStringerˢ, v.String());
        break;
    }
    default: {
        var v = x;
        fmt.Println(switchDefaultˢ);
        break;
    }}
    G g = default!;
    any y = (g).OrTypedNilFunc();
    {
        var (gg, okΔ1) = y._<Getter>(ᐧ); if (okΔ1){
            fmt.Println(getterˢ, gg.Get());
        } else {
            fmt.Println(getterMissˢ);
        }
    }
    (_, ok) = y._<fmt.Stringer>(ᐧ);
    fmt.Println(gStringerˢ, ok);
    (_, ok) = x._<Both>(ᐧ);
    fmt.Println(fnBothˢ, ok);
    Func<nint> plain = default!;
    any z = (plain).OrTypedNilFunc();
    (_, ok) = z._<fmt.Stringer>(ᐧ);
    fmt.Println(plainˢ, ok, z != default!);
    fmt.Printf("%v|%T\n"u8, (plain).OrTypedNilFunc(), (plain).OrTypedNilFunc());
}

} // end main_package

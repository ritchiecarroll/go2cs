namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct calc /*num:nint*/;

internal static @string Zero(this calc c) {
    return fmt.Sprintf("z(%d)"u8, (nint)c);
}

internal static @string One(this calc c, nint a) {
    return fmt.Sprintf("o(%d,%d)"u8, (nint)c, a);
}

internal static @string Two(this calc c, nint a, nint b) {
    return fmt.Sprintf("t(%d,%d,%d)"u8, (nint)c, a, b);
}

internal static @string Three(this calc c, nint a, nint b, nint d) {
    return fmt.Sprintf("h(%d,%d,%d,%d)"u8, (nint)c, a, b, d);
}

internal static @string Four(this calc c, nint a, nint b, nint d, nint e) {
    return fmt.Sprintf("f(%d,%d,%d,%d,%d)"u8, (nint)c, a, b, d, e);
}

internal static @string Five(this calc c, nint a, nint b, nint d, nint e, nint f) {
    return fmt.Sprintf("v(%d,%d,%d,%d,%d,%d)"u8, (nint)c, a, b, d, e, f);
}

internal static nint Sum(this calc c, nint a, nint b, nint d, nint e, nint f) {
    return (nint)c + 10 * a + 100 * b + 1000 * d + 10000 * e + 100000 * f;
}

internal static @string Mixed(this calc c, @string name, bool flag, nint n) {
    return fmt.Sprintf("m(%d,%s,%t,%d)"u8, (nint)c, name, flag, n);
}

internal partial interface main_type /*dyn*/ {
    @string Zero();
    @string One(nint a);
    @string Two(nint a, nint b);
    @string Three(nint a, nint b, nint d);
    @string Four(nint a, nint b, nint d, nint e);
    @string Five(nint a, nint b, nint d, nint e, nint f);
    nint Sum(nint a, nint b, nint d, nint e, nint f);
    @string Mixed(@string name, bool flag, nint n);
}

internal static void Main() {
    var values = new any[]{((calc)7)}.slice();
    var (v, ok) = values[0]._<main_type>(ᐧ);
    fmt.Println((@string)"ok"u8, ok);
    if (!ok) {
        return;
    }
    for (nint i = 0; i < 2; i++) {
        fmt.Println(i, v.Zero());
        fmt.Println(i, v.One(1));
        fmt.Println(i, v.Two(1, 2));
        fmt.Println(i, v.Three(1, 2, 3));
        fmt.Println(i, v.Four(1, 2, 3, 4));
        fmt.Println(i, v.Five(1, 2, 3, 4, 5));
        fmt.Println(i, v.Sum(1, 2, 3, 4, 5));
        fmt.Println(i, v.Mixed("go"u8, true, 9));
    }
}

} // end main_package

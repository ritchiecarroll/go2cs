namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct withM {
    internal @string s;
}

internal static void M(this withM w) {
}

internal static @string String(this withM w) {
    return w.s;
}

internal static (T, bool) assertAs<T, V>(V v) {
    var (t, ok) = ((any)(v).OrTypedNilOf())._<T>(ᐧ);
    return (t, ok);
}

internal partial interface main_type /*dyn*/ {
    void A();
}

internal partial interface main_typeᴛ1 /*dyn*/ {
    void M();
}

internal partial interface main_typeᴛ2 /*dyn*/ {
    @string String();
}

internal partial struct main_typeᴛ3 /*dyn*/ {
    public nint X, Y;
}

internal static void Main() {
    var (_, okA) = assertAs<main_type, withM>(new withM("x"));
    var (_, okAPtr) = assertAs<main_type, ж<withM>>(Ꮡ(new withM("y")));
    var (m, okM) = assertAs<main_typeᴛ1, withM>(new withM("z"));
    var (s, okS) = assertAs<main_typeᴛ2, withM>(new withM("w"));
    var (p, okP) = assertAs<main_typeᴛ3, main_typeᴛ3>(new main_typeᴛ3(1, 2));

    fmt.Println(okA, okAPtr, okM, m != nil, okS, s.String(), okP, p.X + p.Y);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ж<nint> p = default!;
    var a = GoReflect.WithElemDims(new array<any>[]{new any[]{(nint)(1), (nint)(2)}.array(), new any[]{(@string)"a"u8, (@string)"b"u8}.array()}.slice(), 2);
    var b = new slice<any>[]{new any[]{1.5D, (rune)'x'}.slice(), new any[]{(@string)("c"u8 + "d"u8), true}.slice()}.array();
    var c = new ж<array<any>>[]{Ꮡ(new any[]{(nint)(1), (@string)"p"u8}.array())}.slice();
    var d = new slice<any>[]{new any[]{p.OrTypedNil()}.slice()}.slice();
    var (_, isInt) = a[0][0]._<nint>(ᐧ);
    var (_, isFloat) = b[0][0]._<float64>(ᐧ);
    var (_, isRune) = b[0][1]._<rune>(ᐧ);
    var (s, isString) = a[1][0]._<@string>(ᐧ);
    fmt.Println(a, b, c[0].Value, isInt, isFloat, isRune, s, isString, d[0][0] != default!);
}

} // end main_package

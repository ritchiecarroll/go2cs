namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pt {
    internal nint x, y;
}

partial struct holder {
    internal nint got;
}

internal static nint g;
internal static slice<nint> s;
internal static @string str;
internal static slice<byte> bs;
internal static map<@string, nint> m;
internal static any a;
internal static bool flag;
internal static complex128 cx;
internal static ж<holder> h;
internal static slice<nint> dst = new slice<nint>(4);

internal static void reset() {
    (g, s, str, bs, m, a, flag, cx) = (1, slice<nint>([1]), "a", slice<byte>("k"u8), new map<@string, nint>{["k"] = 1}, (nint)1, false, 1D);
    h = Ꮡ(new holder(got: 1));
}

internal static nint mut() {
    (g, s, str, bs, m, a, flag, cx) = (2, slice<nint>([1, 2]), "ab", slice<byte>("j"u8), new map<@string, nint>{["k"] = 2, ["j"] = 2}, (nint)2, true, 2D);
    h.got = 2;
    return 0;
}

internal static @string ints(nint a, nint b, nint c) { return fmt.Sprint(a, " ", b, " ", c); }
internal static @string slices(nint a, slice<nint> b, nint c) { return fmt.Sprint(a, " ", b, " ", c); }
internal static @string bytes(nint a, slice<byte> b, nint c) { return fmt.Sprint(a, " ", b, " ", c); }
internal static @string runes(nint a, slice<rune> b, nint c) { return fmt.Sprint(a, " ", b, " ", c); }
internal static @string floats(nint a, float64 b, nint c) {
    return fmt.Sprint(a, " ", b, " ", c);
}
internal static @string complexes(nint a, complex128 b, nint c) { return fmt.Sprint(a, " ", b, " ", c); }

internal static nint say(@string text) {
    fmt.Print(text, " ");
    return 1;
}

internal static void main() {
    reset();
    var evalᴛ1 = len(s);
    var evalᴛ2 = mut();
    fmt.Println("len:", ints(g, evalᴛ1, evalᴛ2));
    reset();
    var evalᴛ3 = cap(s);
    var evalᴛ4 = mut();
    fmt.Println("cap:", ints(g, evalᴛ3, evalᴛ4));
    reset();
    var evalᴛ5 = appendꓸꓸꓸ(slice<nint>([]), s);
    var evalᴛ6 = mut();
    fmt.Println("append:", slices(g, evalᴛ5, evalᴛ6));
    reset();
    var evalᴛ7 = min(g, 5);
    var evalᴛ8 = mut();
    fmt.Println("min:", ints(g, evalᴛ7, evalᴛ8));
    reset();
    var evalᴛ9 = max(g, 0);
    var evalᴛ10 = mut();
    fmt.Println("max:", ints(g, evalᴛ9, evalᴛ10));
    reset();
    var evalᴛ11 = new slice<nint>(g);
    var evalᴛ12 = mut();
    fmt.Println("make:", slices(g, evalᴛ11, evalᴛ12));
    reset();
    var evalᴛ13 = real(cx);
    var evalᴛ14 = mut();
    fmt.Println("real:", floats(g, evalᴛ13, evalᴛ14));
    reset();
    var evalᴛ15 = imag(cx * 1D.i());
    var evalᴛ16 = mut();
    fmt.Println("imag:", floats(g, evalᴛ15, evalᴛ16));
    reset();
    var evalᴛ17 = complex((float64)g, 0D);
    var evalᴛ18 = mut();
    fmt.Println("complex:", complexes(g, evalᴛ17, evalᴛ18));
    reset();
    var evalᴛ19 = copy(dst, s);
    var evalᴛ20 = mut();
    fmt.Println("copy:", ints(g, evalᴛ19, evalᴛ20));
    reset();
    var evalᴛ21 = slice<byte>(str);
    var evalᴛ22 = mut();
    fmt.Println("[]byte:", bytes(g, evalᴛ21, evalᴛ22));
    reset();
    var evalᴛ23 = slice<rune>(str);
    var evalᴛ24 = mut();
    fmt.Println("[]rune:", runes(g, evalᴛ23, evalᴛ24));
    reset();
    var evalᴛ25 = a._<nint>();
    var evalᴛ26 = mut();
    fmt.Println("type assertion:", ints(g, evalᴛ25, evalᴛ26));
    reset();
    var evalᴛ27 = s[0..];
    var evalᴛ28 = mut();
    fmt.Println("slice expression:", slices(g, evalᴛ27, evalᴛ28));
    reset();
    var evalᴛ29 = m[builtin.tmpstring(bs)];
    var evalᴛ30 = mut();
    fmt.Println("map by bytes:", ints(g, evalᴛ29, evalᴛ30));
    reset();
    var evalᴛ31 = g > 1;
    var evalᴛ32 = mut();
    fmt.Println("boxed bool:", g, evalᴛ31, evalᴛ32);
    reset();
    var evalᴛ33 = !flag;
    var evalᴛ34 = mut();
    fmt.Println("boxed not:", g, evalᴛ33, evalᴛ34);
    reset();
    var evalᴛ35 = (uint8)g;
    var evalᴛ36 = mut();
    fmt.Println("boxed byte:", g, evalᴛ35, evalᴛ36);
    reset();
    var evalᴛ37 = new pt(g, 0);
    var evalᴛ38 = mut();
    fmt.Println("boxed struct:", g, evalᴛ37, evalᴛ38);
    reset();
    var evalᴛ39 = array<nint>([g, 0]);
    var evalᴛ40 = mut();
    fmt.Println("boxed array:", g, evalᴛ39, evalᴛ40);
    reset();
    var evalᴛ41 = Ꮡ(new pt(g, 0));
    var evalᴛ42 = mut();
    fmt.Println("boxed pointer literal:", g, evalᴛ41, evalᴛ42);
    reset();
    var evalᴛ43 = mut();
    fmt.Println("boxed int stays late:", g, g + 0, evalᴛ43);
    reset();
    fmt.Println("printf comparison:", fmt.Sprintf("%v %d"u8, h.got == 1, mut()));
    reset();
    var evalᴛ44 = say("L") == 0 || say("R") == 1;
    var evalᴛ45 = evalᴛ44;
    var evalᴛ46 = say("after");
    fmt.Println("&& unit:", fmt.Sprint(g, evalᴛ45, evalᴛ46));
    reset();
    fmt.Println("&& without a call:", g == 1 && !flag, mut());
}

} // end main_package

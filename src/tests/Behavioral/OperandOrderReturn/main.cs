namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint f;
}

internal static nint g;

internal static (nint, nint) local() {
    nint r = 1;
    [GoFunc("local.func1")] nint setR() {
        r = 10;
        return 2;
    }
    var evalᴛ1 = setR();
    return (r, evalᴛ1);
}

internal static (nint, nint) global() {
    g = 1;
    [GoFunc("global.func1")] nint setG() {
        g = 10;
        return 2;
    }
    var evalᴛ1 = setG();
    return (g, evalᴛ1);
}

internal static (nint, nint) field(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    [GoFunc("field.func1")] nint setF() {
        Ꮡb.Value.f = 10;
        return 2;
    }
    var evalᴛ1 = setF();
    return (b.f, evalᴛ1);
}

internal static (nint a, nint b) named() {
    nint a = default;

    a = 1;
    [GoFunc("named.func1")] nint setA() {
        a = 10;
        return 2;
    }
    var evalᴛ1 = setA();
    return (a, evalᴛ1);
}

internal static void main() {
    var (ᴛ1, ᴛ2) = local();
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = global();
    fmt.Println(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = field(Ꮡ(new box(1)));
    fmt.Println(ᴛ5, ᴛ6);
    var (ᴛ7, ᴛ8) = named();
    fmt.Println(ᴛ7, ᴛ8);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using latelib = ItabLateRegistration.latelib_package;
using lib = ItabLateRegistration.lib_package;
using ItabLateRegistration;

partial class main_package {

internal static void round(@string tag) {
    ref var c = ref heap<lib.Circle>(new lib.Circle(R: 3), out var Ꮡc);
    ref var q = ref heap<lib.Square>(new lib.Square(S: 4), out var Ꮡq);
    ref var b = ref heap<lib.Blank>(new lib.Blank(N: 5), out var Ꮡb);

    for (nint i = 0; i < 3; i++) {
        var (n1, ok1) = lib.TryName(Ꮡc);
        var (n2, ok2) = lib.TryName(Ꮡq);
        fmt.Println(tag, "name", i, n1, ok1, n2, ok2);
    }

    var (n3, ok3) = lib.TryName(Ꮡb);
    fmt.Println(tag, "miss", n3, ok3);

    var (s1, ok4) = lib.TrySize(Ꮡc);
    var (s2, ok5) = lib.TrySize(Ꮡq);
    var (s3, ok6) = lib.TrySize(Ꮡb);
    fmt.Println(tag, "size", s1, ok4, s2, ok5, s3, ok6);
}

internal static (@string, nint) primeLate() {
    return latelib.Prime();
}

internal static void Main() {
    round("before");

    var (pn, ps) = primeLate();
    fmt.Println("primed", pn, ps);

    round("after ");
}

} // end main_package

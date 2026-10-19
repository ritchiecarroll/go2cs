namespace go;

using fmt = fmt_package;
using latelib = ItabLateRegistration.latelib_package;
using lib = ItabLateRegistration.lib_package;
using ItabLateRegistration;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nameˢ = (@string)"name"u8;
private static readonly object missˢ = (@string)"miss"u8;
private static readonly object sizeˢ = (@string)"size"u8;

internal static void round(@string tag) {
    ref var c = ref heap<lib.Circle>(out var Ꮡc);
    c = new lib.Circle(R: 3);
    ref var q = ref heap<lib.Square>(out var Ꮡq);
    q = new lib.Square(S: 4);
    ref var b = ref heap<lib.Blank>(out var Ꮡb);
    b = new lib.Blank(N: 5);

    for (nint i = 0; i < 3; i++) {
        var (n1, ok1) = lib.TryName(Ꮡc);
        var (n2, ok2) = lib.TryName(Ꮡq);
        fmt.Println(tag, nameˢ, i, n1, ok1, n2, ok2);
    }

    var (n3, ok3) = lib.TryName(Ꮡb);
    fmt.Println(tag, missˢ, n3, ok3);

    var (s1, ok4) = lib.TrySize(Ꮡc);
    var (s2, ok5) = lib.TrySize(Ꮡq);
    var (s3, ok6) = lib.TrySize(Ꮡb);
    fmt.Println(tag, sizeˢ, s1, ok4, s2, ok5, s3, ok6);
}

internal static (@string, nint) primeLate() {
    return latelib.Prime();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object primedˢ = (@string)"primed"u8;

internal static void Main() {
    round("before");

    var (pn, ps) = primeLate();
    fmt.Println(primedˢ, pn, ps);

    round("after ");
}

} // end main_package

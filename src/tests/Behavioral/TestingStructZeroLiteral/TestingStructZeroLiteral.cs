namespace go;

using fmt = fmt_package;
using testing = testing_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pbConstructedˢ = (@string)"PB: constructed"u8;

internal static void Main() {
    var t = Ꮡ(new testing.T(nil));
    var m = Ꮡ(new testing.M(nil));
    var f = Ꮡ(new testing.F(nil));
    var b = new testing.B(nil);
    var pb = new testing.PB(nil);
    fmt.Println((@string)"T:"u8, t != nil);
    fmt.Println((@string)"M:"u8, m != nil);
    fmt.Println((@string)"F:"u8, f != nil);
    fmt.Println((@string)"B.N:"u8, b.N);
    _ = pb;
    fmt.Println(pbConstructedˢ);
}

} // end main_package

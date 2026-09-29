namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{10, 20, 30, 40}.array();
    uintptr i = 2;
    var pi = Ꮡa.at<nint>((ulong)(i));
    pi.Value = 99;
    nuint u = 1;
    var pu = Ꮡa.at<nint>((ulong)(u));
    pu.Value = 88;
    uint64 w = 3;
    var pw = Ꮡa.at<nint>((ulong)(w));
    pw.Value = 77;
    nuint g = 5;
    var pe = Ꮡa.at<nint>((ulong)(g % 2));
    pe.Value = 66;
    fmt.Println(a[0], a[1], a[2], a[3]);
    uintptr pc = 0xAB;
    fmt.Println(LiteralByteAt("0123456789abcdef"u8, (uintptr)(pc & 15)));
    fmt.Println(LiteralByteAt("0123456789abcdef"u8, (uintptr)((pc >> (int)(4)) & 15)));
    @string hex = "0123456789abcdef"u8;
    uint64 k = 12;
    fmt.Println(hex[k]);
}

} // end main_package

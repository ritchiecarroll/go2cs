namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct hdr {
    public uint16 A;
    public uint16 B;
    public uint16 C;
    public uint16 D;
}

internal static partial ж<hdr> asHdr(slice<byte> b) {
    return Ꮡ(b, 0).Reinterpret<byte, hdr>();
}

internal static partial void churn() {
    var keep = new slice<slice<byte>>(64);
    for (nint i = 0; i < 400000; i++) {
        keep[(nint)(i & 63)] = new slice<byte>(24);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object readˢ = (@string)"read:"u8;
private static readonly object writeˢ = (@string)"write:"u8;
private static readonly object retainedˢ = (@string)"retained:"u8;

internal static void Main() {
    var b = new slice<byte>(8);
    var h = asHdr(b);
    h.Value.A = 0x0201;
    churn();
    fmt.Println(readˢ, (~h).A == 0x0201, b[0] == 1, b[1] == 2);
    h.Value.B = 0x0403;
    h.Value.C = 0x0605;
    h.Value.D = 0x0807;
    fmt.Println(writeˢ, b[2] == 3, b[3] == 4, b[4] == 5, b[5] == 6, b[6] == 7, b[7] == 8);
    var r = asHdr(new slice<byte>(8));
    r.Value.A = 0x0a09;
    r.Value.D = 0x100f;
    churn();
    fmt.Println(retainedˢ, (~r).A == 0x0a09, (~r).B == 0, (~r).C == 0, (~r).D == 0x100f);
}

} // end main_package

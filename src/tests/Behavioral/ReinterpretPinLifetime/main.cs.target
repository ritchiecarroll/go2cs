namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct hdr {
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
        keep[(i & 63)] = new slice<byte>(24);
    }
}

internal static void Main() {
    var b = new slice<byte>(8);
    var h = asHdr(b);

    h.A = 0x0201;
    churn();
    fmt.Println("read:", h.A == 0x0201, b[0] == 1, b[1] == 2);

    h.B = 0x0403;
    h.C = 0x0605;
    h.D = 0x0807;
    fmt.Println("write:", b[2] == 3, b[3] == 4, b[4] == 5, b[5] == 6, b[6] == 7, b[7] == 8);

    var r = asHdr(new slice<byte>(8));
    r.A = 0x0a09;
    r.D = 0x100f;
    churn();
    fmt.Println("retained:", r.A == 0x0a09, r.B == 0, r.C == 0, r.D == 0x100f);
}

} // end main_package

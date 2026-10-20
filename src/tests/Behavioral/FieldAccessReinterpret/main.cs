namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

[GoFieldAccess(typeof(record), typeof(syscall.RawSockaddrInet4))]
partial class main_package {

partial struct tail {
    public uint16 A, B;
}

partial struct record {
    public uint32 Kind;
    public uint16 Length;
    public uint16 Off;
    public array<uint16> Path = new(4);
    /*embed*/ internal tail tail;
}

internal static void Main() {
    var blob = new slice<byte>(24);
    var rec = Ꮡ(blob, 0).Reinterpret<byte, record>();

    rec.Kind = 0x01020304;
    rec.Length = 0x0506;
    rec.Off = 7;
    rec.Off += 1;
    rec.of(record.ᏑPath).ElementRef(1) = 0x0a0b;
    nint n = copy(rec.of(record.ᏑPath).slice(2), new uint16[]{0x0c0d}.slice());
    rec.tail.B = 0x1112;

    var p = rec.of(record.ᏑLength);
    p.Value += 1;

    fmt.Println("bytes:", blob[..20]);
    fmt.Println("fields:", rec.Kind == 0x01020304, rec.Length, rec.Off, rec.of(record.ᏑPath).ElementRef(1), rec.of(record.ᏑPath).ElementRef(2), rec.tail.B, n);

    var raw = new slice<byte>(16);
    var sa = Ꮡ(raw, 0).Reinterpret<byte, syscall.RawSockaddrInet4>();
    sa.Port = 0x901f;
    fmt.Println("port bytes:", raw[2], raw[3], "port:", sa.Port);
}

} // end main_package

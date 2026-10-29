namespace go;

using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

[GoFieldAccess(typeof(record), typeof(syscall.RawSockaddrInet4), typeof(reparse))]
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

partial struct reparse {
    public uint32 Tag;
    public uint16 Length, Reserved;
    public uint32 Flags;
    public array<uint16> Path = new(1);
}

internal static slice<byte> sink;

internal static (ж<reparse>, error) readReparse(uint32 flags, bool fail) {
    ref var buf = ref heap(new array<byte>(64), out var Ꮡbuf);

    if (fail) {
        return (default, errors.New("no record"));
    }

    (buf[8], buf[9]) = ((byte)flags, (byte)(flags >> 8));
    buf[12] = (byte)'x';
    var rec = Ꮡbuf.at<byte>(0).Reinterpret<byte, reparse>();
    return (rec, default);
}

internal static ж<reparse> viewReparse(slice<byte> blob) {
    return Ꮡ(blob, 8).Reinterpret<byte, reparse>();
}

internal static void Main() {
    var blob = new slice<byte>(24);
    var rec = Ꮡ(blob, 0).Reinterpret<byte, record>();

    rec.Kind = 0x01020304;
    rec.Length = 0x0506;
    rec.Off = 7;
    rec.Off += 1;
    rec.of(record.ᏑPath).ElementRef(1) = 0x0a0b;
    nint n = copy(rec.of(record.ᏑPath).slice(2), slice<uint16>([0x0c0d]));
    rec.tail.B = 0x1112;

    var p = rec.of(record.ᏑLength);
    p.Value += 1;

    fmt.Println("bytes:", blob[..20]);
    fmt.Println("fields:", rec.Kind == 0x01020304, rec.Length, rec.Off, rec.of(record.ᏑPath).ElementRef(1), rec.of(record.ᏑPath).ElementRef(2), rec.tail.B, n);

    var raw = new slice<byte>(16);
    var sa = Ꮡ(raw, 0).Reinterpret<byte, syscall.RawSockaddrInet4>();
    sa.Port = 0x901f;
    fmt.Println("port bytes:", raw[2], raw[3], "port:", sa.Port);

    var (got, err) = readReparse(0x0201, false);

    for (nint i = 0; i < 1000; i++) {
        sink = new slice<byte>(1024);
    }

    runtime.GC();
    fmt.Println("returned:", got.Flags, got.of(reparse.ᏑPath).ElementRef(0), err);

    var view = viewReparse(blob);
    view.Flags = 0x0d0c0b0a;
    fmt.Println("view bytes:", blob[16..20]);

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("nil record:", recover());
            }, ref ᒐ);

            var (missing, errΔ1) = readReparse(0, true);
            fmt.Println("error:", errΔ1);
            fmt.Println(missing.Flags);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package

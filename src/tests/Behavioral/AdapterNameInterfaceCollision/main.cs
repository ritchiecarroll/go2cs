namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

[GoType] partial interface Reader :
    Δio.Reader
{
    nint Remaining();
}

[GoType] partial struct src {
    internal slice<byte> data;
    internal nint pos;
}

[GoRecv] internal static (nint, error) Read(this ref src s, slice<byte> p) {
    if (s.pos >= len(s.data)) {
        return (0, Δio.EOF);
    }
    nint n = copy(p, s.data.slice(s.pos));
    s.pos += n;
    return (n, default!);
}

[GoRecv] internal static nint Remaining(this ref src s) {
    return len(s.data) - s.pos;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errˢ = "err"u8;

internal static @string viaForeign(Δio.Reader r) {
    var buf = new slice<byte>(4);
    var (n, err) = r.Read(buf);
    if (err != default!) {
        return errˢ;
    }
    return ((@string)(buf.slice(0, n)));
}

internal static @string viaLocal(Reader r) {
    nint before = r.Remaining();
    var buf = new slice<byte>(3);
    var (n, err) = r.Read(buf);
    if (err != default!) {
        return errˢ;
    }
    return fmt.Sprintf("%d|%s|%d"u8, before, ((@string)(buf.slice(0, n))), r.Remaining());
}

internal static void Main() {
    fmt.Println(viaLocal(new srcжReader(Ꮡ(new src(data: slice<byte>("abcd"u8))))));
    fmt.Println(viaForeign(new srcжio_Reader(Ꮡ(new src(data: slice<byte>("wxyz"u8))))));
    var s = Ꮡ(new src(data: slice<byte>("0123456789"u8)));
    fmt.Println(viaLocal(new srcжReader(s)));
    fmt.Println(viaForeign(new srcжio_Reader(s)));
    fmt.Println(viaLocal(new srcжReader(s)));
}

} // end main_package

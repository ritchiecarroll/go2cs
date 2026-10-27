namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static nint readInto(ж<uint16> Ꮡbuf, nint toread, slice<uint16> src) {
    return copy(array<uint16>.AliasPointer(Ꮡbuf, 10000).slice(0, toread, toread), src);
}

internal static void bumpAll(ж<byte> Ꮡp) {
    var a = array<byte>.AliasPointer(Ꮡp, 4);
    foreach (var i in range(a, 4)) {
        a.Value[i]++;
    }
}

internal static void Main() {
    var dst = new slice<uint16>(8);
    nint n = readInto(Ꮡ(dst, 0), 4, new uint16[]{1, 2, 3, 4}.slice());
    fmt.Println("copied:", n, "dst:", dst);

    n = readInto(Ꮡ(dst, 4), 3, new uint16[]{7, 8, 9}.slice());
    fmt.Println("copied:", n, "dst:", dst);

    var p = array<uint16>.AliasPointer(Ꮡ(dst, 2), 4);
    p.Value[0] = 0x55;
    p.Value[3] = 0x66;
    fmt.Println("indexed write:", dst);

    dst[3] = 0xab;
    fmt.Println("read back:", p.Value[0], p.Value[1], p.Value[2], p.Value[3]);

    var deref = p.Value.Clone();
    deref[1] = 0x00;
    fmt.Println("deref copy:", deref, "still through pointer:", p.Value[1]);

    var view = p.slice();
    view[2] = 0x7f;
    fmt.Println("p[:] aliases:", dst[4], "len/cap:", len(view), cap(view));

    var inner = p.slice(1, 3);
    inner[0] = 0x21;
    fmt.Println("p[1:3] aliases:", dst[3], "len/cap:", len(inner), cap(inner));

    fmt.Println("element identity:", p.Ꮡelem<uint16>(0) == Ꮡelem(dst, 2), "windowed identity:", p.Ꮡelem<uint16>(2) == Ꮡelem(dst, 4));

    fmt.Println("array length:", 4, len(p.Value));

    ref var raw = ref heap(new array<byte>(8), out var Ꮡraw);
    foreach (var i in range(raw)) {
        raw[i] = (byte)i;
    }
    bumpAll(Ꮡraw.at<byte>(3));
    fmt.Println("fixed array:", raw);

    var buf = new byte[]{0, 1, 2, 3, 4, 5, 6, 7}.slice();
    var tail = buf[4..];
    bumpAll(Ꮡ(tail, 0));
    fmt.Println("resliced view:", buf);
}

} // end main_package

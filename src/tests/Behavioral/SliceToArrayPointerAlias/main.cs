namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void writeThrough(slice<byte> dst) {
    var d = Ꮡ(array<byte>.Alias(dst, 4));
    d.Value[0] = 0x11;
    d.Value[1] = 0x22;
    d.Value[2] = 0x33;
    d.Value[3] = 0x44;
}

internal static array<byte> valueCopy(slice<byte> src) {
    var a = new array<byte>(src, 4);
    a[0] = 0xff;
    return a.Clone();
}

internal static void premultiplyLike(slice<byte> dst, slice<byte> src) {
    for (; len(src) >= 4; (dst, src) = (dst[4..], src[4..])) {
        var d = Ꮡ(array<byte>.Alias(dst, 4));
        var s = Ꮡ(array<byte>.Alias(src, 4));
        if (s.Value[3] == 0x00){
            (d.Value[0], d.Value[1], d.Value[2], d.Value[3]) = (0, 0, 0, 0);
        } else 
        if (s.Value[3] == 0xff){
            copy(d.slice(), s.slice());
        } else {
            d.Value[0] = (byte)(s.Value[0] / 2);
            d.Value[1] = (byte)(s.Value[1] / 2);
            d.Value[2] = (byte)(s.Value[2] / 2);
            d.Value[3] = s.Value[3];
        }
    }
}

internal static void Main() {
    var buf = new slice<byte>(9);
    buf[0] = 0x99;
    writeThrough(buf[1..]);
    fmt.Println("aliased write:", buf);

    var p = Ꮡ(array<byte>.Alias(buf[1..], 4));
    buf[2] = 0xab;
    fmt.Println("read back through pointer:", p.Value[0], p.Value[1], p.Value[2], p.Value[3]);

    var got = valueCopy(buf[1..]);
    fmt.Println("value conversion:", got, "slice unchanged:", buf[1..5]);

    var deref = p.Value.Clone();
    deref[1] = 0x00;
    fmt.Println("deref copy:", deref, "still through pointer:", p.Value[1]);

    var view = p.slice();
    view[3] = 0x7f;
    fmt.Println("slice of array pointer:", buf[4], "len/cap:", len(view), cap(view));

    fmt.Println("element identity:", p.at<byte>(0) == Ꮡ(buf, 1), "windowed identity:", p.at<byte>(2) == Ꮡ(buf, 3));

    var src = new byte[]{
        0x10, 0x20, 0x30, 0x00,
        0x40, 0x50, 0x60, 0xff,
        0x80, 0x90, 0xa0, 0x40
    }.slice();
    var dst = new slice<byte>(len(src));
    premultiplyLike(dst, src);
    fmt.Println("windowed loop:", dst);

    fmt.Println("array length:", 4, len(p.Value));
}

} // end main_package

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aliasedWriteˢ = (@string)"aliased write:"u8;
private static readonly object readBackThroughPointerˢ = (@string)"read back through pointer:"u8;
private static readonly object valueConversionˢ = (@string)"value conversion:"u8;
private static readonly object sliceUnchangedˢ = (@string)"slice unchanged:"u8;
private static readonly object derefCopyˢ = (@string)"deref copy:"u8;
private static readonly object stillThroughPointerˢ = (@string)"still through pointer:"u8;
private static readonly object sliceOfArrayPointerˢ = (@string)"slice of array pointer:"u8;
private static readonly object lenCapˢ = (@string)"len/cap:"u8;
private static readonly object elementIdentityˢ = (@string)"element identity:"u8;
private static readonly object windowedIdentityˢ = (@string)"windowed identity:"u8;
private static readonly object windowedLoopˢ = (@string)"windowed loop:"u8;
private static readonly object arrayLengthˢ = (@string)"array length:"u8;

internal static void Main() {
    var buf = new slice<byte>(9);
    buf[0] = 0x99;
    writeThrough(buf[1..]);
    fmt.Println(aliasedWriteˢ, buf);

    var p = Ꮡ(array<byte>.Alias(buf[1..], 4));
    buf[2] = 0xab;
    fmt.Println(readBackThroughPointerˢ, p.Value[0], p.Value[1], p.Value[2], p.Value[3]);

    var got = valueCopy(buf[1..]);
    fmt.Println(valueConversionˢ, got, sliceUnchangedˢ, buf[1..5]);

    var deref = p.Value.Clone();
    deref[1] = 0x00;
    fmt.Println(derefCopyˢ, deref, stillThroughPointerˢ, p.Value[1]);

    var view = p.slice();
    view[3] = 0x7f;
    fmt.Println(sliceOfArrayPointerˢ, buf[4], lenCapˢ, len(view), cap(view));

    fmt.Println(elementIdentityˢ, p.at<byte>(0) == Ꮡ(buf, 1), windowedIdentityˢ, p.at<byte>(2) == Ꮡ(buf, 3));

    var src = new byte[]{
        0x10, 0x20, 0x30, 0x00,
        0x40, 0x50, 0x60, 0xff,
        0x80, 0x90, 0xa0, 0x40
    }.slice();
    var dst = new slice<byte>(len(src));
    premultiplyLike(dst, src);
    fmt.Println(windowedLoopˢ, dst);

    fmt.Println(arrayLengthˢ, 4, len(p.Value));
}

} // end main_package

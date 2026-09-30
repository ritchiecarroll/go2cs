namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static nint readInto(ж<uint16> Ꮡbuf, nint toread, slice<uint16> src) {
    return copy((~array<uint16>.AliasPointer(Ꮡbuf, 10000)).slice(0, toread, toread), src);
}

internal static void bumpAll(ж<byte> Ꮡp) {
    var a = array<byte>.AliasPointer(Ꮡp, 4);
    foreach (var (i, _) in a.Value) {
        a.Value[i]++;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object copiedˢ = (@string)"copied:"u8;
private static readonly object dstˢ = (@string)"dst:"u8;
private static readonly object indexedWriteˢ = (@string)"indexed write:"u8;
private static readonly object readBackˢ = (@string)"read back:"u8;
private static readonly object derefCopyˢ = (@string)"deref copy:"u8;
private static readonly object stillThroughPointerˢ = (@string)"still through pointer:"u8;
private static readonly object pAliasesˢ = (@string)"p[:] aliases:"u8;
private static readonly object lenCapˢ = (@string)"len/cap:"u8;
private static readonly object p13Aliasesˢ = (@string)"p[1:3] aliases:"u8;
private static readonly object elementIdentityˢ = (@string)"element identity:"u8;
private static readonly object windowedIdentityˢ = (@string)"windowed identity:"u8;
private static readonly object arrayLengthˢ = (@string)"array length:"u8;
private static readonly object fixedArrayˢ = (@string)"fixed array:"u8;
private static readonly object reslicedViewˢ = (@string)"resliced view:"u8;

internal static void Main() {
    var dst = new slice<uint16>(8);
    nint n = readInto(Ꮡ(dst, 0), 4, new uint16[]{1, 2, 3, 4}.slice());
    fmt.Println(copiedˢ, n, dstˢ, dst);
    n = readInto(Ꮡ(dst, 4), 3, new uint16[]{7, 8, 9}.slice());
    fmt.Println(copiedˢ, n, dstˢ, dst);
    var p = array<uint16>.AliasPointer(Ꮡ(dst, 2), 4);
    p.Value[0] = 0x55;
    p.Value[3] = 0x66;
    fmt.Println(indexedWriteˢ, dst);
    dst[3] = 0xab;
    fmt.Println(readBackˢ, p.Value[0], p.Value[1], p.Value[2], p.Value[3]);
    var deref = p.Value.Clone();
    deref[1] = 0x00;
    fmt.Println(derefCopyˢ, deref, stillThroughPointerˢ, p.Value[1]);
    var view = (~p)[..];
    view[2] = 0x7f;
    fmt.Println(pAliasesˢ, dst[4], lenCapˢ, len(view), cap(view));
    var inner = (~p)[1..3];
    inner[0] = 0x21;
    fmt.Println(p13Aliasesˢ, dst[3], lenCapˢ, len(inner), cap(inner));
    fmt.Println(elementIdentityˢ, p.at<uint16>(0) == Ꮡ(dst, 2), windowedIdentityˢ, p.at<uint16>(2) == Ꮡ(dst, 4));
    fmt.Println(arrayLengthˢ, 4, len(p.Value));
    ref var raw = ref heap(new array<byte>(8), out var Ꮡraw);
    foreach (var (i, _) in raw) {
        raw[i] = (byte)i;
    }
    bumpAll(Ꮡraw.at<byte>(3));
    fmt.Println(fixedArrayˢ, raw);
    var buf = new byte[]{0, 1, 2, 3, 4, 5, 6, 7}.slice();
    var tail = buf[4..];
    bumpAll(Ꮡ(tail, 0));
    fmt.Println(reslicedViewˢ, buf);
}

} // end main_package

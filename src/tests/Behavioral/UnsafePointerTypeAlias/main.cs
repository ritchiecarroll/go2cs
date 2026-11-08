global using ptr = go.unsafe_package.Pointer;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct cursor {
    internal ptr @base;
    internal uintptr off;
}

internal static uint8 readU8(ptr p, uintptr o) {
    return ~(ж<uint8>)(uintptr)((ptr)((uintptr)p + o));
}

internal static uint32 readU32(ptr p, uintptr o) {
    return ~(ж<uint32>)(uintptr)(@unsafe.Add(p, o));
}

internal static uint8 next(this ref cursor c) {
    var b = readU8(c.@base, c.off);
    c.off++;
    return b;
}

internal static void Main() {
    ref var x = ref heap<uint64>((uint64)42, out var Ꮡx);
    ptr p = @unsafe.Pointer.FromPinnedBox(Ꮡx);
    fmt.Println(~(ж<uint64>)(uintptr)(p));

    ref var buf = ref heap<array<byte>>(new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.array(), out var Ꮡbuf);
    ref var q = ref heap<ptr>(new ptr(Ꮡbuf.at<byte>(0)), out var Ꮡq);
    fmt.Println(readU8(q, 3), readU32(q, 4) == (uint32)((uint32)((uint32)((uint32)5 | (uint32)6 << 8) | (uint32)7 << 16) | (uint32)8 << 24));

    var c = Ꮡ(new cursor(@base: q));
    fmt.Println(c.next(), c.next(), c.off);
    fmt.Println(p != default, q == @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));
}

} // end main_package

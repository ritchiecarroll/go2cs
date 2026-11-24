global using ptr = go.unsafe_package.Pointer;

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using ualias = global::go.go.ualias_package;
using global::go.go;

partial class main_package {

partial struct cursor {
    internal ptr @base;
    internal uintptr off;
}

internal static uint8 readU8(ptr p, uintptr o) {
    return ~@unsafe.Pointer.To<uint8>(@unsafe.Add(p, o));
}

internal static uint32 readU32(ptr p, uintptr o) {
    return ~@unsafe.Pointer.To<uint32>(@unsafe.Add(p, o));
}
// type gptr[T any] = unsafe.Pointer

partial struct word /*num:uintptr*/;

internal static ptr self(this ж<cursor> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    return (uintptr)ptr.FromRef(ref c);
}

internal static nint headerData(nint rounds) {
    nint sum = 0;

    for (nint i = 0; i < rounds; i++) {
        ref var data = ref heap<slice<byte>>(slice<byte>([(byte)i, (byte)(i + 1), (byte)(i + 2), (byte)(i + 3)]), out var Ꮡdata);
        var h = Ꮡdata.Reinterpret<slice<byte>, reflect.SliceHeader>();
        ptr d = ptr.FromHeaderData(h);
        _ = new slice<byte>(4096);
        runtime.GC();
        sum += (nint)(~@unsafe.Pointer.To<byte>(@unsafe.Add(d, 3)));
    }

    return sum;
}

internal static uint8 next(this ref cursor c) {
    var b = readU8(c.@base, c.off);
    c.off++;
    return b;
}

internal static void main() {
    ref var x = ref heap<uint64>((uint64)42, out var Ꮡx);
    ptr p = @unsafe.Pointer.FromPinnedBox(Ꮡx);
    fmt.Println(~@unsafe.Pointer.To<uint64>(p));

    ref var buf = ref heap<array<byte>>(array<byte>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡbuf);
    ref var q = ref heap<ptr>(ptr.FromPinnedBox(Ꮡbuf.at<byte>(0)), out var Ꮡq);
    fmt.Println(readU8(q, 3), readU32(q, 4) == (uint32)((uint32)((uint32)((uint32)5 | (uint32)6 << 8) | (uint32)7 << 16) | (uint32)8 << 24));

    var c = Ꮡ(new cursor(@base: q));
    fmt.Println(c.next(), c.next(), c.off);
    fmt.Println(p != default, q == @unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)));

    fmt.Println(headerData(64));
    fmt.Println(c.self() != default, ((ptr)(uintptr)((word)0)) == default);

    ref var y = ref heap<uint64>((uint64)11, out var Ꮡy);
    ualiasꓸPtr qa = ualiasꓸPtr.FromPinnedBox(Ꮡy);
    ref var z = ref heap<uint64>((uint64)12, out var Ꮡz);
    @unsafe.Pointer ga = @unsafe.Pointer.FromPinnedBox(Ꮡz);
    runtime.GC();
    fmt.Println(~@unsafe.Pointer.To<uint64>(qa), ~@unsafe.Pointer.To<uint64>(ga));
}

} // end main_package

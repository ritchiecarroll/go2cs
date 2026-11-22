namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

/* [1125899906842623L]byte */
partial struct RawMem /*[1125899906842623]byte*/;

partial struct smallMem /*[64]byte*/;

partial struct hdr {
    internal uint32 a;
    internal array<byte> buf = new(12);
}

internal static readonly ж<array<uint32>> ᏑchachaInit = new StaticBox<array<uint32>>(array<uint32>([0x61707865, 0x3320646e, 0x79622d32, 0x6b206574]));
internal static ref array<uint32> chachaInit => ref ((StaticBox<array<uint32>>)ᏑchachaInit).ValueSlot;
internal static readonly ж<array<uint8>> Ꮡmagic = new StaticBox<array<uint8>>(array<uint8>([0xd9, 0xd5, 0x05, 0xf9, 0x20, 0xa1, 0x63, 0xd7]));
internal static ref array<uint8> magic => ref ((StaticBox<array<uint8>>)Ꮡmagic).ValueSlot;

internal static uintptr native(@string fill) {
    var (b, err) = syscall.Mmap(-1, 0, 4096, (nint)((nint)syscall.PROT_READ | (nint)syscall.PROT_WRITE), (nint)((nint)syscall.MAP_ANON | (nint)syscall.MAP_PRIVATE));

    if (err != nil) {
        throw panic(err);
    }

    var p = (uintptr)Ꮡ(b, 0);
    poke(p, fill);

    return p;
}

internal static @string peek(uintptr p, nint n) {
    var @out = new slice<byte>(n);

    for (nint i = 0; i < n; i++) {
        @out[i] = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + (uintptr)i));
    }

    return fmt.Sprintf("%q"u8, @out);
}

internal static void poke(uintptr p, @string s) {
    for (nint i = 0; i < len(s); i++) {
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + (uintptr)i))).Value = s[i];
    }
}

internal static nint fill(slice<byte> b, byte c) {
    foreach (var i in range(len(b))) {
        b[i] = c;
    }

    return len(b);
}

internal static uintptr memcpy(uintptr dest, uintptr src, uintptr n) {
    if (n != 0) {
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, (nint)(n), (nint)(n), 1125899906842623), @unsafe.Pointer.View<byte>((@unsafe.Pointer)src, 0, (nint)(n), (nint)(n), 1125899906842623));
    }

    return dest;
}

internal static void nativeMemory() {
    @string xs = "XXXXXXXXXXXXXXXX";
    var q = native("abcdefghijklmnop");

    var p = native(xs);
    @string s = "hello";
    nint n = len(s);
    nint c = copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623), s);
    fmt.Println("string into a view      :", c, peek(p, 8));

    p = native(xs);
    n = 6;
    c = copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623), @unsafe.Pointer.View<byte>((@unsafe.Pointer)q, 0, n, n, 1125899906842623));
    fmt.Println("view into a view        :", c, peek(p, 8));

    p = native(xs);
    var wd = slice<byte>("/tmp"u8);
    c = @unsafe.Pointer.CopyInto<byte>((@unsafe.Pointer)p, 0, 1125899906842623, wd);
    fmt.Println("open destination        :", c, peek(p, 8));
    var z = native("");
    c = @unsafe.Pointer.CopyInto<byte>((@unsafe.Pointer)z, 0, 1125899906842623, wd);
    fmt.Println("open destination, zeros :", c, peek(z, 8));

    p = native(xs);
    c = @unsafe.Pointer.CopyInto((@unsafe.Pointer)p, 2, 1125899906842623, "lo"u8);
    fmt.Println("open destination from 2 :", c, peek(p, 8));

    var v = new slice<byte>(6);
    c = @unsafe.Pointer.CopyFrom<byte>(v, (@unsafe.Pointer)q, 0, 1125899906842623);
    fmt.Printf("open source             : %d %q\n"u8, c, v);
    c = @unsafe.Pointer.CopyFrom<byte>(v, (@unsafe.Pointer)q, 10, 1125899906842623);
    fmt.Printf("open source from 10     : %d %q\n"u8, c, v);

    p = native(xs);
    var b = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, 5, 1125899906842623);

    foreach (var i in range(len(b))) {
        b[i] = 0;
    }

    b[0] = (byte)'u';
    b = b[1..];
    fmt.Println("element writes, capacity:", len(b), cap(b) == unchecked((nint)(1125899906842622L)), peek(p, 8));

    p = native(xs);
    fmt.Println("filled by a callee      :", fill(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, 4, 1125899906842623), (byte)'r'), peek(p, 8));

    c = copy(v, @unsafe.Pointer.View<byte>((@unsafe.Pointer)q, 0, 6, 6, 1125899906842623));
    fmt.Printf("view as a source        : %d %q\n"u8, c, v);

    var k = native("abcdefghijklmnop");
    var mem = @unsafe.Pointer.View<byte>((@unsafe.Pointer)k, 0, 4, 4, 1125899906842623);
    poke(k, "WXYZ");
    mem[3] = (byte)'!';
    fmt.Printf("memory changed later    : %q %s\n"u8, mem, peek(k, 4));

    p = native(xs);
    array<byte> r = new(16);

    foreach (var i in range(len(r))) {
        r[i] = (byte)('0' + i % 10);
    }

    c = copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, (nint)(/* unsafe.Sizeof(r) */ (uintptr)16), 1125899906842623), r[..]);
    fmt.Println("uintptr bound           :", c, peek(p, 16));

    p = native(xs);
    var whole = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 60, 64);
    whole[0] = (byte)'s';
    fmt.Println("open form of a small one:", len(whole), cap(whole), peek(p + 60, 4));

    var t = @unsafe.Pointer.View<byte>((@unsafe.Pointer)q, 2, 5, 9, 1125899906842623);
    fmt.Printf("low bound and max       : %d %d %q\n"u8, len(t), cap(t), t);

    fmt.Println("size of the array type  :", (uint64)/* unsafe.Sizeof(RawMem{}) */ (uintptr)1125899906842623);
}

internal static void goMemory() {
    ref var buf = ref builtin.heap(new array<byte>(16), out var Ꮡbuf);
    nint c = copy(@unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡbuf.at<byte>(0)), 0, len(buf), len(buf), 1125899906842623), "errno 7\x00"u8);
    fmt.Printf("element of an array     : %d %q\n"u8, c, buf[..8]);

    var h = Ꮡ(new hdr());
    var v = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(h.at(hdr.Ꮡbuf, 0)), 0, 12, 12, 1125899906842623);
    runtime.GC();
    copy(v, "field"u8);
    fmt.Printf("field, after a GC       : %q\n"u8, h.buf[..6]);

    var heap = new slice<byte>(32);
    var w = @unsafe.Pointer.View<byte>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(heap, 0)), 4), 0, 8, 8, 1125899906842623);
    runtime.GC();
    copy(w, "interior"u8);
    fmt.Printf("unsafe.Add, after a GC  : %q\n"u8, heap[..14]);

    ref var name = ref builtin.heap(new array<byte>(16), out var Ꮡname);
    nint n = @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡname.at<byte>(0)), 0, 1125899906842623, "name\x00"u8);
    fmt.Printf("open destination        : %d %q\n"u8, n, name[..6]);
    array<byte> @out = new(4);
    nint m = @unsafe.Pointer.CopyFrom<byte>(@out[..], @unsafe.Pointer.FromPinnedBox(Ꮡname.at<byte>(0)), 0, 1125899906842623);
    fmt.Printf("open source             : %d %q\n"u8, m, @out[..]);
    nint k = @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡname.at<byte>(0)), 12, 1125899906842623, "wxyz"u8);
    fmt.Printf("open destination from 12: %d %q\n"u8, k, name[10..]);

    ref var dst = ref builtin.heap(new array<byte>(8), out var Ꮡdst);
    memcpy((uintptr)Ꮡdst, (uintptr)Ꮡmagic, 8);
    fmt.Println("array's own number      :", dst);
    ref var half = ref builtin.heap(new array<byte>(8), out var Ꮡhalf);
    memcpy((uintptr)Ꮡhalf.at<byte>(4), (uintptr)Ꮡmagic.at<uint8>(2), 4);
    fmt.Println("element's number        :", half);

    ref var words = ref builtin.heap(new array<byte>(16), out var Ꮡwords);
    memcpy((uintptr)Ꮡwords, (uintptr)ᏑchachaInit, 16);
    fmt.Println("source of another type  :", words);
}

internal static void bounds() {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("bounds.func1")] () => {
            fmt.Println("past the array's length :", recover());
        }, ref ᒐ);

        var p = native("");
        nint n = 65;
        _ = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, 64);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    nativeMemory();
    goMemory();
    bounds();
}

} // end main_package

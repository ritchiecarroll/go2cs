global using ptr = go.unsafe_package.Pointer;
global using ui = go.uintptr;

namespace go;

using binary = encoding.binary_package;
using fmt = fmt_package;
using @unsafe = unsafe_package;
using encoding;

partial class main_package {

partial struct str {
    internal ptr p;
    internal nuint l;
}

[GoLazy] partial class lazyᴛkey { internal static ptr key = new ptr(Ꮡ(new uint8[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}.array())); }

partial struct hasher {
    internal uint64 acc;
    internal array<byte> buf = new(16);
}

internal static uint8 readU8(ptr p, ui o) { return ~(ж<uint8>)(uintptr)((ptr)(((ui)p) + o)); }

internal static uint64 readU64(ptr p, ui o) {
    var b = (ж<array<byte>>)(uintptr)((ptr)(((ui)p) + o));
    return binary.LittleEndian.Uint64(b.slice());
}

internal static void writeU64(ptr p, ui o, uint64 v) {
    var b = (ж<array<byte>>)(uintptr)((ptr)(((ui)p) + o));
    for (nint i = 0; i < 8; i++) {
        b.Value[i] = (byte)(v.Rsh((int64)(8 * i)));
    }
}

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: panic: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        fmt.Printf("%s: %s\n"u8, name, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ref var s = ref heap<@string>("abcdefghijklmnop", out var Ꮡs);
    ref var b = ref heap<slice<byte>>(slice<byte>("ABCDEFGHIJKLMNOP"u8), out var Ꮡb);

    @try("string header", () => {
        var h = ~Ꮡs.Reinterpret<@string, str>();
        return fmt.Sprint(h.l, readU8(h.p, 0), readU8(h.p, 15), readU64(h.p, 8));
    });
    @try("slice header", () => {
        var h = ~Ꮡb.Reinterpret<slice<byte>, str>();
        return fmt.Sprint(h.l, readU8(h.p, 1), readU64(h.p, ((ui)h.l) - 8));
    });
    @try("slice as string", () => ~Ꮡb.Reinterpret<slice<byte>, @string>());
    @try("string data word", () => fmt.Sprint(readU8(~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡs)), 2)));
    @try("pointer step", () => {
        var h = ~Ꮡs.Reinterpret<@string, str>();
        ptr q = (ptr)(((ui)h.p) + 4);
        q = (ptr)(((ui)q) + 4);
        return fmt.Sprint(readU8(q, 0));
    });
    @try("package array reads", () => fmt.Sprint(readU8(key, 0), readU8(key, 15), readU64(key, 8)));
    @try("package array view length", () => fmt.Sprint(len((ж<array<byte>>)(uintptr)(key).Value), ((ж<array<byte>>)(uintptr)(key)).Value[3]));
    @try("fresh array writes", () => {
        ptr secret = new ptr(Ꮡ(new byte[]{}.array(16)));
        writeU64(secret, 0, 0x0102030405060708UL);
        writeU64(secret, 8, 0x1112131415161718UL);
        return fmt.Sprint(readU8(secret, 0), readU8(secret, 8), readU64(secret, 8));
    });
    @try("struct field array", () => {
        ref var h = ref heap(new hasher(), out var Ꮡh);
        writeU64(new ptr(Ꮡh.Ꮡbuf), 8, 42);
        return fmt.Sprint(h.buf[8], len((ж<array<byte>>)(uintptr)(new ptr(Ꮡh.Ꮡbuf)).Value));
    });
}

} // end main_package

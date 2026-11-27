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

partial struct bytes /*[]byte*/;

[GoLazy] partial class lazyᴛkey { internal static ptr key = ptr.FromPinnedBox(Ꮡ(array<uint8>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]))); }

partial struct hasher {
    internal uint64 acc;
    internal array<byte> buf = new(16);
}

internal static uint8 readU8(ptr p, ui o) { return ~@unsafe.Pointer.To<uint8>(@unsafe.Add(p, o)); }

internal static uint64 readU64(ptr p, ui o) {
    var b = @unsafe.Pointer.ToArray<byte>(@unsafe.Add(p, o), 8);
    return binary.LittleEndian.Uint64(b.slice());
}

internal static void writeU64(ptr p, ui o, uint64 v) {
    var b = @unsafe.Pointer.ToArray<byte>(@unsafe.Add(p, o), 8);
    for (nint i = 0; i < 8; i++) {
        b.ElementRef(i) = (byte)(v.Rsh((int64)(8 * i)));
    }
}

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: panic: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        var evalᴛ1 = f();
        fmt.Printf("%s: %s\n"u8, name, evalᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ref var s = ref heap<@string>("abcdefghijklmnop", out var Ꮡs);
    ref var b = ref heap<slice<byte>>(slice<byte>("ABCDEFGHIJKLMNOP"u8), out var Ꮡb);

    @try("string header", [GoFunc("main.func1")] () => {
        var h = ~Ꮡs.Reinterpret<@string, str>();
        return fmt.Sprint(h.l, readU8(h.p, 0), readU8(h.p, 15), readU64(h.p, 8));
    });
    @try("slice header", [GoFunc("main.func2")] () => {
        var h = ~Ꮡb.Reinterpret<slice<byte>, str>();
        return fmt.Sprint(h.l, readU8(h.p, 1), readU64(h.p, ((ui)h.l) - 8));
    });
    @try("slice as string", [GoFunc("main.func3")] () => ~Ꮡb.Reinterpret<slice<byte>, @string>());
    @try("string data word", [GoFunc("main.func4")] () => fmt.Sprint(readU8(~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.StringData(Ꮡs.Value))), 2)));
    @try("pointer step", [GoFunc("main.func5")] () => {
        var h = ~Ꮡs.Reinterpret<@string, str>();
        ptr q = @unsafe.Add(h.p, 4);
        q = @unsafe.Add(q, 4);
        return fmt.Sprint(readU8(q, 0));
    });
    @try("uint length store", [GoFunc("main.func6")] () => {
        ref var s2 = ref heap<@string>("abcdefgh", out var Ꮡs2);
        var hp = Ꮡs2.Reinterpret<@string, str>();
        hp.l = 3;
        return s2;
    });
    @try("slice header after reslice", [GoFunc("main.func7")] () => {
        ref var b2 = ref heap<slice<byte>>(Ꮡb.ValueSlot[2..6], out var Ꮡb2);
        var h = ~Ꮡb2.Reinterpret<slice<byte>, str>();
        return fmt.Sprint(h.l, readU8(h.p, 0), readU8(h.p, 3));
    });
    @try("wide slice header", [GoFunc("main.func8")] () => {
        ref var w = ref heap<slice<uint32>>(slice<uint32>([0x04030201, 0x08070605, 0x0c0b0a09]), out var Ꮡw);
        var h = ~Ꮡw.Reinterpret<slice<uint32>, str>();
        return fmt.Sprint(h.l, readU8(h.p, 4), readU64(h.p, 4));
    });
    @try("slice as string sees writes", [GoFunc("main.func9")] () => {
        ref var c = ref heap<slice<byte>>(slice<byte>("xyz"u8), out var Ꮡc);
        var v = Ꮡc.Reinterpret<slice<byte>, @string>();
        c[0] = (byte)'X';
        c = c[..2];
        return v.Value;
    });
    @try("data words equal header words", [GoFunc("main.func10")] () => {
        var h = ~Ꮡs.Reinterpret<@string, str>();
        var hb = ~Ꮡb.Reinterpret<slice<byte>, str>();
        ref var e = ref heap<@string>("", out var Ꮡe);
        ref var nb = ref heap<bytes>(out var Ꮡnb);

        nb = slice<byte>("wxyz"u8);
        return fmt.Sprint(h.p == ~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.StringData(Ꮡs.Value))), hb.p == ~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.SliceData(Ꮡb.ValueSlot))), readU8(~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.SliceData(Ꮡb.ValueSlot))), 3), ~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.StringData(e))) == default, readU8(~Ꮡ(@unsafe.Pointer.FromBox(@unsafe.SliceData<byte>(nb))), 1));
    });
    @try("package array reads", [GoFunc("main.func11")] () => fmt.Sprint(readU8(key, 0), readU8(key, 15), readU64(key, 8)));
    @try("package array view length", [GoFunc("main.func12")] () => fmt.Sprint(4, @unsafe.Pointer.ElementRef<byte>(key, 3, 4)));
    @try("fresh array writes", [GoFunc("main.func13")] () => {
        ptr secret = ptr.FromPinnedBox(Ꮡ(new byte[]{}.array(16)));
        writeU64(secret, 0, 0x0102030405060708UL);
        writeU64(secret, 8, 0x1112131415161718UL);
        return fmt.Sprint(readU8(secret, 0), readU8(secret, 8), readU64(secret, 8));
    });
    @try("struct field array", [GoFunc("main.func14")] () => {
        ref var h = ref heap(new hasher(), out var Ꮡh);
        writeU64(ptr.FromPinnedBox(Ꮡh.Ꮡbuf), 8, 42);
        return fmt.Sprint(h.buf[8], 8);
    });
}

} // end main_package

global using uuidT = go.array<byte>;

namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class main_package {

/* [1125899906842623L]byte */
partial struct RawMem /*[1125899906842623]byte*/;

partial struct smallMem /*[64]byte*/;

partial struct pair {
    internal int32 a;
    internal int32 b;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 4), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct hdr {
    [FieldOffset(0)] internal uint32 a;
    [FieldOffset(4)] internal uint16 b;
    [FieldOffset(6)] internal uint16 c;
    [FieldOffset(8)] internal array8<byte> buf;
}

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

internal static @unsafe.Pointer ptr(uintptr p) {
    return (@unsafe.Pointer)p;
}

internal static int32 twice(int32 v) {
    return v * 2;
}

internal static byte last(/*[4]*/ array<byte> v) {
    v = v.Clone();

    return v[3];
}

internal partial struct nativeMemory_sixteen /*[16]byte*/;

internal static void nativeMemory() {
    @string xs = "XXXXXXXXXXXXXXXX";

    var p = native(xs);
    var wd = slice<byte>("/tmp"u8);
    @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)p, len(wd), 1125899906842623) = 0;
    fmt.Println("index store, huge array :", peek(p, 8));

    var z = native("");
    @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)z, 4, 1125899906842623) = 7;
    fmt.Println("index store, zeros      :", peek(z, 8));

    p = native(xs);
    @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)p, 5, 64) = 0;
    var evalᴛ1 = peek(p, 8);
    fmt.Println("index store, small array:", @unsafe.Pointer.ElementRef<byte>((@unsafe.Pointer)p, 4, 64), evalᴛ1);

    p = native("");
    @unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4) = 7;
    var evalᴛ2 = ptr(p);
    var evalᴛ3 = ptr(p);
    var evalᴛ4 = peek(p, 8);
    fmt.Println("store, then read        :", @unsafe.Pointer.ElementRef<int32>(evalᴛ2, 1, 4), @unsafe.Pointer.ElementRef<int32>(evalᴛ3, 0, 4), evalᴛ4);

    @unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4) += 3;
    @unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4)++;
    @unsafe.Pointer.ElementRef<int32>(ptr(p), 2, 4) |= (int32)(0x0102);
    var evalᴛ5 = ptr(p);
    var evalᴛ6 = peek(p + 8, 4);
    fmt.Println("compound and increment  :", @unsafe.Pointer.ElementRef<int32>(evalᴛ5, 1, 4), evalᴛ6);

    var bound = @unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4);
    @unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4) = 100;
    fmt.Println("argument and bound      :", twice(@unsafe.Pointer.ElementRef<int32>(ptr(p), 1, 4)), bound);

    uint64 wide = 3;
    uintptr off = 2;
    @unsafe.Pointer.ElementRef<int32>(ptr(p), (nint)(wide), 4) = -1;
    var evalᴛ7 = ptr(p);
    var evalᴛ8 = peek(p + 12, 4);
    fmt.Println("wide index              :", @unsafe.Pointer.ElementRef<int32>(evalᴛ7, (nint)(off + 1), 4), evalᴛ8);

    p = native("");
    var e = @unsafe.Pointer.Element<int32>(ptr(p), 2, 4);
    e.Value = 9;
    var next = @unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(e), 4));
    next.Value = 10;
    var evalᴛ9 = ptr(p);
    fmt.Println("address of an element   :", ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(p + 8)), @unsafe.Pointer.ElementRef<int32>(evalᴛ9, 3, 4), (uintptr)e == p + 8);

    p = native("");
    @unsafe.Pointer.ElementRef<pair>(ptr(p), 1, 3).a = 5;
    @unsafe.Pointer.ElementRef<pair>(ptr(p), 1, 3).b = @unsafe.Pointer.ElementRef<pair>(ptr(p), 1, 3).a + 1;
    var whole = @unsafe.Pointer.ElementRef<pair>(ptr(p), 1, 3);
    fmt.Println("struct element          :", whole, peek(p + 8, 8));

    p = native(xs);
    @unsafe.Pointer.StoreArray<byte>((@unsafe.Pointer)p, 2, array<byte>([(byte)'C', 0]));
    fmt.Println("whole array stored      :", peek(p, 8));

    p = native("abcdefgh");
    var v = @unsafe.Pointer.LoadArray<byte>(ptr(p), 4);
    poke(p, "WXYZ");
    fmt.Printf("whole array loaded      : %q %s %q\n"u8, v, peek(p, 4), last(@unsafe.Pointer.LoadArray<byte>(ptr(p), 4)));

    var q = native("12345678");
    @unsafe.Pointer.StoreArray<byte>(ptr(q + 4), 4, @unsafe.Pointer.LoadArray<byte>(ptr(p), 4));
    fmt.Println("stored from a load      :", peek(q, 8));
    var evalᴛ10 = ptr(q + 4);
    var evalᴛ11 = ptr(p);
    var evalᴛ12 = @unsafe.Pointer.LoadArray<byte>(evalᴛ10, 4) == @unsafe.Pointer.LoadArray<byte>(evalᴛ11, 4);
    var evalᴛ13 = ptr(q);
    var evalᴛ14 = ptr(p);
    fmt.Println("two compared            :", evalᴛ12, @unsafe.Pointer.LoadArray<byte>(evalᴛ13, 4) != @unsafe.Pointer.LoadArray<byte>(evalᴛ14, 4));

    p = native(xs);
    @unsafe.Pointer.ElementRef<uint64>(ptr(p), 0, 2) = 0;
    fmt.Println("one word cleared        :", peek(p, 16));
    @unsafe.Pointer.StoreArray<uint64>(ptr(p), 2, new uint64[]{}.array(2));
    fmt.Println("both words cleared      :", peek(p, 16));

    var nm = native(xs);
    nativeMemory_sixteen id = default;

    foreach (var i in range(len(id))) {
        id[i] = (byte)('a' + i);
    }

    @unsafe.Pointer.StoreArray<byte>(ptr(nm), 16, id);
    var back = (nativeMemory_sixteen)@unsafe.Pointer.LoadArray<byte>(ptr(nm), 16);
    back[0] = (byte)'!';
    fmt.Println("named array, whole      :", peek(nm, 17), (@string)(back[..4]), (nativeMemory_sixteen)@unsafe.Pointer.LoadArray<byte>(ptr(nm), 16) == id);

    var dst = native(xs);
    @unsafe.Pointer.StoreArray<uint8>((@unsafe.Pointer)dst, 16, @unsafe.Pointer.LoadArray<uint8>((@unsafe.Pointer)nm, 16));
    fmt.Println("alias of an array, whole:", peek(dst, 17));

    var c = native("");
    uint32 sum = default;

    for (nint i = 0; i < 1000; i++) {
        @unsafe.Pointer.ElementRef<uint32>(ptr(c), i, 1048576) = (uint32)i;
        sum += @unsafe.Pointer.ElementRef<uint32>(ptr(c), i, 1048576);
    }

    fmt.Println("loop of stores and reads:", sum, @unsafe.Pointer.ElementRef<uint32>(ptr(c), 999, 1048576));
}

internal static void goMemory() {
    ref var arr = ref heap(new array<int32>(4), out var Ꮡarr);
    var n = (uintptr)Ꮡarr;
    @unsafe.Pointer.ElementRef<int32>(ptr(n), 2, 4) = 5;
    arr[3] = 6;
    var evalᴛ1 = ptr(n);
    fmt.Println("array by its number     :", arr, @unsafe.Pointer.ElementRef<int32>(evalᴛ1, 3, 4));

    ref var buf = ref heap(new array<byte>(8), out var Ꮡbuf);
    var m = (uintptr)Ꮡbuf.at<byte>(4);
    @unsafe.Pointer.ElementRef<byte>(ptr(m), 1, 4) = (byte)'e';
    fmt.Printf("element by its number   : %q\n"u8, buf);

    var x = @new<uint64>();
    x.Value = 0x0102030405060708UL;
    @unsafe.Pointer xp = @unsafe.Pointer.FromPinnedBox(x);
    runtime.GC();
    @unsafe.Pointer.ElementRef<byte>(xp, 0, 8) = 0xff;
    fmt.Printf("uint64 as bytes         : %#x %#x\n"u8, x.Value, @unsafe.Pointer.ElementRef<byte>(xp, 7, 8));

    var h = Ꮡ(new hdr(a: 1, b: 2, c: 3));
    @unsafe.Pointer hp = @unsafe.Pointer.FromPinnedBox(h);
    @unsafe.Pointer.ElementRef<byte>(hp, 4, 16) = 9;
    @unsafe.Pointer.ElementRef<uint32>(hp, 0, 4) += 10;
    fmt.Println("struct as bytes, words  :", h.a, h.b, h.c, @unsafe.Pointer.ElementRef<uint16>(hp, 3, 8));

    var e = @unsafe.Pointer.Element<uint16>(hp, 3, 8);
    runtime.GC();
    e.Value = 77;
    fmt.Println("address, after a GC     :", h.c);

    var y = @new<uint32>();
    @unsafe.Pointer yp = @unsafe.Pointer.FromPinnedBox(y);
    @unsafe.Pointer.StoreArray<byte>(yp, 4, array<byte>([1, 2, 3, 4]));
    var stored = y.Value;
    var loaded = @unsafe.Pointer.LoadArray<byte>(yp, 4);
    y.Value = 0;
    fmt.Printf("whole array, a uint32   : %#x %v\n"u8, stored, loaded);

    ref var dst = ref heap(new array<int32>(4), out var Ꮡdst);
    @unsafe.Pointer.StoreArray<int32>(ptr((uintptr)Ꮡdst), 4, array<int32>([1, 2, 3, 4]));
    fmt.Println("whole array, by number  :", dst);
}

internal static void lengths() {
    var p = native("0123456789");
    @unsafe.Pointer n = ptr(p);

    ref var arr = ref heap(new array<byte>(64), out var Ꮡarr);

    foreach (var i in range(len(arr))) {
        arr[i] = (byte)(i + 1);
    }

    @unsafe.Pointer g = @unsafe.Pointer.FromPinnedBox(Ꮡarr);

    @unsafe.Pointer none = default;

    fmt.Println("len, cap native         :", 4, 4, 4, 64);
    fmt.Println("len, cap Go, 64 behind  :", 4, 4, 4);
    fmt.Println("len, cap nil            :", 4, 4);
    fmt.Println("len with a call         :", len(@unsafe.Pointer.LoadArray<byte>(ptr(p), 4)));

    nint sum = 0;
    nint rounds = 0;

    foreach (var i in range(4)) {
        rounds++;
        sum += i;
    }

    fmt.Println("range index, 64 behind  :", rounds, sum);

    rounds = 0;

    foreach (var _ᴛ1 in range(4)) {
        rounds++;
    }

    fmt.Println("range index, nil        :", rounds);

    (sum, rounds) = (0, 0);

    foreach (var (_, b) in range(@unsafe.Pointer.LoadArray<byte>(g, 4))) {
        rounds++;
        sum += (nint)b;
    }

    fmt.Println("range values, 64 behind :", rounds, sum);

    (sum, rounds) = (0, 0);

    foreach (var (i, b) in range(@unsafe.Pointer.LoadArray<byte>(n, 4))) {
        @unsafe.Pointer.ElementRef<byte>(n, 3, 4) = (byte)'z';
        rounds++;
        sum += i + (nint)((byte)(b - '0'));
    }

    fmt.Println("range values, native    :", rounds, sum, peek(p, 4));

    (sum, rounds) = (0, 0);

    foreach (var (i, b) in range(@unsafe.Pointer.View<byte>(g, 0, 4, 4))) {
        if (i == 0) {
            @unsafe.Pointer.ElementRef<byte>(g, 3, 4) = 50;
        }

        rounds++;
        sum += (nint)b;
    }

    fmt.Println("range pointer, 64 behind:", rounds, sum, arr[3]);

    (sum, rounds) = (0, 0);

    foreach (var (_, b) in range(@unsafe.Pointer.View<byte>(n, 0, 4, 4))) {
        rounds++;
        sum += (nint)((byte)(b - '0'));
    }

    fmt.Println("range pointer, native   :", rounds, sum);
}

internal static void pastTheEnd() {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("pastTheEnd.func1")] () => {
            fmt.Println("index past the length   :", recover());
        }, ref ᒐ);

        var p = native("");
        nint i = 4;
        @unsafe.Pointer.ElementRef<int32>(ptr(p), i, 4) = 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void negative() {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("negative.func1")] () => {
            fmt.Println("negative index          :", recover());
        }, ref ᒐ);

        var p = native("");
        nint i = -1;
        _ = @unsafe.Pointer.ElementRef<int32>(ptr(p), i, 4);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void nilPointer() {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("nilPointer.func1")] () => {
            fmt.Println("nil pointer             :", recover());
        }, ref ᒐ);

        @unsafe.Pointer p = default;
        @unsafe.Pointer.ElementRef<int32>(p, 1, 4) = 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    nativeMemory();
    goMemory();
    lengths();
    pastTheEnd();
    negative();
    nilPointer();
}

} // end main_package

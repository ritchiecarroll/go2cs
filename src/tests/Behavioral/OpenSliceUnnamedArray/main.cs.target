namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void arm(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("arm.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Println(name, "panic:", r);
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint takes(slice<byte> b) {
    nint n = 0;
    foreach (var (_, x) in range(b)) {
        n += (nint)x;
    }
    return n + 100 * len(b);
}

internal partial struct main__ᴛ1 /*dyn*/ {
    internal uint32 lo;
    internal uint32 hi;
}

internal static void main() {
    arm("A", [GoFunc("main.func1")] () => {
        array<main__ᴛ1> _ᴛ1 = new(3);
        ref var buf = ref heap(new array<uint64>(3), out var Ꮡbuf);
        @unsafe.Pointer bp = @unsafe.Pointer.FromPinnedBox(Ꮡbuf);
        var tail = @unsafe.Pointer.View<main__ᴛ1>(bp, 1, 3);
        tail[0].hi = 7;
        fmt.Println("A", len(tail), cap(tail), buf[1] >> 32);
    });

    arm("B", [GoFunc("main.func2")] () => {
        ref var arr = ref heap(new array<uint32>(4), out var Ꮡarr);
        var w = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 0, 16);
        w[4] = 1;
        fmt.Println("B", len(w), arr);
    });

    arm("C", [GoFunc("main.func3")] () => {
        ref var nameOff = ref heap(new int32(), out var ᏑnameOff);
        ref var src = ref heap<array<byte>>(array<byte>([0, 1, 2, 3, 4, 5, 6, 7]), out var Ꮡsrc);
        @unsafe.Pointer off = @unsafe.Pointer.FromPinnedBox(Ꮡsrc.at<byte>(2));
        nint n = @unsafe.Pointer.CopyInto<byte>(@unsafe.Pointer.FromPinnedBox(ᏑnameOff), 0, 4, @unsafe.Pointer.View<byte>(off, 0, 4));
        fmt.Println("C", n, nameOff);
    });

    arm("D", [GoFunc("main.func4")] () => {
        ref var u = ref heap<uint32>((uint32)0x01020304, out var Ꮡu);
        fmt.Println("D", takes(@unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡu), 0, 4)));
    });

    arm("E", [GoFunc("main.func5")] () => {
        ref var a = ref heap(new array<byte>(4), out var Ꮡa);
        var s = Ꮡa.slice(1);
        s[0] = 9;
        fmt.Println("E", len(s), cap(s), a);
    });

    arm("F", [GoFunc("main.func6")] () => {
        ref var u = ref heap<uint64>((uint64)0x0807060504030201UL, out var Ꮡu);
        nint sum = 0;
        foreach (var (_, b) in range(@unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡu), 2, 8))) {
            sum += (nint)b;
        }
        fmt.Println("F", sum);
    });

    arm("G", [GoFunc("main.func7")] () => {
        ref var words = ref heap(new array<uint16>(2), out var Ꮡwords);
        var b = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡwords), 2, 4);
        (b[0], b[1]) = (0x34, 0x12);
        fmt.Printf("G %d %#x\n"u8, len(b), words[1]);
    });
}

} // end main_package

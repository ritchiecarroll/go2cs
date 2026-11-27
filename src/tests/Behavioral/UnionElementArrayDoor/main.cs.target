global using Tu32 = uint;
global using TRtreeCoord = go.main_package.TRtreeCoordᴛ1;
global using TExpr = go.main_package.TExprᴛ1;
global using TAddr = go.main_package.TAddrᴛ1;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class main_package {

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] partial struct TRtreeCoordᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int Fi;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tu32>))] public readonly int Fu;
    [FieldOffset(0)] public float32 Ff;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int FiValue;
    [FieldOffset(0)] public uintptr FzToken;
}

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] partial struct TExprᴛ1_Fw /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int FiOfst;
    [FieldOffset(0)] public int32 FiJoin;
}

[global::go.golib.GoInlineLayout(Size = 24, Align = 8), StructLayout(LayoutKind.Explicit, Size = 24)] partial struct TExprᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uint8 Fop;
    [FieldOffset(8)] public TExprᴛ1_Fu Fu;
    [FieldOffset(16)] public TExprᴛ1_Fw Fw;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct TAddrᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint64>))] public readonly long Fa;
    [FieldOffset(0)] public array16<byte> Fb;
}

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

internal static float32 readCoord(@unsafe.Pointer p, nint i) {
    return @unsafe.Pointer.ElementRef<TRtreeCoord>(p, i, 4).Ff;
}

internal static void writeCoord(@unsafe.Pointer p, nint i, float32 f) {
    @unsafe.Pointer.ElementRef<TRtreeCoord>(p, i, 4).Ff = f;
}

internal static int32 joinOf(@unsafe.Pointer p, nint i) {
    return @unsafe.Pointer.ElementRef<TExpr>(p, i, 3).Fw.FiJoin;
}

internal static void setJoin(@unsafe.Pointer p, nint i, int32 j) {
    @unsafe.Pointer.ElementRef<TExpr>(p, i, 3).Fw.FiJoin = j;
}

internal static void main() {
    arm("A", [GoFunc("main.func1")] () => {
        ref var raw = ref heap(new array<uint64>(2), out var Ꮡraw);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡraw);
        writeCoord(p, 3, 2.25F);
        fmt.Printf("A %v %#x\n"u8, readCoord(p, 3), raw[1] >> 32);
    });

    arm("B", [GoFunc("main.func2")] () => {
        ref var raw = ref heap(new array<uint64>(2), out var Ꮡraw);
        raw[0] = 0x3fc00000;
        fmt.Println("B", readCoord(@unsafe.Pointer.FromPinnedBox(Ꮡraw), 0));
    });

    arm("C", [GoFunc("main.func3")] () => {
        ref var words = ref heap(new array<uint64>(9), out var Ꮡwords);
        @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(Ꮡwords);
        setJoin(q, 2, 9);
        fmt.Println("C", joinOf(q, 2), words[8] & 0xffffffffU);
    });

    arm("D", [GoFunc("main.func4")] () => {
        ref var words = ref heap(new array<uint64>(9), out var Ꮡwords);
        words[5] = 7;
        fmt.Println("D", joinOf(@unsafe.Pointer.FromPinnedBox(Ꮡwords), 1), /* unsafe.Sizeof(TExpr{}) */ (uintptr)24);
    });

    arm("E", [GoFunc("main.func5")] () => {
        ref var words = ref heap(new array<uint64>(4), out var Ꮡwords);
        @unsafe.Pointer.ElementRef<TAddr>(@unsafe.Pointer.FromPinnedBox(Ꮡwords), 1, 2).Fb[3] = 0x5a;
        fmt.Printf("E %#x %d\n"u8, words[2], /* unsafe.Sizeof(TAddr{}) */ (uintptr)16);
    });

    arm("F", [GoFunc("main.func6")] () => {
        ref var arr = ref heap(new array<TRtreeCoord>(4, () => new()), out var Ꮡarr);
        writeCoord(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 1, 4.5F);
        var evalᴛ1 = readCoord(@unsafe.Pointer.FromPinnedBox(Ꮡarr), 1);
        fmt.Println("F", arr[1].Ff, evalᴛ1);
    });

    arm("G", [GoFunc("main.func7")] () => {
        ref var raw = ref heap(new array<uint64>(2), out var Ꮡraw);
        var s = @unsafe.Pointer.View<TRtreeCoord>(@unsafe.Pointer.FromPinnedBox(Ꮡraw), 1, 4);
        s[1].Ff = 3.5F;
        fmt.Printf("G %d %d %#x\n"u8, len(s), cap(s), raw[1] & 0xffffffffU);
    });
}

} // end main_package

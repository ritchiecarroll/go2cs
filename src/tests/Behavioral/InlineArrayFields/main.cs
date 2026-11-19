namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class main_package {

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] partial struct Plain {
    [FieldOffset(0)] public int32 N;
}

[global::go.golib.GoInlineLayout(Size = 12, Align = 4), StructLayout(LayoutKind.Explicit, Size = 12)] partial struct Holder {
    [FieldOffset(0)] public Plain P;
    [FieldOffset(4)] public array8<byte> Buf;
}

[global::go.golib.GoInlineLayout(Size = 6, Align = 2), StructLayout(LayoutKind.Explicit, Size = 6)] partial struct S22 {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int16>))] public readonly short FforceAlignment;
    [FieldOffset(0)] public array5<int8> Fa;
    [FieldOffset(5)] public array1<byte> Fpad;
}

[global::go.golib.GoInlineLayout(Size = 12, Align = 4), StructLayout(LayoutKind.Explicit, Size = 12)] partial struct Named {
    [FieldOffset(0)] public uint8 FnName;
    [FieldOffset(1)] public array7<int8> FzName;
    [FieldOffset(8)] public float32 FrLimit;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 2), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Grid {
    [FieldOffset(0)] public array2<array3<int16>> Cells;
    [FieldOffset(12)] public array4<uint8> Off;
}

internal static ж<Holder> view(@unsafe.Pointer p) { return @unsafe.Pointer.To<Holder>(p); }

internal static ж<S22> s22(@unsafe.Pointer p) { return @unsafe.Pointer.To<S22>(p); }

internal static ж<Named> named(@unsafe.Pointer p) { return @unsafe.Pointer.To<Named>(p); }

internal static ж<Grid> grid(@unsafe.Pointer p) { return @unsafe.Pointer.To<Grid>(p); }

internal static any box(any v) { return v; }

internal static void main() {
    ref var raw = ref heap(new array<byte>(16), out var Ꮡraw);
    foreach (var i in range(len(raw))) {
        raw[i] = (byte)(i + 1);
    }
    var h = view(@unsafe.Pointer.FromPinnedBox(Ꮡraw.at<byte>(0)));
    nint n = 8 + 8;
    foreach (var i in range(8)) {
        h.Buf[i] += (byte)i;
    }
    foreach (var (i, bΔ1) in range(h.Buf.ΔRangeSnapshot())) {
        n += i * (nint)bΔ1;
    }
    h.Buf[2] += 3;
    fmt.Println(n, h.P.N, h.Buf[0], h.Buf[7], raw[6], raw[11]);
    fmt.Println(h.Buf.ΔCopy());
    fmt.Printf("%v %d\n"u8, h.Buf.ΔCopy(), /* unsafe.Sizeof(*h) */ (uintptr)12);

    var s = s22(@unsafe.Pointer.FromPinnedBox(Ꮡraw.at<byte>(0)));
    s.Fa[1] = (int8)(-2);
    fmt.Println(/* unsafe.Sizeof(*s) */ (uintptr)6, /* unsafe.Alignof(*s) */ (uintptr)2, s.Fa[0], s.Fa[1], raw[1]);

    var x = new Named(FnName: 6, FzName: new array7<int8>((int8)'s', (int8)'e', (int8)'c', (int8)'o', (int8)'n', (int8)'d'), FrLimit: 4.5F);
    fmt.Println(x.FnName, x.FzName[0], x.FzName[5], x.FzName[6], x.FrLimit, /* unsafe.Sizeof(x) */ (uintptr)12);
    _ = (Func<@unsafe.Pointer, ж<Named>>)(named);

    var g = new Grid(Cells: new array2<array3<int16>>(new array3<int16>(1, 2, 3), new array3<int16>(4, 5, 6)), Off: new array4<uint8>(default, 7, 9));
    var b = box(g.Off.ΔCopy());
    g.Off[1] = 0;
    fmt.Println(b, g.Off.ΔCopy(), AreEqual(box(g.Cells.ΔSelect(static Δe1 => Δe1.ΔCopy())), box(new array<int16>[]{new int16[]{1, 2, 3}.array(), new int16[]{4, 5, 6}.array()}.array())));
    var switchᴛ1 = box(g.Cells[1].ΔCopy());
    switch (switchᴛ1.type()) {
    case array<int16> v: {
        fmt.Println("[3]int16", v[0], v[2]);
        break;
    }
    default: {
        var v = switchᴛ1;
        fmt.Println("other", v);
        break;
    }}
    var m = new map<any, nint>{[box(g.Cells[0].ΔCopy())] = 1};
    fmt.Println(m[new int16[]{1, 2, 3}.array()], reflect.ValueOf(g.Off.ΔCopy()).Cap(), reflect.ValueOf(g.Cells.ΔSelect(static Δe1 => Δe1.ΔCopy())).Index(1).Len());
    fmt.Printf("%T %v\n"u8, g.Cells.ΔSelect(static Δe1 => Δe1.ΔCopy()), g.Cells[1].ΔCopy());
    _ = (Func<@unsafe.Pointer, ж<Grid>>)(grid);
}

} // end main_package

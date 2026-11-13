namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class main_package {

[GoInlineLayout, StructLayout(LayoutKind.Explicit, Size = 4)] partial struct Plain {
    [FieldOffset(0)] public int32 N;
}

[GoInlineLayout, StructLayout(LayoutKind.Explicit, Size = 12)] partial struct Holder {
    [FieldOffset(0)] public Plain P;
    [FieldOffset(4)] public array8<byte> Buf;
}

[GoInlineLayout, StructLayout(LayoutKind.Explicit, Size = 6)] partial struct S22 {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int16>))] public readonly short FforceAlignment;
    [FieldOffset(0)] public array5<int8> Fa;
    [FieldOffset(5)] public array1<byte> Fpad;
}

[GoInlineLayout, StructLayout(LayoutKind.Explicit, Size = 12)] partial struct Named {
    [FieldOffset(0)] public uint8 FnName;
    [FieldOffset(1)] public array7<int8> FzName;
    [FieldOffset(8)] public float32 FrLimit;
}

internal static ж<Holder> view(@unsafe.Pointer p) { return (ж<Holder>)(uintptr)(p); }

internal static ж<S22> s22(@unsafe.Pointer p) { return (ж<S22>)(uintptr)(p); }

internal static ж<Named> named(@unsafe.Pointer p) { return (ж<Named>)(uintptr)(p); }

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
    foreach (var (i, b) in range(h.Buf.ΔRangeSnapshot())) {
        n += i * (nint)b;
    }
    h.Buf[2] += 3;
    fmt.Println(n, h.P.N, h.Buf[0], h.Buf[7], raw[6], raw[11]);
    fmt.Println(h.Buf);
    fmt.Printf("%v %d\n"u8, h.Buf, /* unsafe.Sizeof(*h) */ (uintptr)12);

    var s = s22(@unsafe.Pointer.FromPinnedBox(Ꮡraw.at<byte>(0)));
    s.Fa[1] = (int8)(-2);
    fmt.Println(/* unsafe.Sizeof(*s) */ (uintptr)6, /* unsafe.Alignof(*s) */ (uintptr)2, s.Fa[0], s.Fa[1], raw[1]);

    var x = new Named(FnName: 6, FzName: new array7<int8>((int8)'s', (int8)'e', (int8)'c', (int8)'o', (int8)'n', (int8)'d'), FrLimit: 4.5F);
    fmt.Println(x.FnName, x.FzName[0], x.FzName[5], x.FzName[6], x.FrLimit, /* unsafe.Sizeof(x) */ (uintptr)12);
    _ = (Func<@unsafe.Pointer, ж<Named>>)(named);
}

} // end main_package

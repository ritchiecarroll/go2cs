namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;

partial class main_package {

[global::go.golib.GoInlineLayout(Size = 1048, Align = 8), StructLayout(LayoutKind.Explicit, Size = 1048)] partial struct Dirent {
    [FieldOffset(0)] public uint64 Ino;
    [FieldOffset(8)] public uint64 Seekoff;
    [FieldOffset(16)] public uint16 Reclen;
    [FieldOffset(18)] public uint16 Namlen;
    [FieldOffset(20)] public uint8 Type;
    [FieldOffset(21)] public array1024<int8> Name;
    [FieldOffset(1045)] public array3<byte> Pad_cgo;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Pair {
    [FieldOffset(0)] public uint16 A;
    [FieldOffset(2)] public array2<uint8> B;
    [FieldOffset(8)] public uint64 C;
}

internal static void main() {
    var data = new slice<byte>((nint)(/* unsafe.Sizeof(Dirent{}) */ (uintptr)1048));
    data[0] = 7;
    data[16] = 32;
    data[18] = 3;
    (data[21], data[22], data[23]) = ((byte)'a', (byte)'b', (byte)'c');

    ref var dirent = ref heap(new Dirent(), out var Ꮡdirent);
    nint n = copy(@unsafe.GoImage(Ꮡdirent).slice(), data);
    fmt.Println(n, dirent.Ino, dirent.Reclen, dirent.Namlen, (@string)slice<byte>([(byte)dirent.Name[0], (byte)dirent.Name[1], (byte)dirent.Name[2]]));

    dirent.Seekoff = 99;
    n = copy(@unsafe.GoImage(Ꮡdirent).slice(), data[..10]);
    fmt.Println(n, dirent.Ino, dirent.Seekoff, dirent.Reclen);

    ref var p = ref heap<Pair>(new Pair(A: 0x0102, B: new array2<uint8>(5, 6), C: 0x1122334455667788UL), out var Ꮡp);
    var @out = new slice<byte>(16);
    n = copy(@out, @unsafe.GoImage(Ꮡp).slice());
    fmt.Println(n, @out);

    var q = Ꮡp;
    n = copy(@unsafe.GoImage(q).slice(8, 10), slice<byte>([0xAA, 0xBB, 0xCC]));
    fmt.Printf("%d %#x %v\n"u8, n, p.C, p.B.ΔCopy());
}

} // end main_package

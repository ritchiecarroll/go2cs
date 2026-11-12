namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Dirent {
    public uint64 Ino;
    public uint64 Seekoff;
    public uint16 Reclen;
    public uint16 Namlen;
    public uint8 Type;
    public array<int8> Name = new(1024);
    public array<byte> Pad_cgo = new(3);
}

partial struct Pair {
    public uint16 A;
    public array<uint8> B = new(2);
    public uint64 C;
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

    ref var p = ref heap<Pair>(new Pair(A: 0x0102, B: new uint8[]{5, 6}.array(), C: 0x1122334455667788UL), out var Ꮡp);
    var @out = new slice<byte>(16);
    n = copy(@out, @unsafe.GoImage(Ꮡp).slice());
    fmt.Println(n, @out);

    var q = Ꮡp;
    n = copy(@unsafe.GoImage(q).slice(8, 10), slice<byte>([0xAA, 0xBB, 0xCC]));
    fmt.Printf("%d %#x %v\n"u8, n, p.C, p.B);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static unsafe void Main() {
    var data = new byte[]{0x68, 0, 0x69, 0, 0x21, 0, 0, 0}.slice();
    var u = new slice<uint16>(new ReadOnlySpan<uint16>((uint16*)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡ(data, 0))), (int)(len(data) / 2)));
    fmt.Println(len(u), u[0], u[1], u[2], u[3]);

    var v = new uint16[]{0x4241, 0x4443}.slice();
    var buf = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡ(v, 0))), (int)(len(v) * 2)));
    fmt.Println(len(buf), buf[0], buf[1], buf[2], buf[3]);

    ref var arr = ref heap(new array<uint32>(4), out var Ꮡarr);
    (arr[0], arr[1]) = (0x04030201, 0x08070605);
    var b = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡarr.at<uint32>(0))), 8));
    fmt.Println(len(b), b[0], b[3], b[4], b[7]);

    ref var rb = ref heap(new array<uint16>(8), out var Ꮡrb);
    foreach (var i in range(rb)) {
        rb[i] = (uint16)((rune)'a' + i);
    }
    nint n1 = 2;
    nint n2 = 5;
    var sub = array<uint16>.AliasPointer(Ꮡrb.at<uint16>(0), 64).slice(n1, n2, n2);
    fmt.Println(len(sub), sub[0], sub[1], sub[2]);

    var words = new uint32[]{0x04030201, 0x08070605}.slice();
    var tail = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡ(words, 0))) + 3, 7 - 3));
    fmt.Println(len(tail), tail[0], tail[1], tail[2], tail[3]);
}

} // end main_package

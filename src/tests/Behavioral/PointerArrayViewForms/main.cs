namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

/* [1125899906842623L]byte */
partial struct rawmem /*[1125899906842623]byte*/;

internal static unsafe void main() {
    var src = slice<byte>([1, 2, 3, 4, 5, 6, 7, 8]);
    var p = (uintptr)Ꮡ(src, 0);
    nint n = 8;

    var a = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p), (int)(n)));
    var b = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p), (int)(n)));
    var c = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p), (int)(n)));
    var d = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p) + 2, 6 - 2));
    var e = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p), 4));
    var f = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p) + 1, 5 - 1)).slice(0, 3 - 1);
    var g = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)((@unsafe.Pointer)p) + 2, 7 - 2)).slice(0, 3 - 2);

    fmt.Println(len(a), a[0], a[7]);
    fmt.Println(len(b), b[0], b[7]);
    fmt.Println(len(c), c[0], c[7]);
    fmt.Println(len(d), d[0], d[3]);
    fmt.Println(len(e), e[3]);
    fmt.Println(len(f), cap(f), f[0], f[1]);
    fmt.Println(len(g), cap(g), g[0]);
}

} // end main_package

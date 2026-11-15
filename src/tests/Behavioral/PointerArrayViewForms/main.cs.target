namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

/* [1125899906842623L]byte */
partial struct rawmem /*[1125899906842623]byte*/;

internal static void main() {
    var src = slice<byte>([1, 2, 3, 4, 5, 6, 7, 8]);
    var p = (uintptr)Ꮡ(src, 0);
    nint n = 8;

    var a = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623);
    var b = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623);
    var c = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623);
    var d = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 2, 6, 1125899906842623);
    var e = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, 4, 1125899906842623);
    var f = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 1, 3, 5, 1125899906842623);
    var g = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 2, 3, 7, 1125899906842623);

    fmt.Println(len(a), a[0], a[7]);
    fmt.Println(len(b), b[0], b[7]);
    fmt.Println(len(c), c[0], c[7]);
    fmt.Println(len(d), d[0], d[3]);
    fmt.Println(len(e), e[3]);
    fmt.Println(len(f), cap(f), f[0], f[1]);
    fmt.Println(len(g), cap(g), g[0]);
}

} // end main_package

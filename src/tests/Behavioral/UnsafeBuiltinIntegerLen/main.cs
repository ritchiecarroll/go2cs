namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var arr = ref heap<array<int32>>(out var Ꮡarr);
    arr = new int32[]{10, 20, 30, 40, 50}.array();

    uintptr n = 3;
    var s = @unsafe.Slice(Ꮡarr.at<int32>(0), n);
    fmt.Println(len(s), s[0], s[2]);

    nuint m = 2;
    var s2 = @unsafe.Slice(Ꮡarr.at<int32>(0), m);
    fmt.Println(len(s2), s2[1]);

    var bytes = slice<byte>("hello"u8);
    @string str = @unsafe.String(Ꮡ(bytes, 0), (uintptr)3);
    fmt.Println(str);
}

} // end main_package

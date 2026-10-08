namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Size /*num:nuint*/;

internal static Size total => 3;

internal static void Main() {
    var s = new slice<nint>((nint)(nuint)(total));
    foreach (var (i, _) in s) {
        s[i] = i * i;
    }
    Size n = 2;
    var b = new slice<byte>((nint)(nuint)(n), (nint)(nuint)(total));
    var m = new map<nint, nint>((nint)(nuint)(total));
    m[1] = 10;
    fmt.Println(len(s), s[0], s[1], s[2]);
    fmt.Println(len(b), cap(b));
    fmt.Println(len(m), m[1]);
}

} // end main_package

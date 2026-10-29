namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var s = slice<nint>([10, 20, 30, 40]);

    uint32 i = 2;
    var p = Ꮡ(s, i);
    p.Value = 99;

    uintptr j = 0;
    var q = Ꮡ(s, j);
    q.Value = 7;

    nuint k = 3;
    var r = Ꮡ(s, k);
    r.Value = 55;

    fmt.Println(s[0], s[1], s[2], s[3]);
}

} // end main_package

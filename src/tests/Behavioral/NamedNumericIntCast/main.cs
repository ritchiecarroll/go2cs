namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nuint")] partial struct idx;

[GoType("num:nint")] partial struct cnt;

internal static void Main() {
    var a = new nint[]{10, 20, 30, 40, 50}.slice();
    idx lo = 1;
    idx hi = 4;
    fmt.Println(a.slice((nint)(nuint)(lo), (nint)(nuint)(hi)));
    cnt sh = 3;
    fmt.Println(((uint64)1).Lsh((int64)(nint)(sh)));
    fmt.Println(a.slice((nint)(nuint)(lo + 1), (nint)(nuint)(hi)));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    map<any, @string> tbl = default!;

    tbl = new map<any, @string>();

    tbl[(@string)"12"u8] = "1";
    tbl[(nint)(12)] = "2";
    tbl[12.0D] = "3";

    fmt.Println(tbl[(@string)"12"u8]);
    fmt.Println(tbl[(nint)(12)]);
    fmt.Println(tbl[12.0D]);
}

} // end main_package

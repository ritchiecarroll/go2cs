namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string setFlag(bool @true) {
    if (@true) {
        return "on";
    }
    return "off";
}

internal static nint with() { return 40; }

internal static void main() {
    fmt.Println(setFlag(true), setFlag(false));

    nint @false = 7;
    fmt.Println(@false + 1);

    var ws = slice<nint>([@with(), 2]);
    fmt.Println(len(ws), ws[0] + ws[1]);
}

} // end main_package

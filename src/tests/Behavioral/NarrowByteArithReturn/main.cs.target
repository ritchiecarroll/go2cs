namespace go;

using fmt = fmt_package;

partial class main_package {

internal static partial byte lower(byte c) {
    if ('A' <= c && c <= 'Z') {
        return (byte)(c + ((rune)'a' - (rune)'A'));
    }
    return c;
}

internal static partial byte wrapRet(byte x) {
    return (byte)(x + x + 1);
}

internal static void main() {
    fmt.Println(lower((byte)'A'), lower((byte)'Z'), lower((byte)'a'));
    fmt.Println(wrapRet(200));
}

} // end main_package

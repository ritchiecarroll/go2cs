namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumTable(nint n) {
    nint total = 0;
    for (nint i = 0; i < n; i++) {
        total += (nint)LiteralByteAt("\x01\x02\x03\x04"u8, i);
    }
    return total;
}

internal static void main() {
    fmt.Println(sumTable(4));
    fmt.Println("abcd"u8[2]);
}

} // end main_package

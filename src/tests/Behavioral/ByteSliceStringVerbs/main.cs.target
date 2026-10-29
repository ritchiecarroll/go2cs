namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct main_named /*[]byte*/;

internal static void Main() {
    slice<byte> nilBytes = default;
    var emptyBytes = slice<byte>([]);
    var text = slice<byte>("ab\n"u8);
    var raw = slice<byte>([0xff, 0x00, 0x41]);

    fmt.Printf("q: %q %q %q\n"u8, nilBytes, emptyBytes, text);
    fmt.Printf("s: %s|%s|%s|\n"u8, nilBytes, emptyBytes, text);
    fmt.Printf("x: %x|%x|%x|%x|\n"u8, nilBytes, emptyBytes, text, raw);
    fmt.Printf("X: %X|\n"u8, raw);
    fmt.Printf("v: %v %v %v\n"u8, nilBytes, emptyBytes, text);
    fmt.Printf("d: %d %d\n"u8, emptyBytes, text);

    fmt.Println("sprintf:", fmt.Sprintf("%q"u8, nilBytes), fmt.Sprintf("%q"u8, text));
    fmt.Printf("named: %q %q\n"u8, (main_named)default, (main_named)slice<byte>((@string)"hi"u8));
}

} // end main_package

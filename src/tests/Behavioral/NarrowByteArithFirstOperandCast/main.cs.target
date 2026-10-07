namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial array<byte> encode(nint e) {
    array<byte> buf = new(3);
    buf[0] = (byte)((byte)(e / 100) + (rune)'0');
    buf[1] = (byte)((byte)(e / 10) % 10 + (rune)'0');
    buf[2] = (byte)((byte)(e % 10) + (rune)'0');
    return buf.Clone();
}

internal static partial byte wrapCase(byte x) {
    array<byte> buf = new(1);
    buf[0] = (byte)((byte)x + (byte)x + (rune)'0');
    return buf[0];
}

internal static void Main() {
    var b = encode(305);
    fmt.Println(b[0], b[1], b[2]);
    fmt.Println(wrapCase(200));
}

} // end main_package

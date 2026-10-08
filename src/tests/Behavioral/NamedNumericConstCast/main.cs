namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Class /*num:nuint*/;

partial struct Big /*num:uint64*/;

internal static Class allClass => /* ^Class(0) */ unchecked((Class)18446744073709551615);
internal static Big allBig => /* ^Big(0) */ unchecked((Big)18446744073709551615);
internal static Class small => /* Class(5) */ 5;

internal static void Main() {
    fmt.Println((uint64)(nuint)allClass);
    fmt.Println((uint64)allBig);
    fmt.Println((nuint)small);
}

} // end main_package

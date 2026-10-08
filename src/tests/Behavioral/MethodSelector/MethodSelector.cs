namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Counter /*num:nint*/;

public static @string String(this Counter c) {
    return fmt.Sprint((nint)c);
}

internal static void Main() {
    Counter c = 5;
    var f = () => c.String();
    fmt.Println(f());
}

} // end main_package

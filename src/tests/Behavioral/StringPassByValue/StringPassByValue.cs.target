namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    @string a = default!;

    a = "Hello World";

    test(a);
    fmt.Println(a);
    fmt.Println();

    a = "Hello World";
    test2(ref a);
    fmt.Println(a);
}

internal static void test(@string a) {
    fmt.Println(a);
    a = "Goodbye World";
    fmt.Println(a);
}

internal static void test2(ref @string a) {
    fmt.Println(a);
    a = "Goodbye World";
    fmt.Println(a);
}

} // end main_package

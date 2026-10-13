namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloWorldˢ = "Hello World"u8;

internal static void Main() {
    @string a = default!;

    a = helloWorldˢ;

    test(a);
    fmt.Println(a);
    fmt.Println();

    a = helloWorldˢ;
    test2(ref a);
    fmt.Println(a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goodbyeWorldˢ = "Goodbye World"u8;

internal static void test(@string a) {
    fmt.Println(a);
    a = goodbyeWorldˢ;
    fmt.Println(a);
}

internal static void test2(ref @string a) {
    fmt.Println(a);
    a = goodbyeWorldˢ;
    fmt.Println(a);
}

} // end main_package

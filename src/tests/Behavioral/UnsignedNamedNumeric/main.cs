namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Flags /*num:nuint*/;

partial struct Mask /*num:uintptr*/;

internal static void Main() {
    Flags a = 6;
    Flags b = 2;
    fmt.Println(a + b);
    fmt.Println(a - b);
    fmt.Println(a * b);
    fmt.Println(a / b);
    fmt.Println(((Flags)0 - a) + a);
    Mask m = 5;
    fmt.Println((Mask)(m | 2));
}

} // end main_package

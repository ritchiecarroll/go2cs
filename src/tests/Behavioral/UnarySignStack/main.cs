namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Celsius /*num:float64*/;

partial struct Level /*num:int16*/;

internal static UntypedInt k => 4;

internal static void Main() {
    nint a = 5;
    fmt.Println(- -a, a);
    fmt.Println(+ +a, a);
    int8 b = -7;
    fmt.Println((int8)(- -b), b);
    fmt.Println((int8)(+ +b), b);
    float64 f = 2.5D;
    fmt.Println(- -f, f);
    fmt.Println(+ +f, f);
    Celsius c = 36.5D;
    fmt.Println(- -c, c);
    fmt.Println(+ +c, c);
    Level l = 3;
    fmt.Println(- -l, l);
    fmt.Println(+ +l, l);
    fmt.Println(-(-a), +(+a), a);
    fmt.Println(- - -a, + + +a, a);
    fmt.Println((nint)(- -1), (nint)(+ +1), (nint)(- -k), (nint)(+ +k));
    nint x = - -a;
    var y = (int8)(+ +b);
    fmt.Println(x, y, a, b);
    fmt.Println(-+a, +-a, a);
}

} // end main_package

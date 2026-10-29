namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct StrIntMap /*map[@string, nint]*/;

internal static void Main() {
    var m = new StrIntMap(0);
    fmt.Println(m == nil);
    fmt.Println(len(m));
    m["a"] = 1;
    fmt.Println(m["a"]);

    var m2 = new StrIntMap(0);
    fmt.Println(m2 == nil);

    StrIntMap z = default;
    fmt.Println(z == nil);
    fmt.Println(len(z));

    var e = new StrIntMap(new map<@string, nint>{});
    fmt.Println(e == nil);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string named = "n1"u8;

[GoType("@string")] partial struct sname;

internal static readonly sname typed = "t1"u8;

internal static void Main() {
    var values = new any[]{
        (any)((@string)("lit"u8)),
        (any)((@string)("paren"u8)),
        (any)((@string)(named)),
        (any)(typed),
        ((any)(@string)("control"u8))
    }.slice();
    foreach (var (_, v) in values) {
        fmt.Printf("%v %T\n"u8, v, v);
    }
    @string s = "v"u8;
    fmt.Printf("%v %T\n"u8, (any)(s), (any)(s));
}

} // end main_package

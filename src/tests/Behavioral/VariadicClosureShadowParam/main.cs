namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static void add(@string verb, slice<@string> args) {
    void errorf(@string format, params ꓸꓸꓸany argsΔ1ʗp) {
        var argsΔ1 = argsΔ1ʗp.sslice();
        fmt.Println(fmt.Sprintf(format, argsΔ1.ꓸꓸꓸ));
    }
    if (len(args) != 1) {
        errorf("%s: want 1 argument, got %d"u8, verb, len(args));
        return;
    }
    errorf("%s: %s"u8, verb, args[0]);
}

internal static void Main() {
    add("go"u8, new @string[]{"1.23.0"u8}.slice());
    add("go"u8, default!);
}

} // end main_package

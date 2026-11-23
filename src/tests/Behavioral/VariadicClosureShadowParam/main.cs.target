namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = variadic<any>;

partial class main_package {

internal static void add(@string verb, slice<@string> args) {
    [GoFunc("add.func1")] void errorf(@string format, params ꓸꓸꓸany argsΔ1ʗp) {
        var argsΔ1 = argsΔ1ʗp;
        fmt.Println(fmt.Sprintf(format, argsΔ1.ꓸꓸꓸ));
    }

    if (len(args) != 1) {
        errorf("%s: want 1 argument, got %d", verb, len(args));
        return;
    }

    errorf("%s: %s", verb, args[0]);
}

internal static void main() {
    add("go", ["1.23.0"]);
    add("go", default);
}

} // end main_package

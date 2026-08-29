namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using Δruntime = runtime_package;

partial class main_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸfmt() {
    builtin.initPackage(typeof(fmt_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸreflect() {
    builtin.initPackage(typeof(reflect_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸruntime() {
    builtin.initPackage(typeof(runtime_package));
}

internal static nint passInt(nint x) {
    return x;
}

internal static @string passString(@string s) {
    return s;
}

[GoType] partial struct receiver {
}

internal static void method(this receiver _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilFuncˢ = "<nil Func>"u8;
private static readonly @string emptyNameˢ = "<empty name>"u8;

internal static @string nameOf(any fn) {
    var p = reflect.ValueOf(fn).Pointer();
    var f = Δruntime.FuncForPC(p);
    if (f == nil) {
        return nilFuncˢ;
    }
    @string name = f.Name();
    if (name == ""u8) {
        return emptyNameˢ;
    }
    return name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object funcˢ = (@string)"func:  "u8;
private static readonly object methodˢ = (@string)"method:"u8;
private static readonly object literalPresentˢ = (@string)"literal present:"u8;

internal static void Main() {
    fmt.Println(funcˢ, nameOf(passInt));
    fmt.Println(funcˢ, nameOf(passString));
    fmt.Println(methodˢ, nameOf((Action<receiver>)(method)));
    var literal = () => {
    };
    fmt.Println(literalPresentˢ, nameOf(literal.OrTypedNilFunc()) != "<empty name>"u8 && nameOf(literal.OrTypedNilFunc()) != "<nil Func>"u8);
}

} // end main_package

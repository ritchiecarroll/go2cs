namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;

partial class main_package {

internal static nint passInt(nint x) {
    return x;
}

internal static @string passString(@string s) {
    return s;
}

partial struct receiver {
}

internal static void method(this receiver _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilFuncˢ = "<nil Func>"u8;
private static readonly @string emptyNameˢ = "<empty name>"u8;

internal static @string nameOf(any fn) {
    var p = reflect.ValueOf(fn).Pointer();
    var f = runtime.FuncForPC(p);
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
    fmt.Println(methodˢ, nameOf(((Action<receiver>)(method))));
    var literal = () => {
    };
    fmt.Println(literalPresentˢ, nameOf((literal).OrTypedNilFunc()) != "<empty name>"u8 && nameOf((literal).OrTypedNilFunc()) != "<nil Func>"u8);
}

} // end main_package

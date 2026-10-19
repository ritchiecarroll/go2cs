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

internal static @string nameOf(any fn) {
    var p = reflect.ValueOf(fn).Pointer();
    var f = runtime.FuncForPC(p);
    if (f == nil) {
        return "<nil Func>";
    }
    @string name = f.Name();
    if (name == ""u8) {
        return "<empty name>";
    }
    return name;
}

internal static void Main() {
    fmt.Println("func:  ", nameOf(passInt));
    fmt.Println("func:  ", nameOf(passString));

    fmt.Println("method:", nameOf(((Action<receiver>)(method))));

    var literal = () => {
    };
    fmt.Println("literal present:", nameOf((literal).OrTypedNilFunc()) != "<empty name>"u8 && nameOf((literal).OrTypedNilFunc()) != "<nil Func>"u8);
}

} // end main_package

namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static nint valueMethod(this T t) {
    return t.n;
}

public static nint ValueMethod(this T t) {
    return t.n + 1;
}

internal static nint pointerMethod(this ref T t) {
    return t.n;
}

partial struct W {
    /*embed*/ public bytes_package.Buffer Buffer;
}

internal static @string nameOf(any fn) {
    var f = runtime.FuncForPC(reflect.ValueOf(fn).Pointer());
    if (f == nil) {
        return "<nil Func>";
    }
    return f.Name();
}

internal partial struct table_rows /*dyn*/ {
    internal any fn;
    internal @string label;
}

internal static partial void table() {
        var recvʗ1 = ~(Ꮡ(new T()));
    var rows = new table_rows[]{
        new(() => {
        }, "literal in the table"),
        new(() => new T().valueMethod(), "value method value, composite receiver"),
        new(() => new T().ValueMethod(), "exported value method value, composite receiver"),
        new(() => recvʗ1.valueMethod(), "value method value, pointer receiver"),
        new((Ꮡ(new T())).pointerMethod, "pointer method value (control)"),
        new((Ꮡ(new W())).Write, "promoted method value"),
        new(nint () => 2, "second literal in the table")
    }.slice();

    foreach (var (_, row) in rows) {
        fmt.Printf("%-48s %s\n"u8, row.label + ":", nameOf(row.fn));
    }
}

internal static void Main() {
    table();
}

} // end main_package

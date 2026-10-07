namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct T {
    internal nint n;
}

internal static nint valueMethod(this T t) {
    return t.n;
}

public static nint ValueMethod(this T t) {
    return t.n + 1;
}

[GoRecv] internal static nint pointerMethod(this ref T t) {
    return t.n;
}

[GoType] partial struct W {
    public partial ref bytes_package.Buffer Buffer { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilFuncˢ = "<nil Func>"u8;

internal static @string nameOf(any fn) {
    var f = runtime.FuncForPC(reflect.ValueOf(fn).Pointer());
    if (f == nil) {
        return nilFuncˢ;
    }
    return f.Name();
}

[GoType("dyn")] internal partial struct table_rows {
    internal any fn;
    internal @string label;
}

internal static partial void table() {
        var recvʗ1 = ~(Ꮡ(new T(nil)));
    var rows = new table_rows[]{
        new(() => {
        }, "literal in the table"u8),
        new(() => new T(nil).valueMethod(), "value method value, composite receiver"u8),
        new(() => new T(nil).ValueMethod(), "exported value method value, composite receiver"u8),
        new(() => recvʗ1.valueMethod(), "value method value, pointer receiver"u8),
        new((Ꮡ(new T(nil))).pointerMethod, "pointer method value (control)"u8),
        new((Ꮡ(new W(nil))).Write, "promoted method value"u8),
        new(nint () => 2, "second literal in the table"u8)
    }.slice();
    foreach (var (_, row) in rows) {
        fmt.Printf("%-48s %s\n"u8, row.label + ":", nameOf(row.fn));
    }
}

internal static void Main() {
    table();
}

} // end main_package

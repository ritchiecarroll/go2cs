namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pointerErr {
    internal @string msg;
}

internal static @string Error(this ref pointerErr p) {
    return p.msg;
}

partial struct valueErr {
    internal @string msg;
}

internal static @string Error(this valueErr v) {
    return v.msg;
}

internal partial struct main_cases /*dyn*/ {
    internal error err;
    internal @string want;
}

internal partial struct main_ptrCases /*dyn*/ {
    internal @string want;
    /*embed*/ internal error error;
}

internal static void Main() {
    var cases = new main_cases[]{
        new(new pointerErrжerror(Ꮡ(new pointerErr("pointer-receiver"u8))), "pointer-receiver"u8),
        new(new valueErr("value-receiver"u8), "value-receiver"u8)
    }.slice();
    foreach (var (_, c) in cases) {
        fmt.Printf("%s | %s\n"u8, c.err.Error(), c.want);
    }
    var ptrCases = new ж<main_ptrCases>[]{
        Ꮡ(new main_ptrCases("embedded-pointer"u8, new pointerErrжerror(Ꮡ(new pointerErr("embedded-pointer"u8))))),
        Ꮡ(new main_ptrCases("embedded-value"u8, new valueErr("embedded-value"u8)))
    }.slice();
    foreach (var (_, c) in ptrCases) {
        fmt.Printf("%s | %s\n"u8, (~c).error.Error(), (~c).want);
    }
}

} // end main_package

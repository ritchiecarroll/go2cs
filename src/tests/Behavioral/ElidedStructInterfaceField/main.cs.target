namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pointerErr {
    internal @string msg;
}

internal static @string Error(this ref pointerErr p) { return p.msg; }

partial struct valueErr {
    internal @string msg;
}

internal static @string Error(this valueErr v) { return v.msg; }

internal partial struct main_cases /*dyn*/ {
    internal error err;
    internal @string want;
}

internal partial struct main_ptrCases /*dyn*/ {
    internal @string want;
    /*embed*/ internal error error;
}

internal static void main() {
    var cases = slice<main_cases>([
        new(new pointerErrжerror(Ꮡ(new pointerErr("pointer-receiver"))), "pointer-receiver"),
        new(new valueErr("value-receiver"), "value-receiver")
    ]);

    foreach (var (_, c) in range(cases)) {
        fmt.Printf("%s | %s\n"u8, c.err.Error(), c.want);
    }

    var ptrCases = slice<ж<main_ptrCases>>([
        Ꮡ(new main_ptrCases("embedded-pointer", new pointerErrжerror(Ꮡ(new pointerErr("embedded-pointer"))))),
        Ꮡ(new main_ptrCases("embedded-value", new valueErr("embedded-value")))
    ]);

    foreach (var (_, c) in range(ptrCases)) {
        fmt.Printf("%s | %s\n"u8, (~c).error.Error(), (~c).want);
    }
}

} // end main_package

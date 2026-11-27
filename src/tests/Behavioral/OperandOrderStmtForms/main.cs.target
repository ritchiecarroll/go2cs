namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint g;

internal static nint bump() {
    g++;
    return g * 10;
}

internal static void show(nint a, nint b) { fmt.Println("defer arguments:", a, b); }

internal static nint pick(nint a, nint b) { return a * 100 + b; }

internal static void declCase() {
    var evalᴛ1 = bump();
    nint v = pick(g, evalᴛ1);
    fmt.Println("var with a call:", v);

    var evalᴛ2 = bump();
    nint w = g;
    nint z = evalᴛ2;
    fmt.Println("var pair:", w, z);
}

internal static void deferCase() {
    GoFrame ᒐ = default;
    try {
        var evalᴛ1 = bump();
        defer(show, g, evalᴛ1, ref ᒐ);
        fmt.Println("defer registered at:", g);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void goCase() {
    var done = new channel<bool>(0);

    var evalᴛ1 = bump();
    var doneʗ1 = done;
    goǃ([GoFunc("goCase.func1")] (nint a, nint b) => {
        fmt.Println("go arguments:", a, b);
        doneʗ1.ᐸꟷ(true);
    }, g, evalᴛ1);

    ᐸꟷ(done);
}

internal static void switchCase() {
    var evalᴛ1 = bump();
    switch (g + evalᴛ1) {
    case 44: {
        fmt.Println("switch tag: read before the call");
        break;
    }
    case 55: {
        fmt.Println("switch tag: read after the call");
        break;
    }
    default: {
        fmt.Println("switch tag: other", g);
        break;
    }}

}

internal static void main() {
    declCase();
    deferCase();
    goCase();
    switchCase();
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint g;

internal static nint bump() {
    g++;
    return g * 10;
}

internal static slice<nint> pick(nint a, nint b) { return [a, b]; }

internal static bool show(nint a, nint b, nint c) {
    fmt.Println("else-if condition:", a, b, c);
    return true;
}

internal static void gotoCase() {
    nint n = 0;
again:
    var evalᴛ1 = bump();
    fmt.Println("goto target:", g, evalᴛ1);
    n++;

    if (n < 2) {
        goto again;
    }
}

internal static void rangeCase() {
outer:
    var evalᴛ1 = bump();
    foreach (var (_, x) in range(pick(g, evalᴛ1))) {
        for (nint j = 0; j < 2; j++) {
            if (j == 1) {
                goto continue_outer;
            }

            fmt.Println("labeled range:", x, j);
        }
continue_outer:;
    }
break_outer:;
}

internal static void ifCase() {
    nint n = 0;
check:
    var evalᴛ1 = bump();
    if (g + evalᴛ1 > 0) {
        fmt.Println("labeled if:", g);
    }

    n++;

    if (n < 2) {
        goto check;
    }
}

internal static void elseIfCase() {
    if (g > 1000) {
        fmt.Println("unreachable");
    } else {
        nint v = bump();
        var evalᴛ1 = bump();
        if (show(g, v, evalᴛ1)) {
            fmt.Println("else-if body:", g);
        }
    }
}

internal static void literalCase() {
    var evalᴛ2 = func([GoFunc("literalCase.func1")] nint () => {
        var evalᴛ1 = bump();
        fmt.Println("literal body:", g, evalᴛ1);
        return 1;
    })();
    var evalᴛ3 = bump();
    fmt.Println("literal:", g, evalᴛ2, evalᴛ3);
}

internal static bool less(nint a, nint b) {
    fmt.Println("loop condition:", a, b);
    return a < b;
}

internal static void loopCase() {
    for (nint n = 0; ; n++) {
        var evalᴛ1 = bump();
        if (!(less(g, evalᴛ1))) break;
        fmt.Println("loop pass:", n);

        if (n == 1) {
            break;
        }
    }
}

internal static void main() {
    gotoCase();
    rangeCase();
    ifCase();
    elseIfCase();
    literalCase();
    loopCase();
}

} // end main_package

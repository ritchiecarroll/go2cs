namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> log;

internal static nint f(@string name, nint v) {
    log = append(log, name);
    return v;
}

internal static @string s(@string name, @string v) {
    log = append(log, name);
    return v;
}

internal static void flush(@string label, @string arm) {
    fmt.Printf("%s -> %s %v\n"u8, label, arm, log);
    log = default!;
}

internal static void leading(nint t) {
    var exprᴛ1 = t;
    if (exprᴛ1 == f("a", 1) || exprᴛ1 == f("b", 2)) {
        flush(fmt.Sprint("leading ", t), "ab");
    }
    else if (exprᴛ1 is 3) {
        flush(fmt.Sprint("leading ", t), "const3");
    }
    else if (exprᴛ1 == f("c", 3) || exprᴛ1 == f("d", 4)) {
        flush(fmt.Sprint("leading ", t), "cd");
    }
    else { /* default: */
        flush(fmt.Sprint("leading ", t), "default");
    }

}

internal static void middle(nint t) {
    var exprᴛ1 = t;
    if (exprᴛ1 == f("e", 5)) {
        flush(fmt.Sprint("middle ", t), "e");
    }
    else if (exprᴛ1 is 6) {
        flush(fmt.Sprint("middle ", t), "const6");
    }
    else if (exprᴛ1 == f("g", 6) || exprᴛ1 == f("h", 7)) {
        flush(fmt.Sprint("middle ", t), "gh");
    }
    else { /* default: */
        flush(fmt.Sprint("middle ", t), "default");
    }

}

internal static void nested(@string a, nint b) {
    {
        @string x = a + "!"u8;
        var exprᴛ1 = x;
        if (exprᴛ1 == s("p", "p!")) {
            var exprᴛ2 = b;
            if (exprᴛ2 == f("q", 1)) {
                flush("nested "u8 + x, fmt.Sprint("p/q ", b));
            }
            else { /* default: */
                flush("nested "u8 + x, fmt.Sprint("p/default ", b));
            }

        }
        else if (exprᴛ1 == s("r", "r!") || exprᴛ1 == s("t", "t!")) {
            flush("nested "u8 + x, "rt");
        }
    }

}

internal static void iface(any v) {
    var exprᴛ1 = v;
    if (AreEqual(exprᴛ1, ((any)f("i", 1)))) {
        flush(fmt.Sprintf("iface %v"u8, v), "int 1");
    }
    else if (AreEqual(exprᴛ1, ((any)s("j", "one")))) {
        flush(fmt.Sprintf("iface %v"u8, v), "string one");
    }
    else { /* default: */
        flush(fmt.Sprintf("iface %v"u8, v), "default");
    }

}

internal static void loop() {
    for (nint i = 0; i < 5; i++) {
        var exprᴛ1 = i;
        if (exprᴛ1 == f("x", 1)) {
            do {
                if (i > 0) {
                    break;
                }
                flush("loop unreachable", "x");
            } while (false);
        }
        else if (exprᴛ1 == f("y", 2)) {
            continue;
        }
        else { /* default: */
        }

        flush(fmt.Sprint("loop ", i), "after");
    }
    flush("loop", "end");
}

internal static void Main() {
    foreach (var (_, t) in new nint[]{1, 2, 3, 4, 5, 9}.slice()) {
        leading(t);
    }
    foreach (var (_, t) in new nint[]{5, 6, 7, 9}.slice()) {
        middle(t);
    }
    nested("p", 1);
    nested("p", 2);
    nested("t", 0);
    nested("z", 0);
    iface((nint)1);
    iface("one");
    iface(2.5D);
    loop();
}

} // end main_package

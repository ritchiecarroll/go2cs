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
    log = default;
}

internal static void leading(nint t) {
    switch (t) {
    default: {
        flush(fmt.Sprint("leading ", t), "default");
        break;
    }
    case var ᴛ when ᴛ == f("a", 1) || ᴛ == f("b", 2): {
        flush(fmt.Sprint("leading ", t), "ab");
        break;
    }
    case 3: {
        flush(fmt.Sprint("leading ", t), "const3");
        break;
    }
    case var ᴛ when ᴛ == f("c", 3) || ᴛ == f("d", 4): {
        flush(fmt.Sprint("leading ", t), "cd");
        break;
    }}

}

internal static void middle(nint t) {
    switch (t) {
    case var ᴛ when ᴛ == f("e", 5): {
        flush(fmt.Sprint("middle ", t), "e");
        break;
    }
    default: {
        flush(fmt.Sprint("middle ", t), "default");
        break;
    }
    case 6: {
        flush(fmt.Sprint("middle ", t), "const6");
        break;
    }
    case var ᴛ when ᴛ == f("g", 6) || ᴛ == f("h", 7): {
        flush(fmt.Sprint("middle ", t), "gh");
        break;
    }}

}

internal static void nested(@string a, nint b) {
    {
        @string x = a + "!"u8;
        var exprᴛ1 = x;
        switch (ᐧ) {
        case {} when exprᴛ1 == s("p", "p!"): {
            switch (b) {
            case var ᴛ when ᴛ == f("q", 1): {
                flush("nested "u8 + x, fmt.Sprint("p/q ", b));
                break;
            }
            default: {
                flush("nested "u8 + x, fmt.Sprint("p/default ", b));
                break;
            }}

            break;
        }
        case {} when exprᴛ1 == s("r", "r!") || exprᴛ1 == s("t", "t!"): {
            flush("nested "u8 + x, "rt");
            break;
        }}
    }

}

internal static void iface(any v) {
    switch (v) {
    case var ᴛ when AreEqual(ᴛ, ((any)f("i", 1))): {
        flush(fmt.Sprintf("iface %v"u8, v), "int 1");
        break;
    }
    case var ᴛ when AreEqual(ᴛ, ((any)s("j", "one"))): {
        flush(fmt.Sprintf("iface %v"u8, v), "string one");
        break;
    }
    default: {
        flush(fmt.Sprintf("iface %v"u8, v), "default");
        break;
    }}

}

internal static void bytesNotText(@string t, @string y) {
    var exprᴛ1 = t;
    switch (ᐧ) {
    case {} when exprᴛ1 == y: {
        flush(fmt.Sprintf("bytes %x"u8, t), fmt.Sprintf("matched %x"u8, y));
        break;
    }
    case {} when exprᴛ1 == ((@string)(new byte[]{0xff})): {
        flush(fmt.Sprintf("bytes %x"u8, t), "ff");
        break;
    }
    default: {
        flush(fmt.Sprintf("bytes %x"u8, t), "default");
        break;
    }}

}

internal static void loop() {
    for (nint i = 0; i < 5; i++) {
        switch (i) {
        case var ᴛ when ᴛ == f("x", 1): {
            if (i > 0) {
                break;
            }
            flush("loop unreachable", "x");
            break;
        }
        case var ᴛ when ᴛ == f("y", 2): {
            continue;
            break;
        }
        default: {
            break;
        }}

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
    bytesNotText(((@string)(new byte[]{0xff})), ((@string)(new byte[]{0xfe})));
    bytesNotText(((@string)(new byte[]{0xfe})), ((@string)(new byte[]{0xfd})));
    bytesNotText(((@string)(new byte[]{0xfe})), ((@string)(new byte[]{0xfe})));
}

} // end main_package

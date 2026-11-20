namespace go;

using fmt = fmt_package;

partial class main_package {

internal static partial @string classify(nint n) {
    var matchᴛ1 = false;
    if (n is 0) { matchᴛ1 = true;
        return "zero";
    }
    if (n is 1) { matchᴛ1 = true;
        return "one";
    }
    if (n is 2) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return "many";
    }
    return default;

}

internal static partial nint nonTerminal(nint n) {
    nint acc = 0;
    var matchᴛ1 = false;
    if (n is 0) { matchᴛ1 = true;
        acc = 10;
    }
    else if (n is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        acc = 20;
    }

    return acc + n;
}

internal static partial nint conditionalReturn(nint n) {
    var matchᴛ1 = false;
    if (n is 0) { matchᴛ1 = true;
        if (n < 0) {
            return 111;
        }
    }
    else if (n is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return n * 1000;
    }

    return 777;
}

internal static partial (nint r, bool ok) namedDefer(nint n) {
    nint r = default;
    bool ok = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != nil) {
                (r, ok) = (-1, false);
            }
        }, ref ᒐ);
        var matchᴛ1 = false;
        if (n is 0) { matchᴛ1 = true;
            (r, ok) = (100, true); goto ᒐdone;
        }
        if (n is 1) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            (r, ok) = (n * 10, true); goto ᒐdone;
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, ok);
}

internal static @string keepAlive(nint idle, nint interval) {
    switch (ᐧ) {
    case {} when idle < 0 && interval >= 0: {
        return "interval-only";
    }
    case {} when idle < 0 && interval < 0: {
        return "none";
    }
    case {} when idle >= 0 && interval >= 0: {
        break;
    }}

    return "both-or-idle";
}

internal static partial nint leadingDefault(nint n) {
    nint v = 0;
    var matchᴛ1 = false;
    var matchᴛ2 = n is 3 || n is 2 || n is 1;
    if (!matchᴛ2) { /* default: */
        v |= (nint)(8);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && n is 3) { matchᴛ1 = true;
        v |= (nint)(4);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && n is 2) {
        v |= (nint)(2);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && n is 1) { matchᴛ1 = true;
        v |= (nint)(1);
    }

    return v;
}

internal static nint waitObject0 = 0;
internal static nint waitFailed = -1;

internal static partial @string waitShape(nint s) {
    switch (s) {
    case var ᴛ when ᴛ == waitObject0: {
        break;
        break;
    }
    case var ᴛ when ᴛ == waitFailed: {
        return "failed";
    }
    default: {
        return "unexpected";
    }}

    return "ok";
}

internal static void main() {
    foreach (var (_, n) in range(slice<nint>([0, 1, 2, 3]))) {
        fmt.Println(classify(n));
    }
    foreach (var (_, n) in range(slice<nint>([0, 1, 2, 3, 4]))) {
        fmt.Println(leadingDefault(n));
    }
    fmt.Println(keepAlive(-1, 5), keepAlive(-1, -1), keepAlive(3, 5));
    foreach (var (_, n) in range(slice<nint>([0, 1, 2]))) {
        fmt.Println(nonTerminal(n));
    }
    foreach (var (_, n) in range(slice<nint>([0, 1, 2]))) {
        fmt.Println(conditionalReturn(n));
    }
    foreach (var (_, n) in range(slice<nint>([0, 1, 2]))) {
        var (r, ok) = namedDefer(n);
        fmt.Println(r, ok);
    }
    foreach (var (_, s) in range(slice<nint>([0, -1, 258]))) {
        fmt.Println(waitShape(s));
    }
}

} // end main_package

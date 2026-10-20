namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var words = new @string[]{"skip", "stop", "keep", "skip", "keep"}.slice();

    for (nint i = 0; i < len(words); i++) {
        var exprᴛ1 = words[i];
        if (exprᴛ1 == "skip"u8) {
            do {
                if (i == 0) {
                    fmt.Println("a: breaking at", i);
                    break;
                }
                fmt.Println("a: continuing at", i);
                goto continueᴛ1;
            } while (false);
        }
        else if (exprᴛ1 == "stop"u8) {
            fmt.Println("a: stop at", i);
        }

        fmt.Println("a: after switch", i);
continueᴛ1:;
    }

    for (nint i = 0; i < 4; i++) {
        var exprᴛ2 = words[i];
        if (exprᴛ2 == "skip"u8) {
            fmt.Println("b: continuing at", i);
            continue;
        }

        fmt.Println("b: after switch", i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ3 = words[i];
        if (exprᴛ3 == "skip"u8 || exprᴛ3 == "stop"u8) {
            do {
                if (words[i] == "stop") {
                    fmt.Println("c: breaking at", i);
                    break;
                }
                fmt.Println("c: took case at", i);
            } while (false);
        }

        fmt.Println("c: after switch", i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ4 = words[i];
        if (exprᴛ4 == "skip"u8 || exprᴛ4 == "keep"u8) {
            do {
                if (words[i] == "keep") {
                    fmt.Println("d: breaking at", i);
                    break;
                }
                for (nint j = 0; j < 3; j++) {
                    if (j == 1) {
                        continue;
                    }
                    fmt.Println("d: inner", i, j);
                }
                fmt.Println("d: after inner loop", i);
                goto continueᴛ4;
            } while (false);
        }

        fmt.Println("d: after switch", i);
continueᴛ4:;
    }

outer:
    for (nint i = 0; i < 3; i++) {
        var exprᴛ5 = words[i];
        if (exprᴛ5 == "skip"u8 || exprᴛ5 == "stop"u8) {
            do {
                if (words[i] == "stop") {
                    fmt.Println("e: breaking at", i);
                    break;
                }
                fmt.Println("e: labeled continue at", i);
                goto continue_outer;
            } while (false);
        }

        fmt.Println("e: after switch", i);
continue_outer:;
    }
break_outer:;

    foreach (var (idx, w) in new @string[]{"go", "brk", "go", "end"}.slice()) {
        var exprᴛ6 = w;
        if (exprᴛ6 == "go"u8 || exprᴛ6 == "brk"u8) {
            do {
                if (w == "brk"u8) {
                    fmt.Println("f: breaking at", idx);
                    break;
                }
                fmt.Println("f: continuing at", idx);
                goto continueᴛ7;
            } while (false);
        }

        fmt.Println("f: after switch", idx);
continueᴛ7:;
    }

    slice<nint> got = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
        var i = iᴛ1;
        nint f() => i;
        var exprᴛ7 = fmt.Sprint(i % 2);
        if (exprᴛ7 == "1"u8) {
            do {
                if (i == 3) {
                    fmt.Println("g: breaking at", i);
                    break;
                }
                i++;
                got = append(got, f());
                goto continueᴛ8;
            } while (false);
        }

        fmt.Println("g: after switch", i);
continueᴛ8:;
        iᴛ1 = i;
    }
    fmt.Println("g: got", got);
}

} // end main_package

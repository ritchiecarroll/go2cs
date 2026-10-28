namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var words = new @string[]{"skip", "stop", "keep", "skip", "keep"}.slice();

    for (nint i = 0; i < len(words); i++) {
        var exprᴛ1 = words[i];
        switch (ᐧ) {
        case {} when exprᴛ1 == "skip"u8: {
            if (i == 0) {
                fmt.Println("a: breaking at", i);
                break;
            }
            fmt.Println("a: continuing at", i);
            continue;
            break;
        }
        case {} when exprᴛ1 == "stop"u8: {
            fmt.Println("a: stop at", i);
            break;
        }}

        fmt.Println("a: after switch", i);
    }

    for (nint i = 0; i < 4; i++) {
        var exprᴛ2 = words[i];
        switch (ᐧ) {
        case {} when exprᴛ2 == "skip"u8: {
            fmt.Println("b: continuing at", i);
            continue;
            break;
        }}

        fmt.Println("b: after switch", i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ3 = words[i];
        switch (ᐧ) {
        case {} when exprᴛ3 == "skip"u8 || exprᴛ3 == "stop"u8: {
            if (words[i] == "stop") {
                fmt.Println("c: breaking at", i);
                break;
            }
            fmt.Println("c: took case at", i);
            break;
        }}

        fmt.Println("c: after switch", i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ4 = words[i];
        switch (ᐧ) {
        case {} when exprᴛ4 == "skip"u8 || exprᴛ4 == "keep"u8: {
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
            continue;
            break;
        }}

        fmt.Println("d: after switch", i);
    }

outer:
    for (nint i = 0; i < 3; i++) {
        var exprᴛ5 = words[i];
        switch (ᐧ) {
        case {} when exprᴛ5 == "skip"u8 || exprᴛ5 == "stop"u8: {
            if (words[i] == "stop") {
                fmt.Println("e: breaking at", i);
                break;
            }
            fmt.Println("e: labeled continue at", i);
            goto continue_outer;
            break;
        }}

        fmt.Println("e: after switch", i);
continue_outer:;
    }
break_outer:;

    foreach (var (idx, w) in new @string[]{"go", "brk", "go", "end"}.slice()) {
        var exprᴛ6 = w;
        switch (ᐧ) {
        case {} when exprᴛ6 == "go"u8 || exprᴛ6 == "brk"u8: {
            if (w == "brk"u8) {
                fmt.Println("f: breaking at", idx);
                break;
            }
            fmt.Println("f: continuing at", idx);
            continue;
            break;
        }}

        fmt.Println("f: after switch", idx);
    }

    slice<nint> got = default;
    for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
        var i = iᴛ1;
        nint f() => i;
        var exprᴛ7 = fmt.Sprint(i % 2);
        switch (ᐧ) {
        case {} when exprᴛ7 == "1"u8: {
            if (i == 3) {
                fmt.Println("g: breaking at", i);
                break;
            }
            i++;
            got = append(got, f());
            iᴛ1 = i;
            continue;
            break;
        }}

        fmt.Println("g: after switch", i);
        iᴛ1 = i;
    }
    fmt.Println("g: got", got);

    for (nint i = 0; i < len(words); i++) {
        var exprᴛ8 = words[i];
        var matchᴛ1 = false;
        if (exprᴛ8 == "skip"u8) { matchᴛ1 = true;
            do {
                if (i == 0) {
                    fmt.Println("h: breaking at", i);
                    break;
                }
                fmt.Println("h: continuing at", i);
                goto continueᴛ9;
            } while (false);
        }
        else if (exprᴛ8 == "stop"u8) {
            fmt.Println("h: stop at", i);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ8 == "keep"u8) { matchᴛ1 = true;
            fmt.Println("h: stop-or-keep at", i);
        }

        fmt.Println("h: after switch", i);
continueᴛ9:;
    }

    slice<nint> gotFt = default;
    for (nint iᴛ2 = 0; iᴛ2 < 6; iᴛ2++) {
        var i = iᴛ2;
        nint f() => i;
        var exprᴛ9 = fmt.Sprint(i % 3);
        var matchᴛ2 = false;
        if (exprᴛ9 == "1"u8) { matchᴛ2 = true;
            do {
                if (i == 4) {
                    fmt.Println("i: breaking at", i);
                    break;
                }
                i++;
                gotFt = append(gotFt, f());
                goto continueᴛ10;
            } while (false);
        }
        else if (exprᴛ9 == "2"u8) {
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && exprᴛ9 == "0"u8) { matchᴛ2 = true;
            fmt.Println("i: two-or-zero at", i);
        }

        fmt.Println("i: after switch", i);
continueᴛ10:;
        iᴛ2 = i;
    }
    fmt.Println("i: got", gotFt);
}

} // end main_package

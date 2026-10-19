namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aBreakingAtˢ = (@string)"a: breaking at"u8;
private static readonly object aContinuingAtˢ = (@string)"a: continuing at"u8;
private static readonly object aStopAtˢ = (@string)"a: stop at"u8;
private static readonly object aAfterSwitchˢ = (@string)"a: after switch"u8;
private static readonly object bContinuingAtˢ = (@string)"b: continuing at"u8;
private static readonly object bAfterSwitchˢ = (@string)"b: after switch"u8;
private static readonly object cBreakingAtˢ = (@string)"c: breaking at"u8;
private static readonly object cTookCaseAtˢ = (@string)"c: took case at"u8;
private static readonly object cAfterSwitchˢ = (@string)"c: after switch"u8;
private static readonly object dBreakingAtˢ = (@string)"d: breaking at"u8;
private static readonly object dInnerˢ = (@string)"d: inner"u8;
private static readonly object dAfterInnerLoopˢ = (@string)"d: after inner loop"u8;
private static readonly object dAfterSwitchˢ = (@string)"d: after switch"u8;
private static readonly object eBreakingAtˢ = (@string)"e: breaking at"u8;
private static readonly object eLabeledContinueAtˢ = (@string)"e: labeled continue at"u8;
private static readonly object eAfterSwitchˢ = (@string)"e: after switch"u8;
private static readonly object fBreakingAtˢ = (@string)"f: breaking at"u8;
private static readonly object fContinuingAtˢ = (@string)"f: continuing at"u8;
private static readonly object fAfterSwitchˢ = (@string)"f: after switch"u8;
private static readonly object gBreakingAtˢ = (@string)"g: breaking at"u8;
private static readonly object gAfterSwitchˢ = (@string)"g: after switch"u8;
private static readonly object gGotˢ = (@string)"g: got"u8;

internal static void Main() {
    var words = new @string[]{"skip", "stop", "keep", "skip", "keep"}.slice();

    for (nint i = 0; i < len(words); i++) {
        var exprᴛ1 = words[i];
        if (exprᴛ1 == "skip"u8) {
            do {
                if (i == 0) {
                    fmt.Println(aBreakingAtˢ, i);
                    break;
                }
                fmt.Println(aContinuingAtˢ, i);
                goto continueᴛ1;
            } while (false);
        }
        else if (exprᴛ1 == "stop"u8) {
            fmt.Println(aStopAtˢ, i);
        }

        fmt.Println(aAfterSwitchˢ, i);
continueᴛ1:;
    }

    for (nint i = 0; i < 4; i++) {
        var exprᴛ2 = words[i];
        if (exprᴛ2 == "skip"u8) {
            fmt.Println(bContinuingAtˢ, i);
            continue;
        }

        fmt.Println(bAfterSwitchˢ, i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ3 = words[i];
        if (exprᴛ3 == "skip"u8 || exprᴛ3 == "stop"u8) {
            do {
                if (words[i] == "stop") {
                    fmt.Println(cBreakingAtˢ, i);
                    break;
                }
                fmt.Println(cTookCaseAtˢ, i);
            } while (false);
        }

        fmt.Println(cAfterSwitchˢ, i);
    }

    for (nint i = 0; i < 3; i++) {
        var exprᴛ4 = words[i];
        if (exprᴛ4 == "skip"u8 || exprᴛ4 == "keep"u8) {
            do {
                if (words[i] == "keep") {
                    fmt.Println(dBreakingAtˢ, i);
                    break;
                }
                for (nint j = 0; j < 3; j++) {
                    if (j == 1) {
                        continue;
                    }
                    fmt.Println(dInnerˢ, i, j);
                }
                fmt.Println(dAfterInnerLoopˢ, i);
                goto continueᴛ4;
            } while (false);
        }

        fmt.Println(dAfterSwitchˢ, i);
continueᴛ4:;
    }

outer:
    for (nint i = 0; i < 3; i++) {
        var exprᴛ5 = words[i];
        if (exprᴛ5 == "skip"u8 || exprᴛ5 == "stop"u8) {
            do {
                if (words[i] == "stop") {
                    fmt.Println(eBreakingAtˢ, i);
                    break;
                }
                fmt.Println(eLabeledContinueAtˢ, i);
                goto continue_outer;
            } while (false);
        }

        fmt.Println(eAfterSwitchˢ, i);
continue_outer:;
    }
break_outer:;

    foreach (var (idx, w) in new @string[]{"go", "brk", "go", "end"}.slice()) {
        var exprᴛ6 = w;
        if (exprᴛ6 == "go"u8 || exprᴛ6 == "brk"u8) {
            do {
                if (w == "brk"u8) {
                    fmt.Println(fBreakingAtˢ, idx);
                    break;
                }
                fmt.Println(fContinuingAtˢ, idx);
                goto continueᴛ7;
            } while (false);
        }

        fmt.Println(fAfterSwitchˢ, idx);
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
                    fmt.Println(gBreakingAtˢ, i);
                    break;
                }
                i++;
                got = append(got, f());
                goto continueᴛ8;
            } while (false);
        }

        fmt.Println(gAfterSwitchˢ, i);
continueᴛ8:;
        iᴛ1 = i;
    }
    fmt.Println(gGotˢ, got);
}

} // end main_package

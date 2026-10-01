namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint n;

internal static (nint, bool) next() {
    n++;
    return (n, n < 4);
}

internal static bool keep(nint v, bool ok) {
    return ok;
}

internal static nint must(nint v, bool ok) {
    return v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object whileCondˢ = (@string)"while cond:"u8;
private static readonly object condIˢ = (@string)"  cond i:"u8;
private static readonly object forCondˢ = (@string)"for cond:"u8;
private static readonly object postIˢ = (@string)"  post i:"u8;
private static readonly object outerIˢ = (@string)"  outer i:"u8;

internal static void Main() {
    n = 0;
    nint count = 0;
    while (ᐧ) {
        var (ᴛ1, ᴛ2) = next();
        if (!(keep(ᴛ1, ᴛ2))) break;
        count++;
        if (count > 10) {
            break;
        }
    }
    fmt.Println(whileCondˢ, count, n);
    n = 0;
    count = 0;
    for (nint i = 0; ᐧ ; i++) {
        var (ᴛ3, ᴛ4) = next();
        if (!(keep(ᴛ3, ᴛ4))) break;
        {
            count++; if (count > 10) {
                break;
            }
        }
        if (i % 2 == 0) {
            continue;
        }
        fmt.Println(condIˢ, i);
    }
    fmt.Println(forCondˢ, count, n);
    n = 0;
    var (ᴛ5, ᴛ6) = next();
    for (nint i = must(ᴛ5, ᴛ6); i < 6; ) {
        if (i % 2 == 0) {
            goto continueᴛ3;
        }
        fmt.Println(postIˢ, i);
continueᴛ3:;
        var (ᴛ7, ᴛ8) = next();
        i = must(ᴛ7, ᴛ8);
    }
    n = 0;
outer:
    var (ᴛ9, ᴛ10) = next();
    for (nint i = must(ᴛ9, ᴛ10); i < 7; ) {
        switch (ᐧ) {
        case {} when i is 2: {
            goto continueᴛ4;
            break;
        }}

        for (nint j = 0; j < 2; j++) {
            if (i == 4 && j == 1) {
                goto continue_outer;
            }
        }
        fmt.Println(outerIˢ, i);
continue_outer:;
continueᴛ4:;
        var (ᴛ11, ᴛ12) = next();
        i = must(ᴛ11, ᴛ12);
    }
break_outer:;
    n = 0;
    slice<Func<nint>> funcs = default!;
    var (ᴛ13, ᴛ14) = next();
    for (nint iᴛ1 = must(ᴛ13, ᴛ14); iᴛ1 < 4; ) {
        var i = iᴛ1;
        funcs = append(funcs, () => i);
        var (ᴛ15, ᴛ16) = next();
        iᴛ1 = must(ᴛ15, ᴛ16);
    }
    foreach (var (_, f) in funcs) {
        fmt.Print(f(), (@string)" "u8);
    }
    fmt.Println();
}

} // end main_package

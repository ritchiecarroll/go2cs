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
    var (ᴛ1, ᴛ2) = next();
    while (keep(ᴛ1, ᴛ2)) {
        count++;
        if (count > 10) {
            break;
        }
    }
    fmt.Println(whileCondˢ, count, n);
    n = 0;
    count = 0;
    var (ᴛ3, ᴛ4) = next();
    for (nint i = 0; keep(ᴛ3, ᴛ4); i++) {
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
    for (nint i = must(next()); i < 6; i = must(next())) {
        if (i % 2 == 0) {
            continue;
        }
        fmt.Println(postIˢ, i);
    }
    n = 0;
outer:
    for (nint i = must(next()); i < 7; i = must(next())) {
        switch (ᐧ) {
        case {} when i is 2: {
            continue;
            break;
        }}

        for (nint j = 0; j < 2; j++) {
            if (i == 4 && j == 1) {
                goto continue_outer;
            }
        }
        fmt.Println(outerIˢ, i);
continue_outer:;
    }
break_outer:;
    n = 0;
    slice<Func<nint>> funcs = default!;
    for (nint iᴛ1 = must(next()); iᴛ1 < 4; iᴛ1 = must(next())) {
        var i = iᴛ1;
        funcs = append(funcs, () => i);
    }
    foreach (var (_, f) in funcs) {
        fmt.Print(f(), (@string)" "u8);
    }
    fmt.Println();
}

} // end main_package

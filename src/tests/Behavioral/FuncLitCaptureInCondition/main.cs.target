namespace go;

using fmt = fmt_package;

partial class main_package {

internal static bool anyMatch(slice<nint> vals, Func<nint, bool> f) {
    foreach (var (_, v) in vals) {
        if (f(v)) {
            return true;
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ifInitˢ = (@string)"if-init:"u8;
private static readonly object forˢ = (@string)"for:"u8;
private static readonly object whileˢ = (@string)"while:"u8;

internal static void Main() {
    var vals = new nint[]{1, 2, 3}.slice();
    var lookup = new map<nint, bool>{[2] = true, [5] = true};

    var lookupʗ1 = lookup;
    if (anyMatch(vals, (nint u) => lookupʗ1[u])) {
        fmt.Println((@string)"if:"u8, true);
    }

    {
        nint extra = 7;
        var lookupʗ2 = lookup;
        if (anyMatch(vals, (nint u) => lookupʗ2[u] || u == extra)) {
            fmt.Println(ifInitˢ, true);
        }
    }

    var lookupʗ3 = lookup;
    for (nint i = 0; i < 2 && anyMatch(vals, (nint u) => lookupʗ3[u + i]); i++) {
        fmt.Println(forˢ, i);
    }

    nint n = 0;
    var lookupʗ4 = lookup;
    while (anyMatch(vals, (nint u) => lookupʗ4[u] && n < 2)) {
        fmt.Println(whileˢ, n);
        n++;
    }
}

} // end main_package

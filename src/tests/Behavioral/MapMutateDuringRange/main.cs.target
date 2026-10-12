namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string dump(map<@string, nint> m, slice<@string> keys) {
    @string s = ""u8;
    foreach (var (_, k) in keys) {
        {
            var (v, ok) = m[k, ꟷ]; if (ok) {
                s += fmt.Sprintf("%s=%d "u8, k, v);
            }
        }
    }
    return fmt.Sprintf("[%s] len=%d"u8, s, len(m));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object insertVisitedˢ = (@string)"insert visited:"u8;
private static readonly object insertFinalˢ = (@string)"insert final:"u8;
private static readonly object updateFinalˢ = (@string)"update final:"u8;
private static readonly object deleteFinalˢ = (@string)"delete final:"u8;
private static readonly object mixFinalˢ = (@string)"mix final:"u8;
private static readonly object controlFinalˢ = (@string)"control final:"u8;
private static readonly object nanLenˢ = (@string)"nan len:"u8;
private static readonly object visitedˢ = (@string)"visited:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object nanLookupFoundˢ = (@string)"nan lookup found:"u8;
private static readonly object nanLenAfterDeleteˢ = (@string)"nan len after delete:"u8;
private static readonly object mixedLenˢ = (@string)"mixed len:"u8;
private static readonly object at25ˢ = (@string)"at2.5:"u8;

internal static void Main() {
    var insert = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2, ["c"u8] = 3};
    nint visited = 0;
    foreach (var (k, v) in insert) {
        if (len(k) == 1) {
            visited++;
            insert[k + "!"u8] = v * 10;
        }
    }
    fmt.Println(insertVisitedˢ, visited);
    fmt.Println(insertFinalˢ, dump(insert, new @string[]{"a"u8, "a!"u8, "b"u8, "b!"u8, "c"u8, "c!"u8}.slice()));

    var update = new map<@string, nint>{["x"u8] = 1, ["y"u8] = 2};
    foreach (var (k, v) in update) {
        update[k] = v + 100;
    }
    fmt.Println(updateFinalˢ, dump(update, new @string[]{"x"u8, "y"u8}.slice()));

    var del = new map<@string, nint>{["p"u8] = 1, ["q"u8] = 2, ["r"u8] = 3};
    foreach (var (k, _) in del) {
        delete(del, k);
    }
    fmt.Println(deleteFinalˢ, dump(del, new @string[]{"p"u8, "q"u8, "r"u8}.slice()));

    var mix = new map<@string, nint>{["m"u8] = 1, ["n"u8] = 2};
    foreach (var (k, v) in mix) {
        if (len(k) == 1) {
            mix[k + k] = v * 7;
            delete(mix, k);
        }
    }
    fmt.Println(mixFinalˢ, dump(mix, new @string[]{"m"u8, "mm"u8, "n"u8, "nn"u8}.slice()));

    var src = new map<@string, nint>{["s"u8] = 5};
    var dst = new map<@string, nint>{};
    foreach (var (k, v) in src) {
        dst[k + "-copy"u8] = v;
    }
    fmt.Println(controlFinalˢ, dump(dst, new @string[]{"s-copy"u8}.slice()));

    var zero = 0.0D;
    var nan = new map<float64, nint>{};
    nan[zero / zero] = 1;
    nan[zero / zero] = 1;

    nint seen = 0;
    nint sum = 0;
    foreach (var (_, v) in nan) {
        seen++;
        sum += v;
    }
    fmt.Println(nanLenˢ, len(nan), visitedˢ, seen, sumˢ, sum);

    var (_, nanFound) = nan[zero / zero, ꟷ];
    fmt.Println(nanLookupFoundˢ, nanFound);

    delete(nan, zero / zero);
    fmt.Println(nanLenAfterDeleteˢ, len(nan));

    var mixed = new map<float64, nint>{[1.5D] = 10};
    mixed[zero / zero] = 1;
    mixed[zero / zero] = 1;
    foreach (var (k, v) in mixed) {
        if (k == 1.5D) {
            mixed[2.5D] = v * 2;
        }
    }
    fmt.Println(mixedLenˢ, len(mixed), at25ˢ, mixed[2.5D]);
}

} // end main_package

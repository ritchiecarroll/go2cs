namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string dump(map<@string, nint> m, slice<@string> keys) {
    @string s = "";
    foreach (var (_, k) in keys) {
        {
            var (v, ok) = m[k, ꟷ]; if (ok) {
                s += fmt.Sprintf("%s=%d "u8, k, v);
            }
        }
    }
    return fmt.Sprintf("[%s] len=%d"u8, s, len(m));
}

internal static void Main() {
    var insert = new map<@string, nint>{["a"] = 1, ["b"] = 2, ["c"] = 3};
    nint visited = 0;
    foreach (var (k, v) in insert) {
        if (len(k) == 1) {
            visited++;
            insert[k + "!"u8] = v * 10;
        }
    }
    fmt.Println("insert visited:", visited);
    fmt.Println("insert final:", dump(insert, slice<@string>(["a", "a!", "b", "b!", "c", "c!"])));

    var update = new map<@string, nint>{["x"] = 1, ["y"] = 2};
    foreach (var (k, v) in update) {
        update[k] = v + 100;
    }
    fmt.Println("update final:", dump(update, slice<@string>(["x", "y"])));

    var del = new map<@string, nint>{["p"] = 1, ["q"] = 2, ["r"] = 3};
    foreach (var (k, _) in del) {
        delete(del, k);
    }
    fmt.Println("delete final:", dump(del, slice<@string>(["p", "q", "r"])));

    var mix = new map<@string, nint>{["m"] = 1, ["n"] = 2};
    foreach (var (k, v) in mix) {
        if (len(k) == 1) {
            mix[k + k] = v * 7;
            delete(mix, k);
        }
    }
    fmt.Println("mix final:", dump(mix, slice<@string>(["m", "mm", "n", "nn"])));

    var src = new map<@string, nint>{["s"] = 5};
    var dst = new map<@string, nint>{};
    foreach (var (k, v) in src) {
        dst[k + "-copy"u8] = v;
    }
    fmt.Println("control final:", dump(dst, slice<@string>(["s-copy"])));

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
    fmt.Println("nan len:", len(nan), "visited:", seen, "sum:", sum);

    var (_, nanFound) = nan[zero / zero, ꟷ];
    fmt.Println("nan lookup found:", nanFound);

    delete(nan, zero / zero);
    fmt.Println("nan len after delete:", len(nan));

    var mixed = new map<float64, nint>{[1.5D] = 10};
    mixed[zero / zero] = 1;
    mixed[zero / zero] = 1;
    foreach (var (k, v) in mixed) {
        if (k == 1.5D) {
            mixed[2.5D] = v * 2;
        }
    }
    fmt.Println("mixed len:", len(mixed), "at2.5:", mixed[2.5D]);
}

} // end main_package

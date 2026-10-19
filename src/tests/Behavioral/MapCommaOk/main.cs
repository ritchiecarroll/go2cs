namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zAbsentˢ = (@string)"z absent"u8;

internal static void Main() {
    var m = new map<@string, nint>{["a"] = 1, ["b"] = 2};

    var (v, ok) = m["a", ꟷ];
    fmt.Println(v, ok);

    var (v2, ok2) = m["z", ꟷ];
    fmt.Println(v2, ok2);

    var (_, ok3) = m["b", ꟷ];
    fmt.Println(ok3);

    {
        var (_, ok4) = m["z", ꟷ]; if (!ok4) {
            fmt.Println(zAbsentˢ);
        }
    }

    nint w = default!;
    bool present = default!;
    (w, present) = m["b", ꟷ];
    fmt.Println(w, present);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal any value;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noneˢ = "none"u8;

internal static @string sprint(any x) {
    switch (x.type()) {
    case @string v: {
        return "str:"u8 + v;
    }
    case null: {
        return noneˢ;
    }
    default: {
        var v = x;
        return fmt.Sprintf("other:%v"u8, v);
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilˢ = (@string)"<nil>"u8;
private static readonly object missingˢ = (@string)"<missing>"u8;
private static readonly object fieldˢ = (@string)"field"u8;

internal static void Main() {
    any arg = default!;
    arg = nilˢ;
    fmt.Println(sprint(arg));
    var args = new any[]{(nint)(1), default!, (@string)"keep"u8}.slice();
    foreach (var (i, vᴛ1) in args) {
        var a = vᴛ1;

        if (a == default!) {
            a = missingˢ;
        }
        fmt.Println(i, sprint(a));
    }
    holder h = default!;
    h.value = fieldˢ;
    fmt.Println(sprint(h.value));
}

} // end main_package

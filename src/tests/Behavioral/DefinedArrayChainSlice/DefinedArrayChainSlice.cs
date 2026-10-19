namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct @base /*[4]uint64*/;

partial struct one /*@base*/;

partial struct two /*one*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fullSliceWritesˢ = (@string)"full-slice writes:"u8;
private static readonly object sharedSlicesˢ = (@string)"shared slices:"u8;
private static readonly object threeIndexˢ = (@string)"three-index:"u8;

internal static void Main() {
    var a = @new<@base>();
    a.Value[..][1] = 5;
    var b = @new<one>();
    b.Value[..][2] = 6;
    var c = @new<two>();
    c.Value[..][3] = 7;
    two d = default!;
    d[..][0] = 8;
    fmt.Println(fullSliceWritesˢ, a.Value[1], b.Value[2], c.Value[3], d[0]);

    var s = c.Value[..];
    s[0] = 9;
    var t = c.Value[1..3];
    t[0] = 10;
    nint n = 2;
    var u = a.Value.slice(n);
    u[1] = 11;
    fmt.Println(sharedSlicesˢ, c.Value[0], c.Value[1], a.Value[3], len(s), len(t), len(u));

    var w = c.Value.slice(0, 2, 3);
    w[1] = 12;
    var x = d.slice(1, 2, 4);
    x[0] = 13;
    fmt.Println(threeIndexˢ, c.Value[1], d[1], len(w), cap(w), len(x), cap(x));
}

} // end main_package

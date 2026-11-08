namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct SE /*[]any*/;

partial struct fileOps /*[]any*/;

partial struct row {
    internal @string format;
    internal SE val;
}

internal static @string describe(this SE s) { return fmt.Sprint(len(s), ":", s); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object twoˢ = (@string)"two"u8;
private static readonly object cdeˢ = (@string)"cde"u8;

internal static void Main() {
    var rows = slice<row>([
        new("%d", new SE(new any[]{(nint)1}.slice())),
        new("%d %s", new SE(new any[]{(nint)2, twoˢ}.slice())),
        new("%6.2f", new SE(new any[]{12.0D}.slice()))
    ]);
    foreach (var (_, r) in range(rows)) {
        fmt.Printf(r.format + "\n"u8, r.val.ꓸꓸꓸ);
    }

    SE s = default;
    s = append(s, (any)((nint)1), (any)(twoˢ), (any)3.5D);
    fmt.Println(len(s), cap(s) >= 3, s);
    fmt.Println(s[0], s[1], s[2]);
    fmt.Println(s[1..], s.describe());

    foreach (var (i, v) in range<any>(s)) {
        fmt.Print(i, "=", v, ";");
    }
    fmt.Println();

    var t = new SE(new any[]{}.slice());
    t = appendꓸꓸꓸ(t, s);
    fmt.Println(len(t), t);

    var ops = new fileOps(new any[]{(@string)"ab", (int64)3, cdeˢ}.slice());
    fmt.Println(len(ops), ops, ops[1..2]);

    SE zero = default;
    fmt.Println(zero == nil, len(zero), zero, new SE(new any[]{}.slice()) == nil);
}

} // end main_package

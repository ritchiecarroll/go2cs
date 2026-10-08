namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct SE /*[]any*/;

partial struct fileOps /*[]any*/;

partial struct row {
    internal @string format;
    internal SE val;
}

internal static @string describe(this SE s) {
    return fmt.Sprint(len(s), (@string)":"u8, s);
}

internal static void Main() {
    var rows = new row[]{
        new("%d"u8, new SE(new any[]{(nint)(1)}.slice())),
        new("%d %s"u8, new SE(new any[]{(nint)(2), (@string)"two"u8}.slice())),
        new("%6.2f"u8, new SE(new any[]{12.0D}.slice()))
    }.slice();
    foreach (var (_, r) in rows) {
        fmt.Printf(r.format + "\n"u8, r.val.ꓸꓸꓸ);
    }
    SE s = default!;
    s = append(s, (any)(1), (any)((@string)"two"u8), (any)(3.5D));
    fmt.Println(len(s), cap(s) >= 3, s);
    fmt.Println(s[0], s[1], s[2]);
    fmt.Println(s[1..], s.describe());
    foreach (var (i, v) in s) {
        fmt.Print(i, (@string)"="u8, v, (@string)";"u8);
    }
    fmt.Println();
    var t = new SE(new any[]{}.slice());
    t = appendꓸꓸꓸ(t, s);
    fmt.Println(len(t), t);
    var ops = new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cde"u8}.slice());
    fmt.Println(len(ops), ops, ops[1..2]);
    SE zero = default!;
    fmt.Println(zero == default!, len(zero), zero, new SE(new any[]{}.slice()) == default!);
}

} // end main_package

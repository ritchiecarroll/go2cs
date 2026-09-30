namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Range {
    public nint Start, End;
}

[GoType("@string")] partial struct ViewType;

[GoType("[]byte")] partial struct byteView;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcdefˢ = "abcdef"u8;

internal static void Main() {
    var r = new Range(Start: 1, End: 4);
    ViewType v = ((ViewType)(@string)abcdefˢ);
    ViewType sub = v.slice(r.Start, r.End);
    fmt.Println(sub);
    fmt.Println(len(sub));
    fmt.Println(v[0]);
    var raw = new byte[]{120, 121, 122, 119}.slice();
    var b = ((byteView)raw);
    var bs = b.slice(r.Start, r.End);
    fmt.Println(len(bs));
    fmt.Println(bs[0]);
    fmt.Println(r.End - r.Start);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt named => 7;
internal static UntypedFloat ratio => 0.5;
internal static UntypedInt letter => /* 'z' */ 122;
internal static readonly @string word = "w"u8;

internal static @string kind(any v) {
    switch (v.type()) {
    case nint: {
        return "int";
    }
    case int32: {
        return "int32 (rune)";
    }
    case float64: {
        return "float64";
    }
    case @string: {
        return "string";
    }}

    return fmt.Sprintf("other %T"u8, v);
}

internal static void main() {
    var pathᴷ = new stackAppend<any>();

    var path = slice<any>([(@string)"a"]);
    path = append(pathᴷ, path, (any)((nint)0));
    path = append(path, (any)((nint)(1 << 3)), (any)((nint)(-5)), (any)((rune)'r'), (any)2.5D, (any)((@string)"s"));
    path = append(path, (any)((nint)(named)), (any)((nint)(named + 1)), (any)((float64)(ratio)), (any)((int32)(letter)), (any)(word), (any)(word + "x"));

    foreach (var (i, v) in range(path)) {
        fmt.Printf("%d: %T %v %s\n"u8, i, v, v, kind(v));
    }

    fmt.Println(AreEqual(path[1], ((any)(nint)0)), AreEqual(path[2], ((any)(nint)8)));
}

} // end main_package

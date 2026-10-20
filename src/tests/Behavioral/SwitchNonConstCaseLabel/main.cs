namespace go;

using fmt = fmt_package;

partial class main_package {

public static UntypedInt Width => 8;

internal static @string classify(uintptr n) {
    var exprᴛ1 = n;
    if (exprᴛ1 == Width) {
        return "width";
    }
    { /* default: */
        return "other";
    }

}

internal static @string matchVar(uintptr p, uintptr a, uintptr b) {
    var exprᴛ1 = p;
    if (exprᴛ1 == a) {
        return "A";
    }
    if (exprᴛ1 == b) {
        return "B";
    }
    { /* default: */
        return "?";
    }

}

internal static void Main() {
    fmt.Println(classify(8));
    fmt.Println(classify(3));
    fmt.Println(matchVar(1, 1, 2));
    fmt.Println(matchVar(2, 1, 2));
    fmt.Println(matchVar(9, 1, 2));
}

} // end main_package

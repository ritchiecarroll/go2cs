namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string classify(nint n) {
    @string @out = default!;
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 1) { matchᴛ1 = true;
        @out = "one";
    }
    else if (exprᴛ1 is 2) { matchᴛ1 = true;
        @out = "two";
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) {
        @out += "-three"u8;
    }
    else if (!matchᴛ1) { /* default: */
        @out += "-other"u8;
    }

    return @out;
}

internal static void Main() {
    fmt.Println(classify(1));
    fmt.Println(classify(2));
    fmt.Println(classify(3));
    fmt.Println(classify(9));
}

} // end main_package

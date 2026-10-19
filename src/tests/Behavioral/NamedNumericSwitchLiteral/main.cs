namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct reply /*num:nint*/;

internal static reply statusOK => 0x00;

internal static @string describe(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == statusOK) {
        return "ok";
    }
    if (exprᴛ1 == (reply)(0x01)) {
        return "general failure";
    }
    if (exprᴛ1 == (reply)(0x02) || exprᴛ1 == (reply)(0x03)) {
        return "blocked";
    }
    { /* default: */
        return "unknown";
    }

}

internal static @string kind(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == (reply)(0x00)) {
        return "zero";
    }
    if (exprᴛ1 == (reply)(0x04)) {
        return "four";
    }

    return "many";
}

internal static void Main() {
    for (nint i = 0; i <= 4; i++) {
        fmt.Println(describe(((reply)i)), kind(((reply)i)));
    }
}

} // end main_package

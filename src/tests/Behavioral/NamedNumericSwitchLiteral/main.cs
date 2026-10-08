namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct reply /*num:nint*/;

internal static reply statusOK => 0x00;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string generalFailureˢ = "general failure"u8;
private static readonly @string blockedˢ = "blocked"u8;
private static readonly @string unknownˢ = "unknown"u8;

internal static @string describe(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == statusOK) {
        return "ok"u8;
    }
    if (exprᴛ1 == (reply)(0x01)) {
        return generalFailureˢ;
    }
    if (exprᴛ1 == (reply)(0x02) || exprᴛ1 == (reply)(0x03)) {
        return blockedˢ;
    }
    { /* default: */
        return unknownˢ;
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroˢ = "zero"u8;
private static readonly @string fourˢ = "four"u8;
private static readonly @string manyˢ = "many"u8;

internal static @string kind(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == (reply)(0x00)) {
        return zeroˢ;
    }
    if (exprᴛ1 == (reply)(0x04)) {
        return fourˢ;
    }

    return manyˢ;
}

internal static void Main() {
    for (nint i = 0; i <= 4; i++) {
        fmt.Println(describe(((reply)i)), kind(((reply)i)));
    }
}

} // end main_package

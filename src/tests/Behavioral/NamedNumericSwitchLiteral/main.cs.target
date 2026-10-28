namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct reply /*num:nint*/;

internal static reply statusOK => 0x00;

internal static @string describe(reply code) {
    switch (code) {
    case var ᴛ when ᴛ == statusOK: {
        return "ok";
    }
    case var ᴛ when ᴛ == (reply)(0x01): {
        return "general failure";
    }
    case var ᴛ when ᴛ == (reply)(0x02) || ᴛ == (reply)(0x03): {
        return "blocked";
    }
    default: {
        return "unknown";
    }}

}

internal static @string kind(reply code) {
    switch (code) {
    case var ᴛ when ᴛ == (reply)(0x00): {
        return "zero";
    }
    case var ᴛ when ᴛ == (reply)(0x04): {
        return "four";
    }}

    return "many";
}

internal static void Main() {
    for (nint i = 0; i <= 4; i++) {
        fmt.Println(describe((reply)i), kind((reply)i));
    }
}

} // end main_package

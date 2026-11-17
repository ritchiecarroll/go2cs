namespace go;

using fmt = fmt_package;

partial class main_package {

internal static int32 @lock(nint typ) {
    var matchᴛ1 = false;
    var matchᴛ2 = typ is 0 || typ is 1;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && typ is 0) {
        return 10;
    }
    if (typ is 1) { matchᴛ1 = true;
        for (nint n = 0; ; n++) {
            if (n == 3) {
                return (int32)(20 + n);
            }
        }
    }
    return default;

}

internal static @string middle(nint n) {
    var matchᴛ1 = false;
    var matchᴛ2 = n is 0 || n is 1;
    if (n is 0) {
        return "zero";
    }
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && n is 1) { matchᴛ1 = true;
        return fmt.Sprint("one or other ", n);
    }
    return default;

}

internal static nint notTerminal(nint n) {
    var nᴛ1 = n;
    var matchᴛ1 = false;
    var matchᴛ2 = nᴛ1 is 0 || nᴛ1 is 1;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && nᴛ1 is 0) {
        n += 100;
    }
    else if (nᴛ1 is 1) { matchᴛ1 = true;
        return 1;
    }

    return n;
}

internal static void main() {
    fmt.Println(@lock(0), @lock(1), @lock(7));
    fmt.Println(middle(0), middle(1), middle(5));
    fmt.Println(notTerminal(0), notTerminal(1), notTerminal(5));
}

} // end main_package

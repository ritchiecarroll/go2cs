namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt isoZone => iota;
internal static UntypedInt numZone => 1;
internal static UntypedInt loopZone => 2;
internal static UntypedInt nameZone => 3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unsetˢ = "unset"u8;
private static readonly @string anonymousˢ = "anonymous"u8;

internal static @string zone(nint kind, @string s) {
    @string @out = unsetˢ;
    var exprᴛ1 = kind;
    var matchᴛ1 = false;
    if (exprᴛ1 == isoZone) { matchᴛ1 = true;
        do {
            if (len(s) > 0 && s[0] == (rune)'Z') {
                @out = "utc(" + s[1..] + ")";
                break;
            }
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == numZone) { matchᴛ1 = true;
        @out = "offset("u8 + s + ")"u8;
    }
    else if (exprᴛ1 == loopZone) { matchᴛ1 = true;
        for (nint i = 0; i < 3; i++) {
            if (i == 1) {
                break;
            }
            s += "!"u8;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == nameZone) {
        do {
            if (s == ""u8) {
                @out = anonymousˢ;
                break;
            }
            @out = "named("u8 + s + ")"u8;
        } while (false);
    }
    else if (!matchᴛ1) { /* default: */
        @out = "other("u8 + s + ")"u8;
    }

    return @out;
}

internal partial struct main_cases /*dyn*/ {
    internal nint kind;
    internal @string s;
}

internal static void Main() {
    var cases = new main_cases[]{
        new(isoZone, "Z07:00"u8),
        new(isoZone, "Z"u8),
        new(isoZone, "+08:00"u8),
        new(numZone, "-05:00"u8),
        new(loopZone, "x"u8),
        new(nameZone, ""u8),
        new(nameZone, "MST"u8),
        new(9, "?"u8)
    }.slice();
    foreach (var (_, c) in cases) {
        fmt.Printf("%d %q -> %s\n"u8, c.kind, c.s, zone(c.kind, c.s));
    }
}

} // end main_package

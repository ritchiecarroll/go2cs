namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint scan(slice<nint> vals) {
    nint best = -1;
    foreach (var (_, v) in vals) {
        if (v < 0) {
            goto keep;
        }
        if (v > best) {
            best = v;
        }
keep:;
    }
    return best;
}

internal static @string gotoEndOfFunc(nint v) {
    @string msg = "small";
    if (v > 10) {
        goto big;
    }
    return msg;
big:
    msg = "big";
    return msg;
}

internal static nint nestedGotoEnd(slice<slice<nint>> rows) {
    nint total = 0;
    foreach (var (_, row) in rows) {
        foreach (var (_, v) in row) {
            if (v == 0) {
                goto skip;
            }
            total += v;
skip:;
        }
    }
    return total;
}

internal static void Main() {
    fmt.Println(scan(slice<nint>([3, -1, 7, 2, -5, 5])));
    fmt.Println(gotoEndOfFunc(3), gotoEndOfFunc(20));
    fmt.Println(nestedGotoEnd(slice<slice<nint>>([slice<nint>([1, 2, 3]), slice<nint>([4, 0, 6]), slice<nint>([7, 8])])));
}

} // end main_package

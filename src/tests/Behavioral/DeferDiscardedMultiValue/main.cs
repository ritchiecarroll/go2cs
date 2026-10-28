namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (nint, nint, error) triple(@string tag, nint n) {
    fmt.Println("ran:", tag, n);
    return (n, n * 2, default);
}

internal static error single(@string tag) {
    fmt.Println("ran:", tag);
    return default;
}

internal static uintptr sysConst => 11;

internal static (uintptr, uintptr, error) quad(uintptr a, uintptr b, uintptr c, uintptr d) {
    fmt.Println("ran: quad", a, b, c, d);
    return (a, b, default);
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        nint n = 1;

        var @base = (uintptr)4;
        var off = (uintptr)2;

        defer(ᴛ1 => triple("multi-value/eager", ᴛ1), n, ref ᒐ);
        defer(() => single("single-result"), ref ᒐ);
        defer(ᴛ1 => quad(sysConst, ᴛ1, 65536, 0), @base + off, ref ᒐ);

        n = 99;
        fmt.Println("body done, n =", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

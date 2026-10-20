namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (nint, nint, error) triple(@string tag, nint n) {
    fmt.Println("ran:", tag, n);
    return (n, n * 2, default!);
}

internal static error single(@string tag) {
    fmt.Println("ran:", tag);
    return default!;
}

internal static uintptr sysConst => 11;

internal static (uintptr, uintptr, error) quad(uintptr a, uintptr b, uintptr c, uintptr d) {
    fmt.Println("ran: quad", a, b, c, d);
    return (a, b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string multiValueEagerˢ = "multi-value/eager"u8;
private static readonly @string singleResultˢ = "single-result"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        nint n = 1;

        var @base = (uintptr)4;
        var off = (uintptr)2;

        defer(triple, multiValueEagerˢ, n, ref ᒐ);
        defer(single, singleResultˢ, ref ᒐ);
        defer(quad, sysConst, @base + off, (uintptr)65536, (uintptr)0, ref ᒐ);

        n = 99;
        fmt.Println("body done, n =", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

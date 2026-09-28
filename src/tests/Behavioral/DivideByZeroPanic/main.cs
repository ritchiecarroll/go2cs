namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"  recovered:"u8;

internal static (nint result, bool recovered) safeDiv(nint a, nint b) {
    nint result = default!;
    bool recovered = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(recoveredˢ, r);
                    (result, recovered) = (-1, true);
                }
            }
        }, ref ᒐ);
        result = quo(a, b);
        (result, recovered) = (result, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (result, recovered);
}

internal static (nint result, bool recovered) safeMod(nint a, nint b) {
    nint result = default!;
    bool recovered = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    (result, recovered) = (-2, true);
                }
            }
        }, ref ᒐ);
        result = rem(a, b);
        (result, recovered) = (result, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (result, recovered);
}

internal static nint divide(nint a, nint b) {
    return quo(a, b);
}

internal static bool /*ok*/ outerGuard(nint a, nint b) {
    bool ok = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != default!) {
                ok = false;
            }
        }, ref ᒐ);
        divide(a, b);
        ok = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return ok;
}

internal static void Main() {
    var (q, rec) = safeDiv(10, 2);
    fmt.Println(q, rec);
    var (q2, rec2) = safeDiv(7, 0);
    fmt.Println(q2, rec2);
    var (m, mrec) = safeMod(9, 0);
    fmt.Println(m, mrec);
    fmt.Println(outerGuard(4, 2));
    fmt.Println(outerGuard(4, 0));
}

} // end main_package

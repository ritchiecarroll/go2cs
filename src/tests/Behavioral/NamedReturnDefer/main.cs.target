namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint /*x*/ incr() {
    nint x = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            x++;
        }, ref ᒐ);
        x = 5;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return x;
}

internal static nint /*x*/ incrBare() {
    nint x = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            x += 10;
        }, ref ᒐ);
        x = 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return x;
}

internal static (nint a, nint b) swapAndBump() {
    nint a = default!;
    nint b = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            (a, b) = (b, a);
            a += 100;
        }, ref ᒐ);
        a = 1;
        b = 2;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, b);
}

internal static nint @double(nint n) {
    return n * 2;
}

internal static nint /*total*/ closures(nint n) {
    nint total = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            total += 1;
        }, ref ᒐ);
        nint dbl(nint x) => x * 2;
        nint noisy() => 99;
        _ = noisy();
        total = dbl(n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string negˢ = "neg"u8;

internal static (nint @out, @string label) compute(nint x) {
    nint @out = default!;
    @string label = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            @out += 1000;
        }, ref ᒐ);
        if (x < 0) {
            (@out, label) = (-1, negˢ);
            goto ᒐdone;
        }
        (@out, label) = (@double(x), fmt.Sprintf("v=%d"u8, x));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (@out, label);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string recoveredˢ = "recovered"u8;

internal static (nint code, @string msg) guarded(bool boom) {
    nint code = default!;
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    code = -1;
                    msg = recoveredˢ;
                }
            }
        }, ref ᒐ);
        if (boom) {
            throw panic("kaboom");
        }
        code = 0;
        msg = "ok"u8;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (code, msg);
}

internal static (ж<box>, error err) parseLimited(nint n) {
    ж<box> _ᴛ1 = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    err = fmt.Errorf("too big: %v"u8, r);
                }
            }
        }, ref ᒐ);
        if (n > 10) {
            throw panic(n);
        }
        (_ᴛ1, err) = (Ꮡ(new box(n * 2)), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (_ᴛ1, err);
}

partial struct box {
    internal nint v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object parseLimited4ˢ = (@string)"parseLimited(4):"u8;
private static readonly object parseLimited99ˢ = (@string)"parseLimited(99):"u8;

internal static void Main() {
    fmt.Println(incr());
    fmt.Println(incrBare());
    var (a, b) = swapAndBump();
    fmt.Println(a, b);
    var (o1, l1) = compute(3);
    fmt.Println(o1, l1);
    var (o2, l2) = compute(-5);
    fmt.Println(o2, l2);
    fmt.Println(closures(5));
    var (c1, m1) = guarded(false);
    fmt.Println(c1, m1);
    var (c2, m2) = guarded(true);
    fmt.Println(c2, m2);
    {
        var (bΔ1, err) = parseLimited(4); if (err == default!) {
            fmt.Println(parseLimited4ˢ, (~bΔ1).v, err);
        }
    }
    var (b2, err2) = parseLimited(99);
    fmt.Println(parseLimited99ˢ, b2 == nil, err2);
}

} // end main_package

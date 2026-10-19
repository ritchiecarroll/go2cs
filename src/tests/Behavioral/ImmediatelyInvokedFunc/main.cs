namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered:"u8;
private static readonly object afterRecoverˢ = (@string)"after recover"u8;
private static readonly object afterArgRecoverˢ = (@string)"after arg recover"u8;
private static readonly object ignoredArgCallsˢ = (@string)"ignored arg; calls ="u8;

internal static void Main() {
    ((Action)(() => {
        fmt.Println((@string)"a"u8);
    }))();

    nint x = ((Func<nint>)(() => {
        return 6 * 7;
    }))();
    fmt.Println(x);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(recoveredˢ, r);
                    }
                }
            }, ref ᒐ);
            throw panic("boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    fmt.Println(afterRecoverˢ);

    nint total = 10 + ((Func<nint>)(() => {
        nint sum = 0;
        for (nint i = 1; i <= 4; i++) {
            sum += i;
        }
        return sum;
    }))();
    fmt.Println(total);

    nint y = ((Func<nint>)(() => {
        return ((Func<nint>)(() => {
            return 5;
        }))() * 2;
    }))();
    fmt.Println(y);

    ((Action<nint>)(n => {
        fmt.Println((@string)"n ="u8, n);
    }))(7);

    nint tri = ((Func<nint, nint, nint, nint>)((a, b, c) => {
        return a + b + c;
    }))(1, 2, 3);
    fmt.Println(tri);

    for (nint k = 0; k < 3; k++) {
        ((Action<nint>)(xΔ1 => {
            fmt.Print(xΔ1, (@string)" "u8);
        }))(k);
    }
    fmt.Println();

    ((Action<@string>)(label => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(label, recoveredˢ, r);
                    }
                }
            }, ref ᒐ);
            throw panic("argboom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))("scope");
    fmt.Println(afterArgRecoverˢ);

    nint calls = 0;
    nint bump() {
        calls++;
        return 99;
    }
    ((Action<nint>)(_ => {
        fmt.Println(ignoredArgCallsˢ, calls);
    }))(bump());
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ((Action)(() => {
        fmt.Println("a");
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
                    var r = recover(); if (r != nil) {
                        fmt.Println("recovered:", r);
                    }
                }
            }, ref ᒐ);
            throw panic("boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    fmt.Println("after recover");

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
        fmt.Println("n =", n);
    }))(7);

    nint tri = ((Func<nint, nint, nint, nint>)((a, b, c) => {
        return a + b + c;
    }))(1, 2, 3);
    fmt.Println(tri);

    for (nint k = 0; k < 3; k++) {
        ((Action<nint>)(xΔ1 => {
            fmt.Print(xΔ1, " ");
        }))(k);
    }
    fmt.Println();

    ((Action<@string>)(label => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println(label, "recovered:", r);
                    }
                }
            }, ref ᒐ);
            throw panic("argboom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))("scope");
    fmt.Println("after arg recover");

    nint calls = 0;
    nint bump() {
        calls++;
        return 99;
    }
    ((Action<nint>)(_ => {
        fmt.Println("ignored arg; calls =", calls);
    }))(bump());
}

} // end main_package

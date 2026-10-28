namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    func(() => {
        fmt.Println("a");
    })();

    nint x = func(nint () => {
        return 6 * 7;
    })();
    fmt.Println(x);

    func(() => {
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
    })();
    fmt.Println("after recover");

    nint total = 10 + func(nint () => {
        nint sum = 0;
        for (nint i = 1; i <= 4; i++) {
            sum += i;
        }
        return sum;
    })();
    fmt.Println(total);

    nint y = func(nint () => {
        return func(nint () => {
            return 5;
        })() * 2;
    })();
    fmt.Println(y);

    func((nint n) => {
        fmt.Println("n =", n);
    })(7);

    nint tri = func(nint (nint a, nint b, nint c) => {
        return a + b + c;
    })(1, 2, 3);
    fmt.Println(tri);

    for (nint k = 0; k < 3; k++) {
        func((nint xΔ1) => {
            fmt.Print(xΔ1, " ");
        })(k);
    }
    fmt.Println();

    func((@string label) => {
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
    })("scope");
    fmt.Println("after arg recover");

    nint calls = 0;
    nint bump() {
        calls++;
        return 99;
    }
    func((nint _) => {
        fmt.Println("ignored arg; calls =", calls);
    })(bump());
}

} // end main_package

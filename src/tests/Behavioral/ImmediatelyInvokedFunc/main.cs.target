namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string drain(@string _Δp0, channel<EmptyStruct> ch) {
    var chᴛ1 = ch;
    switch (select(ᐸꟷ(chᴛ1, ꓸꓸꓸ))) {
    case 0 when chᴛ1.ꟷᐳ(out _): {
        return "drained";
    }}
    return default;
}

internal static void main() {
    func([GoFunc("main.func1")] () => {
        fmt.Println("a");
    })();

    nint x = func([GoFunc("main.func2")] nint () => {
        return 6 * 7;
    })();
    fmt.Println(x);

    func([GoFunc("main.func3")] () => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func3.1")] () => {
                var r = recover(); if (r != nil) {
                    fmt.Println("recovered:", r);
                }
            }, ref ᒐ);
            throw panic("boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    fmt.Println("after recover");

    nint total = 10 + func([GoFunc("main.func4")] nint () => {
        nint sum = 0;
        for (nint i = 1; i <= 4; i++) {
            sum += i;
        }
        return sum;
    })();
    fmt.Println(total);

    nint y = func([GoFunc("main.func5")] nint () => {
        return func([GoFunc("main.func5.1")] nint () => {
            return 5;
        })() * 2;
    })();
    fmt.Println(y);

    func([GoFunc("main.func6")] (nint n) => {
        fmt.Println("n =", n);
    })(7);

    nint tri = func([GoFunc("main.func7")] nint (nint a, nint b, nint c) => {
        return a + b + c;
    })(1, 2, 3);
    fmt.Println(tri);

    for (nint k = 0; k < 3; k++) {
        func([GoFunc("main.func8")] (nint xΔ1) => {
            fmt.Print(xΔ1, " ");
        })(k);
    }
    fmt.Println();

    func([GoFunc("main.func9")] (@string label) => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func9.1")] () => {
                var r = recover(); if (r != nil) {
                    fmt.Println(label, "recovered:", r);
                }
            }, ref ᒐ);
            throw panic("argboom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })("scope");
    fmt.Println("after arg recover");

    nint calls = 0;
    [GoFunc("main.func10")] nint bump() {
        calls++;
        return 99;
    }
    func([GoFunc("main.func11")] (nint _) => {
        fmt.Println("ignored arg; calls =", calls);
    })(bump());

    var done = new channel<EmptyStruct>(0);
    close(done);
    fmt.Println(drain("unused", done));
}

} // end main_package

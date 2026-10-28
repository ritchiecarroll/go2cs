namespace go;

using fmt = fmt_package;

partial class main_package {

internal static Action makeGreeter(@string name) {
    return () => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => fmt.Println("bye", ᴛ1), name, ref ᒐ);
            fmt.Println("hi", name);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

internal static void Main() {
    void f() {
        GoFrame ᒐ = default;
        try {
            defer(() => fmt.Println("deferred"), ref ᒐ);
            fmt.Println("body");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    f();

    void divPrint(nint a, nint b) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println("closure recovered:", r);
                    }
                }
            }, ref ᒐ);
            fmt.Println(quo(a, b));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    divPrint(20, 4);
    divPrint(1, 0);

    nint /*result*/ safeDiv(nint a, nint b) {
        nint result = default;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        result = -1;
                    }
                }
            }, ref ᒐ);
            result = quo(a, b); goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return result;
    }
    fmt.Println(safeDiv(20, 4));
    fmt.Println(safeDiv(1, 0));

    nint /*n*/ counted() {
        nint n = default;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                n++;
            }, ref ᒐ);
            n = 10;
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return n;
    }
    fmt.Println(counted());

    var greet = makeGreeter("go2cs");
    greet();

    void run(Action fn) {
        fn();
    }
    run(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => fmt.Println("arg-closure deferred"), ref ᒐ);
            fmt.Println("arg-closure body");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println("outer recovered:", r);
                    }
                }
            }, ref ᒐ);
            throw panic("from-iife");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    (nint, error) fetch() {
        GoFrame ᒐ = default;
        try {
            defer(() => fmt.Println("fetch deferred"), ref ᒐ);
            return (42, default);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    }
    var (v, err) = fetch();
    var tk = makeTask(5);
    fmt.Println("task:", (~tk).fn(), (~tk).name);
    fmt.Println("fetched:", v, err);

    fmt.Println("done");
}

partial struct task {
    internal Func<nint> fn;
    internal @string name;
}

internal static ж<task> makeTask(nint @base) {
    nint bonus = @base * 2;
    return Ꮡ(new task(fn: () => bonus + 1, name: "t"));
}

} // end main_package

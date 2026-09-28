namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object byeˢ = (@string)"bye"u8;

internal static Action makeGreeter(@string name) {
    return () => {
        GoFrame ᒐ = default;
        try {
            defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), byeˢ, name, ref ᒐ);
            fmt.Println((@string)"hi"u8, name);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredˢ = (@string)"deferred"u8;
private static readonly object bodyˢ = (@string)"body"u8;
private static readonly object closureRecoveredˢ = (@string)"closure recovered:"u8;
private static readonly @string go2csˢ = "go2cs"u8;
private static readonly object argClosureDeferredˢ = (@string)"arg-closure deferred"u8;
private static readonly object argClosureBodyˢ = (@string)"arg-closure body"u8;
private static readonly object outerRecoveredˢ = (@string)"outer recovered:"u8;
private static readonly object fetchDeferredˢ = (@string)"fetch deferred"u8;
private static readonly object taskˢ = (@string)"task:"u8;
private static readonly object fetchedˢ = (@string)"fetched:"u8;
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    void f() {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => fmt.Println(ᴛ1), deferredˢ, ref ᒐ);
            fmt.Println(bodyˢ);
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
                    var r = recover(); if (r != default!) {
                        fmt.Println(closureRecoveredˢ, r);
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
        nint result = default!;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
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
        nint n = default!;
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
    var greet = makeGreeter(go2csˢ);
    greet();
    void run(Action fn) {
        fn();
    }
    run(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => fmt.Println(ᴛ1), argClosureDeferredˢ, ref ᒐ);
            fmt.Println(argClosureBodyˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(outerRecoveredˢ, r);
                    }
                }
            }, ref ᒐ);
            throw panic("from-iife");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    (nint, error) fetch() {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => fmt.Println(ᴛ1), fetchDeferredˢ, ref ᒐ);
            return (42, default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    var (v, err) = fetch();
    var tk = makeTask(5);
    fmt.Println(taskˢ, (~tk).fn(), (~tk).name);
    fmt.Println(fetchedˢ, v, err);
    fmt.Println(doneˢ);
}

[GoType] partial struct task {
    internal Func<nint> fn;
    internal @string name;
}

internal static ж<task> makeTask(nint @base) {
    nint bonus = @base * 2;
    return Ꮡ(new task(fn: () => bonus + 1, name: "t"u8));
}

} // end main_package

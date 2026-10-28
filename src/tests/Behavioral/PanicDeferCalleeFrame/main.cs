namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> trace;

internal static void note(@string s) {
    trace = append(trace, s);
}

internal static void show(@string label, bool open) {
    fmt.Println(label, "open =", open, trace);
    trace = default;
}

internal static bool open;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unlockˢ = "unlock"u8;

internal static void withLock(Action fn) {
    GoFrame ᒐ = default;
    try {
        note("lock");
        defer(note, unlockˢ, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void release() {
    withLock(() => {
        note("release");
    });
    open = false;
    note("closed");
}

internal static void raw(Action f) {
    GoFrame ᒐ = default;
    try {
        note("acquire");
        defer(() => {
            note("cleanup begin");
            release();
            note("cleanup end");
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep3Deferˢ = "deep3 defer"u8;

internal static void deep3() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep3Deferˢ, ref ᒐ);
        note("deep3 body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep2Deferˢ = "deep2 defer"u8;

internal static void deep2() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep2Deferˢ, ref ᒐ);
        deep3();
        note("deep2 after");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep1Deferˢ = "deep1 defer"u8;

internal static void deep1() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep1Deferˢ, ref ᒐ);
        deep2();
        note("deep1 after");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deepCleanup() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note("deep cleanup begin");
            deep1();
            note("deep cleanup end");
        }, ref ᒐ);
        throw panic("deep boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void calleeDeferPanics() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note("callee defer");
            throw panic("callee boom");
        }, ref ᒐ);
        note("callee body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void calleeDeferPanicsDuringPanic() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note("outer cleanup");
            calleeDeferPanics();
            note("unreachable");
        }, ref ᒐ);
        throw panic("outer boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    open = true;
    raw(() => {
        note("callback ok");
    });
    show("normal:", open);

    open = true;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            raw(() => {
                throw panic("callback boom");
            });
            note("unreachable");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show("panicked:", open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            deepCleanup();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show("deep:", open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            calleeDeferPanics();
            note("unreachable");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show("callee panic:", open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            calleeDeferPanicsDuringPanic();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show("callee panic during panic:", open);
}

} // end main_package

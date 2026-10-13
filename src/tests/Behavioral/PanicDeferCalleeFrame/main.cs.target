namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> trace;

internal static void note(@string s) {
    trace = append(trace, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object openˢ = (@string)"open ="u8;

internal static void show(@string label, bool open) {
    fmt.Println(label, openˢ, open, trace);
    trace = default!;
}

internal static bool open;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string lockˢ = "lock"u8;
private static readonly @string unlockˢ = "unlock"u8;

internal static void withLock(Action fn) {
    GoFrame ᒐ = default;
    try {
        note(lockˢ);
        defer(note, unlockˢ, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string releaseˢ = "release"u8;
private static readonly @string closedˢ = "closed"u8;

internal static void release() {
    withLock(() => {
        note(releaseˢ);
    });
    open = false;
    note(closedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string acquireˢ = "acquire"u8;
private static readonly @string cleanupBeginˢ = "cleanup begin"u8;
private static readonly @string cleanupEndˢ = "cleanup end"u8;

internal static void raw(Action f) {
    GoFrame ᒐ = default;
    try {
        note(acquireˢ);
        defer(() => {
            note(cleanupBeginˢ);
            release();
            note(cleanupEndˢ);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep3Deferˢ = "deep3 defer"u8;
private static readonly @string deep3Bodyˢ = "deep3 body"u8;

internal static void deep3() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep3Deferˢ, ref ᒐ);
        note(deep3Bodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep2Deferˢ = "deep2 defer"u8;
private static readonly @string deep2Afterˢ = "deep2 after"u8;

internal static void deep2() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep2Deferˢ, ref ᒐ);
        deep3();
        note(deep2Afterˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deep1Deferˢ = "deep1 defer"u8;
private static readonly @string deep1Afterˢ = "deep1 after"u8;

internal static void deep1() {
    GoFrame ᒐ = default;
    try {
        defer(note, deep1Deferˢ, ref ᒐ);
        deep2();
        note(deep1Afterˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deepCleanupBeginˢ = "deep cleanup begin"u8;
private static readonly @string deepCleanupEndˢ = "deep cleanup end"u8;

internal static void deepCleanup() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note(deepCleanupBeginˢ);
            deep1();
            note(deepCleanupEndˢ);
        }, ref ᒐ);
        throw panic("deep boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string calleeDeferˢ = "callee defer"u8;
private static readonly @string calleeBodyˢ = "callee body"u8;

internal static void calleeDeferPanics() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note(calleeDeferˢ);
            throw panic("callee boom");
        }, ref ᒐ);
        note(calleeBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string outerCleanupˢ = "outer cleanup"u8;
private static readonly @string unreachableˢ = "unreachable"u8;

internal static void calleeDeferPanicsDuringPanic() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            note(outerCleanupˢ);
            calleeDeferPanics();
            note(unreachableˢ);
        }, ref ᒐ);
        throw panic("outer boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string callbackOkˢ = "callback ok"u8;
private static readonly @string normalˢ = "normal:"u8;
private static readonly object recoveredˢ = (@string)"recovered:"u8;
private static readonly @string panickedˢ = "panicked:"u8;
private static readonly @string deepˢ = "deep:"u8;
private static readonly @string calleePanicˢ = "callee panic:"u8;
private static readonly @string calleePanicDuringPanicˢ = "callee panic during panic:"u8;

internal static void Main() {
    open = true;
    raw(() => {
        note(callbackOkˢ);
    });
    show(normalˢ, open);

    open = true;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println(recoveredˢ, recover());
            }, ref ᒐ);
            raw(() => {
                throw panic("callback boom");
            });
            note(unreachableˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show(panickedˢ, open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println(recoveredˢ, recover());
            }, ref ᒐ);
            deepCleanup();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show(deepˢ, open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println(recoveredˢ, recover());
            }, ref ᒐ);
            calleeDeferPanics();
            note(unreachableˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show(calleePanicˢ, open);

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println(recoveredˢ, recover());
            }, ref ᒐ);
            calleeDeferPanicsDuringPanic();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    show(calleePanicDuringPanicˢ, open);
}

} // end main_package

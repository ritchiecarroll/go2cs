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

internal static void withLock(Action fn) {
    GoFrame ᒐ = default;
    try {
        note("lock");
        defer(() => note("unlock"), ref ᒐ);
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

internal static void deep3() {
    GoFrame ᒐ = default;
    try {
        defer(() => note("deep3 defer"), ref ᒐ);
        note("deep3 body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deep2() {
    GoFrame ᒐ = default;
    try {
        defer(() => note("deep2 defer"), ref ᒐ);
        deep3();
        note("deep2 after");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deep1() {
    GoFrame ᒐ = default;
    try {
        defer(() => note("deep1 defer"), ref ᒐ);
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
    func(() => {
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
    })();
    show("panicked:", open);

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            deepCleanup();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    show("deep:", open);

    func(() => {
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
    })();
    show("callee panic:", open);

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            calleeDeferPanicsDuringPanic();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    show("callee panic during panic:", open);
}

} // end main_package

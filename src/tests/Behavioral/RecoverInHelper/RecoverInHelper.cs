namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var (got, outer) = helperGetsNil();
    fmt.Println("1 helper:", got, "| outer:", outer);

    fmt.Println("2 direct:", directWorks());

    fmt.Println("3 defer f(&err):", deferNamedWithPointer(), "| gob shape:", deferGobShape());

    var g = deferMethodValue();
    fmt.Println("4 method value:", (~g).got, "| with args:", deferMethodValueWithArgs());

    fmt.Println("5 defer recover():", deferRecoverItself());

    var (inner, outer6) = namedFunctionCalledNormally();
    fmt.Println("6 called normally:", inner, "| outer:", outer6);

    fmt.Println("7 func value:", deferFuncValue());
}

internal static any helperRecover() {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        return recover(calledByDefer);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (any got, any outer) helperGetsNil() {
    any got = default;
    any outer = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            outer = recover();
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    got = helperRecover();
                }, ref ᒐ);
                throw panic("one");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (got, outer);
}

internal static any /*got*/ directWorks() {
    any got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            got = recover();
        }, ref ᒐ);
        throw panic("two");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static void catchError(ж<error> Ꮡerr) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var err = ref Ꮡerr.DerefOrNull();

        {
            var e = recover(calledByDefer); if (e != nil) {
                err = fmt.Errorf("caught: %v"u8, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error /*err*/ deferNamedWithPointer() {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(catchError, Ꮡerr, ref ᒐ, recovers: true);
        throw panic("three");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

partial struct gobError {
    internal error err;
}

internal static void catchGobError(ref error err) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != nil) {
                var (ge, ok) = e._<gobError>(ᐧ);
                if (!ok) {
                    throw panic(e);
                }
                err = ge.err;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error /*err*/ deferGobShape() {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(ᴛ1 => catchGobError(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ, recovers: true);
        throw panic(new gobError(fmt.Errorf("three-b"u8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

partial struct guard {
    internal any got;
}

internal static void @catch(this ж<guard> Ꮡg) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var g = ref Ꮡg.DerefOrNull();

        g.got = recover(calledByDefer);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void catchTagged(this ж<guard> Ꮡg, @string tag) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var g = ref Ꮡg.DerefOrNull();

        g.got = fmt.Sprint(tag, ":", recover(calledByDefer));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<guard> /*g*/ deferMethodValue() {
    ж<guard> g = default;
    GoFrame ᒐ = default;
    try {
        g = Ꮡ(new guard());
        defer(g.@catch, ref ᒐ, recovers: true);
        throw panic("four");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return g;
}

internal static any deferMethodValueWithArgs() {
    var g = Ꮡ(new guard());
    var gʗ1 = g;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            var gʗ2 = gʗ1;
            defer(gʗ2.catchTagged, (@string)"t", ref ᒐ, recovers: true);
            throw panic("four-b");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    return (~g).got;
}

internal static any /*outer*/ deferRecoverItself() {
    any outer = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            outer = recover();
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => recover(calledByDefer: false), ref ᒐ);
                throw panic("five");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return outer;
}

internal static (error inner, any outer) namedFunctionCalledNormally() {
    heap<error>(out var Ꮡinner);
    any outer = default;
    GoFrame ᒐ = default;
    try {
        ref var inner = ref Ꮡinner.ValueSlot;

        defer(() => {
            outer = recover();
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    catchError(Ꮡinner);
                }, ref ᒐ);
                throw panic("six");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (Ꮡinner.ValueSlot, outer);
}

internal static error /*err*/ deferFuncValue() {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        var h = catchError;
        var hʗ1 = h;
        defer(hʗ1, Ꮡerr, ref ᒐ, recovers: true);
        throw panic("seven");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

} // end main_package

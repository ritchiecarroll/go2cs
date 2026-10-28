namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void setErr(ref error err) {
    err = fmt.Errorf("written via pointer"u8);
}

internal static void addOne(ref nint n) {
    n++;
}

internal static void handlePanic(ref error err) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != nil) {
                err = fmt.Errorf("recovered: %v"u8, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error /*err*/ errResult(bool fail) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(ᴛ1 => setErr(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ);
        if (fail) {
            err = fmt.Errorf("original"u8); goto ᒐdone;
        }
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

internal static nint /*n*/ intResult() {
    heap<nint>(out var Ꮡn);
    GoFrame ᒐ = default;
    try {
        ref var n = ref Ꮡn.Value;

        defer(ᴛ1 => addOne(ref ᴛ1.DerefOrNull()), Ꮡn, ref ᒐ);
        n = 5;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡn.Value;
}

internal static error /*err*/ recoverResult() {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(ᴛ1 => handlePanic(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ, recovers: true);
        throw panic("boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

internal static void Main() {
    fmt.Println(errResult(false));
    fmt.Println(errResult(true));
    fmt.Println(intResult());
    fmt.Println(recoverResult());
}

} // end main_package

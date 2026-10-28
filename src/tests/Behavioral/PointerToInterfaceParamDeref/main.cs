namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void handle(@string label, ref error err) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != nil) {
                var prev = err;
                throw panic(fmt.Sprintf("wrapped %s: %v (prev=%v)"u8, label, e, prev));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string doitˢ = "doit"u8;

internal static error /*err*/ doit() {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer((ᴛ1, ᴛ2) => handle(ᴛ1, ref ᴛ2.DerefOrNull()), doitˢ, Ꮡerr, ref ᒐ, recovers: true);
        throw panic("boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (e != nil) {
                    fmt.Println(e);
                }
            }
        }, ref ᒐ);
        doit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal @string msg;
}

internal static @string Error(this ref myErr e) {
    return e.msg;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueCaptureRecoveredˢ = (@string)"valueCapture recovered:"u8;

internal static void valueCapture() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(valueCaptureRecoveredˢ, recover());
        }, ref ᒐ);
        defer(ᴛ1 => throw panic(ᴛ1), (nint)(42), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object reassignedAfterDeferˢ = (@string)"reassignedAfterDefer recovered:"u8;

internal static void reassignedAfterDefer() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(reassignedAfterDeferˢ, recover());
        }, ref ᒐ);
        var err = fmt.Errorf("first"u8);
        defer(ᴛ1 => throw panic(ᴛ1), err, ref ᒐ);
        err = fmt.Errorf("second"u8);
        _ = err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object computedExprRecoveredˢ = (@string)"computedExpr recovered:"u8;

internal static void computedExpr() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(computedExprRecoveredˢ, recover());
        }, ref ᒐ);
        nint n = 3;
        defer(ᴛ1 => throw panic(ᴛ1), n * 7 + 1, ref ᒐ);
        n = 100;
        _ = n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pointerValueRecoveredˢ = (@string)"pointerValue recovered typed:"u8;
private static readonly object pointerValueRecoveredˢ2 = (@string)"pointerValue recovered UNTYPED:"u8;

internal static void pointerValue() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            {
                var (e, ok) = r._<ж<myErr>>(ᐧ); if (ok){
                    fmt.Println(pointerValueRecoveredˢ, (~e).msg);
                } else {
                    fmt.Println(pointerValueRecoveredˢ2, r);
                }
            }
        }, ref ᒐ);
        var p = Ꮡ(new myErr(msg: "boxed"u8));
        defer(ᴛ1 => throw panic(ᴛ1), p.OrTypedNil(), ref ᒐ);
        p = Ꮡ(new myErr(msg: "replaced"u8));
        _ = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object replacesInFlightˢ = (@string)"replacesInFlight recovered:"u8;

internal static void replacesInFlight() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(replacesInFlightˢ, recover());
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => throw panic(ᴛ1), "from defer", ref ᒐ);
                throw panic("original");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object multipleRecoveredˢ = (@string)"multiple recovered:"u8;
private static readonly @string aChangedˢ = "A-changed"u8;
private static readonly @string bChangedˢ = "B-changed"u8;

internal static void multiple() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(multipleRecoveredˢ, recover());
        }, ref ᒐ);
        @string a = "A"u8;
        @string b = "B"u8;
        defer(ᴛ1 => throw panic(ᴛ1), a, ref ᒐ);
        defer(ᴛ1 => throw panic(ᴛ1), b, ref ᒐ);
        a = aChangedˢ;
        b = bChangedˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    valueCapture();
    reassignedAfterDefer();
    computedExpr();
    pointerValue();
    replacesInFlight();
    multiple();
}

} // end main_package

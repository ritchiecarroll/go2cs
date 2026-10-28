namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal @string msg;
}

internal static @string Error(this ref myErr e) {
    return e.msg;
}

internal static void valueCapture() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("valueCapture recovered:", recover());
        }, ref ᒐ);
        defer(ᴛ1 => throw panic(ᴛ1), (nint)42, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void reassignedAfterDefer() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("reassignedAfterDefer recovered:", recover());
        }, ref ᒐ);
        var err = fmt.Errorf("first"u8);
        defer(ᴛ1 => throw panic(ᴛ1), err, ref ᒐ);
        err = fmt.Errorf("second"u8);
        _ = err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void computedExpr() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("computedExpr recovered:", recover());
        }, ref ᒐ);
        nint n = 3;
        defer(ᴛ1 => throw panic(ᴛ1), n * 7 + 1, ref ᒐ);
        n = 100;
        _ = n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pointerValue() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            {
                var (e, ok) = r._<ж<myErr>>(ᐧ); if (ok) {
                    fmt.Println("pointerValue recovered typed:", (~e).msg);
                } else {
                    fmt.Println("pointerValue recovered UNTYPED:", r);
                }
            }
        }, ref ᒐ);
        var p = Ꮡ(new myErr(msg: "boxed"));
        defer(ᴛ1 => throw panic(ᴛ1), p.OrTypedNil(), ref ᒐ);
        p = Ꮡ(new myErr(msg: "replaced"));
        _ = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void replacesInFlight() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("replacesInFlight recovered:", recover());
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => throw panic(ᴛ1), (@string)"from defer", ref ᒐ);
                throw panic("original");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void multiple() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("multiple recovered:", recover());
        }, ref ᒐ);
        @string a = "A";
        @string b = "B";
        defer(ᴛ1 => throw panic(ᴛ1), a, ref ᒐ);
        defer(ᴛ1 => throw panic(ᴛ1), b, ref ᒐ);
        a = "A-changed";
        b = "B-changed";
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

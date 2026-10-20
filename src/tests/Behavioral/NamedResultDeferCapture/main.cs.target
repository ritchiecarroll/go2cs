namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (nint, error) pair(nint n) {
    if (n < 0) {
        return (0, fmt.Errorf("negative %d"u8, n));
    }
    return (n * 2, default!);
}

internal static (int64 written, error err) send(nint n) {
    int64 written = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            fmt.Println("hook:", written, Ꮡerr.ValueSlot, written > 0);
        }, ref ᒐ);
        (var v, err) = pair(n);
        if (err != default!) {
            (written, err) = (0, fmt.Errorf("send: %w"u8, err));
            goto ᒐdone;
        }
        written = (int64)v;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, Ꮡerr.ValueSlot);
}

internal static nint /*x*/ addrv() {
    ref var x = ref heap(new nint(), out var Ꮡx);

    nint y = 1;
    x = 2;
    _ = y;
    var p = Ꮡx;
    p.Value = 5;
    return x;
}

internal static (int64, error) lit(nint n) {
    (int64 w, error e) f() {
        int64 w = default!;
        heap<error>(out var Ꮡe);
        GoFrame ᒐ = default;
        try {
            ref var e = ref Ꮡe.ValueSlot;
            defer(() => {
                fmt.Println("lit hook:", w, Ꮡe.ValueSlot);
            }, ref ᒐ);
            (var v, Ꮡe.ValueSlot) = pair(n);
            if (Ꮡe.ValueSlot != default!) {
                goto ᒐdone;
            }
            w = (int64)v;
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return (w, Ꮡe.ValueSlot);
    }
    return f();
}

internal static error /*err*/ cls(nint n) {
    ref var err = ref heap<error>(out var Ꮡerr);

    void set() {
        Ꮡerr.ValueSlot = fmt.Errorf("cls %d"u8, n);
    }
    (var v, err) = pair(n);
    fmt.Println("cls pair:", v, err);
    set();
    return err;
}

internal static void Main() {
    var (w, e) = send(3);
    fmt.Println("send:", w, e);
    (w, e) = send(-1);
    fmt.Println("send:", w, e);
    fmt.Println("addrv:", addrv());
    (w, e) = lit(4);
    fmt.Println("lit:", w, e);
    (w, e) = lit(-2);
    fmt.Println("lit:", w, e);
    fmt.Println("cls:", cls(5));
}

} // end main_package

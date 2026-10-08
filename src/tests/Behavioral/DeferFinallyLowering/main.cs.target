namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tracer {
    internal @string id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object touchˢ = (@string)"touch"u8;

internal static void touch(this ref tracer t) {
    fmt.Println(touchˢ, t.id);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void done(this ref tracer t) {
    fmt.Println(doneˢ, t.id);
}

partial struct box {
    internal tracer a;
    internal tracer b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyTwoˢ = (@string)"body two"u8;

internal static void two(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        x.b.touch();
        ᒐd1 = true;
        ᒐd2 = true;
        fmt.Println(bodyTwoˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

internal static void boom(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        ᒐd1 = true;
        throw panic("boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object earlyOutˢ = (@string)"early out"u8;
private static readonly object bodyEarlyˢ = (@string)"body early"u8;

internal static void early(this ж<box> Ꮡx, bool skip) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        if (skip) {
            fmt.Println(earlyOutˢ);
            return;
        }
        ᒐd1 = true;
        fmt.Println(bodyEarlyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyMixedˢ = (@string)"body mixed"u8;

internal static void mixed(this ж<box> Ꮡx, bool f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        x.b.touch();
        ᒐd1 = true;
        if (f) {
            ᒐd2 = true;
        }
        fmt.Println(bodyMixedˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyUnguardedˢ = (@string)"body unguarded"u8;

internal static void unguarded(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        defer(Ꮡx.of(box.Ꮡb).done, ref ᒐ);
        fmt.Println(bodyUnguardedˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object innerRecoveredˢ = (@string)"inner recovered"u8;
private static readonly object bodyWithLitˢ = (@string)"body withLit"u8;

internal static void withLit(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        ᒐd1 = true;
        void f() {
            GoFrame ᒐ = default;
            try {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(innerRecoveredˢ, r);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        f();
        fmt.Println(bodyWithLitˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyCondPrefixˢ = (@string)"body condPrefix"u8;

internal static void condPrefix(this ж<box> Ꮡx, bool f) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        if (f) {
            x.b.touch();
        }
        defer(Ꮡx.of(box.Ꮡb).done, ref ᒐ);
        fmt.Println(bodyCondPrefixˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object finishˢ = (@string)"finish"u8;

internal static void finish(this ref box x) {
    fmt.Println(finishˢ, x.a.id);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyMethodShapeˢ = (@string)"body methodShape"u8;

internal static void methodShape(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        ᒐd1 = true;
        fmt.Println(bodyMethodShapeˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().finish(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyWriteShapeˢ = (@string)"body writeShape"u8;

internal static void writeShape(this ж<box> Ꮡx, bool isFile) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        {
            @string id = x.a.id; if (id == ""u8) {
                return;
            }
        }
        ᒐd1 = true;
        if (isFile) {
            x.b.touch();
            ᒐd2 = true;
        }
        fmt.Println(bodyWriteShapeˢ, isFile);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().finish(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bodyReboundˢ = (@string)"body rebound"u8;

internal static void rebound(this ж<box> Ꮡx, ж<box> Ꮡother) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        defer(Ꮡx.of(box.Ꮡa).done, ref ᒐ);
        Ꮡx = Ꮡother; x = ref Ꮡx.DerefOrNull();
        fmt.Println(bodyReboundˢ, x.a.id);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered"u8;

internal static void Main() {
    var x = Ꮡ(new box(a: new tracer(id: "a"u8), b: new tracer(id: "b"u8)));
    x.two();
    var xʗ1 = x;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(recoveredˢ, r);
                    }
                }
            }, ref ᒐ);
            xʗ1.boom();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    x.early(true);
    x.early(false);
    x.mixed(true);
    x.unguarded();
    x.condPrefix(false);
    x.methodShape();
    x.writeShape(true);
    x.writeShape(false);
    x.withLit();
    var other = Ꮡ(new box(a: new tracer(id: "other-a"u8), b: new tracer(id: "other-b"u8)));
    x.rebound(other);
}

} // end main_package

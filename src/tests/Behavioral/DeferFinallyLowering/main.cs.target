namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tracer {
    internal @string id;
}

internal static void touch(this ref tracer t) {
    fmt.Println("touch", t.id);
}

internal static void done(this ref tracer t) {
    fmt.Println("done", t.id);
}

partial struct box {
    internal tracer a;
    internal tracer b;
}

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
        fmt.Println("body two");
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

internal static void early(this ж<box> Ꮡx, bool skip) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();

        if (skip) {
            fmt.Println("early out");
            return;
        }

        ᒐd1 = true;
        fmt.Println("body early");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

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

        fmt.Println("body mixed");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

internal static void unguarded(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        defer(Ꮡx.of(box.Ꮡb).done, ref ᒐ);
        fmt.Println("body unguarded");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

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
                        fmt.Println("inner recovered", r);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }

        f();
        fmt.Println("body withLit");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}

internal static void condPrefix(this ж<box> Ꮡx, bool f) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();

        if (f) {
            x.b.touch();
        }

        defer(Ꮡx.of(box.Ꮡb).done, ref ᒐ);
        fmt.Println("body condPrefix");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void finish(this ref box x) {
    fmt.Println("finish", x.a.id);
}

internal static void methodShape(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        ᒐd1 = true;
        fmt.Println("body methodShape");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡx.DerefOrNull().finish(); ᒐ.Run(); }
}

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

        fmt.Println("body writeShape", isFile);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().finish(); ᒐ.Run(); }
}

internal static void rebound(this ж<box> Ꮡx, ж<box> Ꮡother) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        defer(Ꮡx.of(box.Ꮡa).done, ref ᒐ);
        Ꮡx = Ꮡother; x = ref Ꮡx.DerefOrNull();
        fmt.Println("body rebound", x.a.id);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var x = Ꮡ(new box(a: new tracer(id: "a"), b: new tracer(id: "b")));

    x.two();

    var xʗ1 = x;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println("recovered", r);
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

    var other = Ꮡ(new box(a: new tracer(id: "other-a"), b: new tracer(id: "other-b")));
    x.rebound(other);
}

} // end main_package

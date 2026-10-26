namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void reset(this ref counter c) {
    c.n = 0;
}

internal static void inc(this ref counter c) {
    c.n++;
}

partial struct builder {
    internal counter c;
}

internal static void run(this ж<builder> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        defer(Ꮡb.of(builder.Ꮡc).reset, ref ᒐ);
        b.c.inc();
        b.c.inc();
        fmt.Println("inside:", b.c.n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var b = Ꮡ(new builder(nil));
    b.run();
    fmt.Println("after:", (~b).c.n);

    var x = Ꮡ(new builder(nil));
    var xʗ1 = x;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            var xʗ2 = xʗ1;
            defer(xʗ2.of(builder.Ꮡc).reset, ref ᒐ);
            xʗ1.of(builder.Ꮡc).inc();
            fmt.Println("inside2:", (~xʗ1).c.n);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    fmt.Println("after2:", (~x).c.n);
}

} // end main_package

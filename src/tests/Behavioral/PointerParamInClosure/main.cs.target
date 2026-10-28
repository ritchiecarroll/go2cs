namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void setViaDefer(ж<nint> p) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            p.Value = 42;
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void bumpInClosure(ж<nint> p) {
    void add() {
        p.Value = p.Value + 1;
    }
    add();
    add();
}

internal static void mixed(ж<nint> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        p = 5;
        defer(() => {
            Ꮡp.Value = Ꮡp.Value * 10;
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ref var a = ref heap(new nint(), out var Ꮡa);
    setViaDefer(Ꮡa);
    fmt.Println(a);

    ref var b = ref heap<nint>(out var Ꮡb);
    b = 10;
    bumpInClosure(Ꮡb);
    fmt.Println(b);

    ref var c = ref heap<nint>(out var Ꮡc);
    c = 0;
    mixed(Ꮡc);
    fmt.Println(c);
}

} // end main_package

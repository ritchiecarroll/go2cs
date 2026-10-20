namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct elem {
    internal uint64 x;
}

internal static void consume(ref uint64 p, uint64 marker) {
    fmt.Println("consume entered");
    p += marker;
}

internal static void eagerFrom(ref elem e) {
    var m = side("side effect ran");
    consume(ref nonnil(ref e).x, m);
    fmt.Println("eagerFrom completed");
}

internal static uint64 side(@string msg) {
    fmt.Println(msg);
    return 1;
}

internal static void passThrough(ref uint64 p) {
    fmt.Println("entered callee");
    fmt.Println("value:", p);
}

internal static @string nilGuard(ж<elem> Ꮡp) {
    if (Ꮡp == nil) {
        return "nil ok";
    }
    return "non-nil";
}

internal static ж<elem> nilElem() {
    return default!;
}

internal static void tryEager(@string label, ref elem e) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label, "recovered:", recover());
        }, ref ᒐ);
        eagerFrom(ref e);
        fmt.Println(label, "no panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferredFault() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("deferred recovered:", recover());
        }, ref ᒐ);
        ж<uint64> q = default!;
        passThrough(ref (q).DerefOrNull());
        fmt.Println("not reached");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ж<elem> e1 = default!;
    tryEager("zerovar", ref (e1).DerefOrNull());

    tryEager("literal", ref ((ж<elem>)default!).DerefOrNull());

    any i = nilElem().OrTypedNil();
    var e3 = i._<ж<elem>>();
    tryEager("boxed", ref (e3).DerefOrNull());

    ref var good = ref heap<elem>(out var Ꮡgood);
    good = new elem(x: 40);
    tryEager("live", ref good);
    fmt.Println("good.x:", good.x);

    deferredFault();

    fmt.Println("guard:", nilGuard(nil));
    fmt.Println("guard:", nilGuard(Ꮡgood));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct elem {
    internal uint64 x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object consumeEnteredˢ = (@string)"consume entered"u8;

internal static void consume(ref uint64 p, uint64 marker) {
    fmt.Println(consumeEnteredˢ);
    p += marker;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object eagerFromCompletedˢ = (@string)"eagerFrom completed"u8;

internal static void eagerFrom(ref elem e) {
    var m = side("side effect ran");
    consume(ref nonnil(ref e).x, m);
    fmt.Println(eagerFromCompletedˢ);
}

internal static uint64 side(@string msg) {
    fmt.Println(msg);
    return 1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object enteredCalleeˢ = (@string)"entered callee"u8;
private static readonly object valueˢ = (@string)"value:"u8;

internal static void passThrough(ref uint64 p) {
    fmt.Println(enteredCalleeˢ);
    fmt.Println(valueˢ, p);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered:"u8;
private static readonly object noPanicˢ = (@string)"no panic"u8;

internal static void tryEager(@string label, ref elem e) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label, recoveredˢ, recover());
        }, ref ᒐ);
        eagerFrom(ref e);
        fmt.Println(label, noPanicˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredRecoveredˢ = (@string)"deferred recovered:"u8;
private static readonly object notReachedˢ = (@string)"not reached"u8;

internal static void deferredFault() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(deferredRecoveredˢ, recover());
        }, ref ᒐ);
        ж<uint64> q = default!;
        passThrough(ref (q).DerefOrNull());
        fmt.Println(notReachedˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goodXˢ = (@string)"good.x:"u8;
private static readonly object guardˢ = (@string)"guard:"u8;

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
    fmt.Println(goodXˢ, good.x);

    deferredFault();

    fmt.Println(guardˢ, nilGuard(nil));
    fmt.Println(guardˢ, nilGuard(Ꮡgood));
}

} // end main_package

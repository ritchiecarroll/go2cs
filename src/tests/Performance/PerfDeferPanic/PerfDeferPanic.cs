namespace go;

using errors = errors_package;
using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void add(this ref counter c, nint d) {
    c.n += d;
}

internal static error errBoom = errors.New("boom");

internal static nint /*r*/ viaLiteral(nint i) {
    nint r = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            r += (i & 7);
        }, ref ᒐ);
        r = i & 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static nint viaMethod(ж<counter> Ꮡc, nint i) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var add = (nint p1) => Ꮡc.add(p1);
        var addʗ1 = add;
        defer(addʗ1, i & 3, ref ᒐ);
        return c.n & 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static nint /*r*/ viaRecover(nint i) {
    nint r = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (AreEqual(e, errBoom)) {
                    r = i & 15;
                }
            }
        }, ref ᒐ);
        throw panic(errBoom);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static UntypedInt deferCalls => 5_000_000;
internal static UntypedInt recoverCalls => 200_000;

internal static void Main() {
    var start = time.Now().UnixNano();

    nint sum = 0;
    for (nint i = 0; i < deferCalls; i++) {
        sum += viaLiteral(i);
    }
    var afterLiteral = time.Now().UnixNano();

    var c = Ꮡ(new counter());
    for (nint i = 0; i < deferCalls; i++) {
        sum += viaMethod(c, i);
    }
    var afterMethod = time.Now().UnixNano();

    for (nint i = 0; i < recoverCalls; i++) {
        sum += viaRecover(i);
    }
    var end = time.Now().UnixNano();

    fmt.Println("checksum:", sum, c.n);
    fmt.Println("elapsed_ns.defer_literal:", afterLiteral - start);
    fmt.Println("elapsed_ns.defer_method:", afterMethod - afterLiteral);
    fmt.Println("elapsed_ns.panic_recover:", end - afterMethod);
    fmt.Println("elapsed_ns:", end - start);
}

} // end main_package

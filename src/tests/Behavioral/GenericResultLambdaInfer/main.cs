namespace go;

using fmt = fmt_package;

partial class main_package {

public static Func<T> Once<T>(Func<T> f) {
    ref var v = ref heap<T>(out var Ꮡv);
    bool done = default;
    return () => {
        if (!done) {
            Ꮡv.ValueSlot = f();
            done = true;
        }
        return Ꮡv.ValueSlot;
    };
}

public static Func<(A, B)> Once2<A, B>(Func<(A, B)> f) {
    ref var a = ref heap<A>(out var Ꮡa);
    ref var b = ref heap<B>(out var Ꮡb);
    bool done = default;
    return () => {
        if (!done) {
            (Ꮡa.ValueSlot, Ꮡb.ValueSlot) = f();
            done = true;
        }
        return (Ꮡa.ValueSlot, Ꮡb.ValueSlot);
    };
}

internal static E reduce<E>(slice<E> xs, Func<E, E, E> combine) {
    var acc = xs[0];
    foreach (var (_, x) in xs[1..]) {
        acc = combine(acc, x);
    }
    return acc;
}

internal static E pick<E>(slice<E> xs, Func<E, E, bool> better) {
    var best = xs[0];
    foreach (var (_, x) in xs[1..]) {
        if (better(x, best)) {
            best = x;
        }
    }
    return best;
}

internal static Func<nint> fortyTwo = Once(nint () => 42);

internal static @string describe(any v) {
    if (v == nil) {
        return "<nil>";
    }
    return fmt.Sprintf("%v"u8, v);
}

internal static void Main() {
    fmt.Println(fortyTwo(), fortyTwo());

    var nilOnce = Once(any () => default);
    fmt.Println(describe(nilOnce()));

    var panicOnce = Once(any () => {
        throw panic("boom");
    });
    var panicOnceʗ1 = panicOnce;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", describe(recover()));
            }, ref ᒐ);
            panicOnceʗ1();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();

    var pairOnce = Once2((any, any) () => (default, default));
    var (p, q) = pairOnce();
    fmt.Println(describe(p), describe(q));

    var both = Once2((nint, @string) () => (7, "seven"));
    var (n, s) = both();
    fmt.Println(n, s);

    fmt.Println(reduce(new nint[]{1, 2, 3, 4}.slice(), nint (nint a, nint b) => a + b));
    fmt.Println(reduce(new @string[]{"a", "b", "c"}.slice(), @string (@string a, @string b) => a + b));

    fmt.Println(pick(new nint[]{3, 9, 4}.slice(), (nint a, nint b) => a > b));
    fmt.Println(pick(new @string[]{"pear", "fig", "plum"}.slice(), (@string a, @string b) => len(a) < len(b)));
}

} // end main_package

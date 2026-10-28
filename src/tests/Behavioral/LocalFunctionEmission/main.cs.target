namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tally {
    internal nint n;
    internal @string log;
}

internal static void p1() {
    var ok = true;
    nint calls = 0;
    nint /*x*/ parseUint(@string s, nint min, nint max) {
        nint x = default;
        calls++;
        foreach (var (_, cΔ1) in slice<byte>(s)) {
            if (cΔ1 < (rune)'0' || (rune)'9' < cΔ1) {
                ok = false;
                return min;
            }
            x = x * 10 + (nint)cΔ1 - (rune)'0';
        }
        if (x < min || max < x) {
            ok = false;
            return min;
        }
        return x;
    }
    nint a = parseUint("42", 0, 99);
    nint b = parseUint("7x", 1, 99);
    nint c = parseUint("98", 0, 50);
    fmt.Println("P1:", a, b, c, ok, calls);
}

internal static void p2() {
    nint @base = 10;
    nint add(nint a, nint b) => a + b + @base;
    @base = 20;
    fmt.Println("P2:", add(1, 2), add(3, 4));
}

internal static void p3() {
    ref var t = ref heap<tally>(out var Ꮡt);
    t = new tally(1, "a");
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3}.array();
    void bump(nint k) {
        Ꮡt.Value.n += k;
        Ꮡt.Value.log += "!"u8;
        Ꮡarr.Value[0] += k;
    }
    bump(2);
    bump(3);
    t.n++;
    fmt.Println("P3:", t.n, t.log, arr);
}

internal static void p4() {
    nint c = 0;
    var next = () => {
        c++;
        return c;
    };
    next();
    var f = next;
    fmt.Println("P4:", f(), next(), c);
}

internal static void p5() {
    var step = (nint n) => n + 1;
    fmt.Println("P5a:", step(1));
    step = (nint n) => n + 2;
    fmt.Println("P5b:", step(1));
}

internal static void p6() {
    nint /*r*/ guard(nint n) {
        nint r = default;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var e = recover(); if (e != nil) {
                        r = -1;
                    }
                }
            }, ref ᒐ);
            if (n < 0) {
                throw panic("negative");
            }
            r = n * 2; goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return r;
    }
    fmt.Println("P6:", guard(3), guard(-1));
}

internal static void p7() {
    ref var fact = ref heap<Func<nint, nint>>(out var Ꮡfact);
    fact = (nint n) => {
        if (n <= 1) {
            return 1;
        }
        return n * Ꮡfact.ValueSlot(n - 1);
    };
    fmt.Println("P7:", fact(5));
}

internal static nint apply(Func<nint, nint> f, nint v) {
    return f(v);
}

internal static void p8() {
    nint m = 3;
    var scale = (nint v) => v * m;
    fmt.Println("P8:", apply(scale, 5), scale(2));
}

internal static void p9() {
    nint total = 0;
    nint outer(nint k) {
        nint inner(nint v) {
            total += v;
            return total;
        }
        return inner(k) + inner(k);
    }
    nint a = outer(2);
    nint b = outer(3);
    fmt.Println("P9:", a, b, total);
}

internal static void p10() {
    nint pure(nint a, nint b) => a * b;
    nint sum = 0;
    void acc(nint v) {
        sum += v;
    }
    for (nint i = 1; i <= 3; i++) {
        acc(pure(i, i));
    }
    fmt.Println("P10:", sum);
}

internal static void Main() {
    p1();
    p2();
    p3();
    p4();
    p5();
    p6();
    p7();
    p8();
    p9();
    p10();
}

} // end main_package

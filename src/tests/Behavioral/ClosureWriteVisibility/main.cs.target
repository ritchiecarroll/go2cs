namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct Tally {
    internal nint total;
    internal @string log;
}

public static void Add(this ref Tally t, nint n) {
    t.total += n;
    t.log += "+"u8;
}

internal static void probeA1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    void bump() {
        Ꮡt.Value.total += 100;
    }
    bump();
    t.total++;
    fmt.Println("A1:", t.total, t.log);
}

internal static void probeA2() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    nint get() => Ꮡt.Value.total;
    t.total += 10;
    fmt.Println("A2:", get());
}

internal static void probeA3() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    t.total += 2;
    var tʗ1 = t;
    nint get() => tʗ1.total;
    fmt.Println("A3:", get());
}

internal static void probeB1(Tally t) {
    void bump() {
        t.total += 100;
    }
    bump();
    t.total++;
    fmt.Println("B1:", t.total);
}

internal static void probeB2(Tally t) {
    nint get() => t.total;
    t.total += 10;
    fmt.Println("B2:", get());
}

internal static void probeC1() {
    var t = new Tally(5, "s");
    void bump() {
        t.total += 100;
    }
    bump();
    t.Add(3);
    bump();
    fmt.Println("C1:", t.total, t.log);
}

internal static void probeC2() {
    var t = new Tally(5, "s");
    nint get() => t.total;
    t.Add(3);
    fmt.Println("C2:", get());
}

internal static void probeD1(Tally t) {
    void bump() {
        t.total += 100;
    }
    bump();
    t.Add(3);
    bump();
    fmt.Println("D1:", t.total, t.log);
}

internal static void probeE1() {
    GoFrame ᒐ = default;
    try {
        ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
        defer(() => {
            fmt.Println("E1:", Ꮡt.Value.total);
        }, ref ᒐ);
        t.total = 42;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void probeE2() {
    GoFrame ᒐ = default;
    try {
        ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
        defer(ᴛ1 => fmt.Println("E2:", ᴛ1), t.total, ref ᒐ);
        t.total = 42;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void probeF1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    var done = new channel<nint>(0);
    var doneʗ1 = done;
    goǃ(() => {
        Ꮡt.Value.total += 100;
        doneʗ1.ᐸꟷ(1);
    }, GoStack.Bounded);
    ᐸꟷ(done);
    fmt.Println("F1:", t.total);
}

internal static void probeG1() {
    var fsᴷ = new stackAppend<Action>();

    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    slice<Action> fs = default;
    for (nint i = 0; i < 2; i++) {
        fs = append(fsᴷ, fs, () => {
            Ꮡt.Value.total += 10;
        });
    }
    fs[0]();
    fs[1]();
    fmt.Println("G1:", t.total);
}

internal static void probeG3() {
    var fsᴷ = new stackAppend<Func<nint>>();

    slice<Func<nint>> fs = default;
    foreach (var (_, x) in slice<nint>([10, 20, 30])) {
        fs = append(fsᴷ, fs, () => x);
    }
    fmt.Println("G3:", fs[0](), fs[1](), fs[2]());
}

internal static void probeH1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    void inc() {
        Ꮡt.Value.total++;
    }
    nint get() => Ꮡt.Value.total;
    inc();
    inc();
    fmt.Println("H1:", get());
}

internal static void probeI1() {
    ref var s = ref heap<slice<nint>>(slice<nint>([1]), out var Ꮡs);
    void app() {
        Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, (nint)2);
    }
    app();
    fmt.Println("I1:", len(s), s[0]);
}

internal static void probeJ1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    func(() => {
        Ꮡt.Value.total += 100;
    })();
    fmt.Println("J1:", t.total);
}

internal static void probeK1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    var p = Ꮡt;
    nint get() => Ꮡt.Value.total;
    p.total = 50;
    fmt.Println("K1:", get());
}

internal static void probeL1() {
    ref var m = ref heap<map<nint, nint>>(new map<nint, nint>{}, out var Ꮡm);
    void set() {
        Ꮡm.ValueSlot = new map<nint, nint>{[1] = 1};
    }
    set();
    fmt.Println("L1:", len(m));
}

internal static void probeM1() {
    ref var t = ref heap<Tally>(new Tally(5, "s"), out var Ꮡt);
    void bump() {
        Ꮡt.Value.total += 100;
    }
    nint get() => Ꮡt.Value.total;
    bump();
    fmt.Println("M1:", get(), t.total);
}

internal static void probeN1() {
    nint n = 0;
    void inc() {
        n++;
    }
    inc();
    fmt.Println("N1:", n);
}

internal static void probeN2() {
    ref var n = ref heap<nint>(0, out var Ꮡn);
    var p = Ꮡn;
    void inc() {
        Ꮡn.Value += 5;
    }
    inc();
    p.Value += 2;
    fmt.Println("N2:", n);
}

internal static (V, nint) probeP1<V>(slice<V> seq) {
    ref var v = ref heap<V>(out var Ꮡv);
    nint n = 0;
    var p = Ꮡv;
    void set(V x) {
        (Ꮡv.ValueSlot, n) = (x, n + 1);
    }
    foreach (var (_, x) in seq) {
        set(x);
    }
    return (p.ValueSlot, n);
}

internal static void probeQ1() {
    ref var walk = ref heap<Func<nint, nint>>(out var Ꮡwalk);
    nint calls = 0;
    walk = (nint n) => {
        calls++;
        if (n <= 0) {
            return 0;
        }
        return n + Ꮡwalk.ValueSlot(n - 1);
    };
    fmt.Println("Q1:", walk(10), calls);
}

internal static void probeQ2() {
    Func<nint, bool> even = default;
    ref var odd = ref heap<Func<nint, bool>>(out var Ꮡodd);
    even = (nint n) => {
        if (n == 0) {
            return true;
        }
        return Ꮡodd.ValueSlot(n - 1);
    };
    var evenʗ1 = even;
    odd = (nint n) => {
        if (n == 0) {
            return false;
        }
        return evenʗ1(n - 1);
    };
    fmt.Println("Q2:", even(8), odd(8));
}

internal static void Main() {
    probeA1();
    probeA2();
    probeA3();
    probeB1(new Tally(5, "s"));
    probeB2(new Tally(5, "s"));
    probeC1();
    probeC2();
    probeD1(new Tally(5, "s"));
    probeE1();
    probeE2();
    probeF1();
    probeG1();
    probeG3();
    probeH1();
    probeI1();
    probeJ1();
    probeK1();
    probeL1();
    probeM1();
    probeN1();
    probeN2();
    probeQ1();
    probeQ2();
    var (v, n) = probeP1(slice<nint>([10, 20, 30]));
    fmt.Println("P1:", v, n);
}

} // end main_package

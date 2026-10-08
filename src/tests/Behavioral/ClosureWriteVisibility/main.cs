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
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    void bump() {
        Ꮡt.Value.total += 100;
    }
    bump();
    t.total++;
    fmt.Println((@string)"A1:"u8, t.total, t.log);
}

internal static void probeA2() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    nint get() => Ꮡt.Value.total;
    t.total += 10;
    fmt.Println((@string)"A2:"u8, get());
}

internal static void probeA3() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    t.total += 2;
    var tʗ1 = t;
    nint get() => tʗ1.total;
    fmt.Println((@string)"A3:"u8, get());
}

internal static void probeB1(Tally t) {
    void bump() {
        t.total += 100;
    }
    bump();
    t.total++;
    fmt.Println((@string)"B1:"u8, t.total);
}

internal static void probeB2(Tally t) {
    nint get() => t.total;
    t.total += 10;
    fmt.Println((@string)"B2:"u8, get());
}

internal static void probeC1() {
    var t = new Tally(5, "s"u8);
    void bump() {
        t.total += 100;
    }
    bump();
    t.Add(3);
    bump();
    fmt.Println((@string)"C1:"u8, t.total, t.log);
}

internal static void probeC2() {
    var t = new Tally(5, "s"u8);
    nint get() => t.total;
    t.Add(3);
    fmt.Println((@string)"C2:"u8, get());
}

internal static void probeD1(Tally t) {
    void bump() {
        t.total += 100;
    }
    bump();
    t.Add(3);
    bump();
    fmt.Println((@string)"D1:"u8, t.total, t.log);
}

internal static void probeE1() {
    GoFrame ᒐ = default;
    try {
        ref var t = ref heap<Tally>(out var Ꮡt);
        t = new Tally(5, "s"u8);
        defer(() => {
            fmt.Println((@string)"E1:"u8, Ꮡt.Value.total);
        }, ref ᒐ);
        t.total = 42;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void probeE2() {
    GoFrame ᒐ = default;
    try {
        ref var t = ref heap<Tally>(out var Ꮡt);
        t = new Tally(5, "s"u8);
        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), (@string)"E2:", t.total, ref ᒐ);
        t.total = 42;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void probeF1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    var done = new channel<nint>(0);
    var doneʗ1 = done;
    goǃ(() => {
        Ꮡt.Value.total += 100;
        doneʗ1.ᐸꟷ(1);
    });
    ᐸꟷ(done);
    fmt.Println((@string)"F1:"u8, t.total);
}

internal static void probeG1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    slice<Action> fs = default!;
    for (nint i = 0; i < 2; i++) {
        fs = append(fs, () => {
            Ꮡt.Value.total += 10;
        });
    }
    fs[0]();
    fs[1]();
    fmt.Println((@string)"G1:"u8, t.total);
}

internal static void probeG3() {
    slice<Func<nint>> fs = default!;
    foreach (var (_, x) in new nint[]{10, 20, 30}.slice()) {
        fs = append(fs, () => x);
    }
    fmt.Println((@string)"G3:"u8, fs[0](), fs[1](), fs[2]());
}

internal static void probeH1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    void inc() {
        Ꮡt.Value.total++;
    }
    nint get() => Ꮡt.Value.total;
    inc();
    inc();
    fmt.Println((@string)"H1:"u8, get());
}

internal static void probeI1() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    s = new nint[]{1}.slice();
    void app() {
        Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, (nint)(2));
    }
    app();
    fmt.Println((@string)"I1:"u8, len(s), s[0]);
}

internal static void probeJ1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    ((Action)(() => {
        Ꮡt.Value.total += 100;
    }))();
    fmt.Println((@string)"J1:"u8, t.total);
}

internal static void probeK1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    var p = Ꮡt;
    nint get() => Ꮡt.Value.total;
    p.Value.total = 50;
    fmt.Println((@string)"K1:"u8, get());
}

internal static void probeL1() {
    ref var m = ref heap<map<nint, nint>>(out var Ꮡm);
    m = new map<nint, nint>{};
    void set() {
        Ꮡm.ValueSlot = new map<nint, nint>{[1] = 1};
    }
    set();
    fmt.Println((@string)"L1:"u8, len(m));
}

internal static void probeM1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s"u8);
    void bump() {
        Ꮡt.Value.total += 100;
    }
    nint get() => Ꮡt.Value.total;
    bump();
    fmt.Println((@string)"M1:"u8, get(), t.total);
}

internal static void probeN1() {
    nint n = 0;
    void inc() {
        n++;
    }
    inc();
    fmt.Println((@string)"N1:"u8, n);
}

internal static void probeN2() {
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 0;
    var p = Ꮡn;
    void inc() {
        Ꮡn.Value += 5;
    }
    inc();
    p.Value += 2;
    fmt.Println((@string)"N2:"u8, n);
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
    fmt.Println((@string)"Q1:"u8, walk(10), calls);
}

internal static void probeQ2() {
    Func<nint, bool> even = default!;
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
    fmt.Println((@string)"Q2:"u8, even(8), odd(8));
}

internal static void Main() {
    probeA1();
    probeA2();
    probeA3();
    probeB1(new Tally(5, "s"u8));
    probeB2(new Tally(5, "s"u8));
    probeC1();
    probeC2();
    probeD1(new Tally(5, "s"u8));
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
    var (v, n) = probeP1(new nint[]{10, 20, 30}.slice());
    fmt.Println((@string)"P1:"u8, v, n);
}

} // end main_package

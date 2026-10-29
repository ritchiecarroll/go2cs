namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct box {
    internal nint x;
    internal nint y;
}

internal static ж<box> gPtr;

internal static box gVal = new box(x: 7);

internal static void run(Action f) {
    f();
}

internal static void set(ref box p) {
    p.x = 42;
}

partial struct payload {
    internal slice<nint> vals;
}

internal static nint sum(this payload p) {
    nint t = 0;
    foreach (var (_, v) in p.vals) {
        t += v;
    }
    return t;
}

internal static nint nestedStructCapture() {
    ref var p = ref heap<payload>(out var Ꮡp);
    p = new payload(vals: new nint[]{1, 2, 3, 4}.slice());
    var @out = new channel<nint>(1);
    var outʗ1 = @out;
    var pʗ1 = p;
    [MethodImpl(MethodImplOptions.NoInlining)] void outer() {
        var outʗ2 = outʗ1;
        var pʗ2 = pʗ1;
        goǃ(() => {
            outʗ2.ᐸꟷ(pʗ2.sum());
        }, GoStack.Bounded);
    }
    outer();
    return ᐸꟷ(@out);
}

internal static partial nint selfRefCapture() {
    var done = new channel<nint>(1);
    var doneʗ1 = done;
    void worker(Action cb) {
        cb();
        doneʗ1.ᐸꟷ(5);
    }
    var workerʗ1 = worker;
    goǃ(workerʗ1, () => {
    });
    return ᐸꟷ(done);
}

internal static void deferArgCapture(ж<box> @out) {
    GoFrame ᒐ = default;
    try {
        var pf = @out;
        var pfʗ1 = pf;
        defer(run, () => {
            pfʗ1.x = 77;
        }, ref ᒐ);
        pf.x = 5;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint nestedArgLiteralCapture() {
    var total = new channel<nint>(1);
    var totalʗ1 = total;
    run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var items = new nint[]{1, 2, 3}.slice();
        var done = new channel<nint>(len(items));
        foreach (var i in range(items)) {
            nint iΔ1 = i;
            var doneʗ1 = done;
            var itemsʗ1 = items;
            goǃ(() => {
                doneʗ1.ᐸꟷ(itemsʗ1[iΔ1] * 10);
            }, GoStack.Bounded);
        }
        nint sum = 0;
        foreach (var _ᴛ1 in range(items)) {
            sum += ᐸꟷ(done);
        }
        totalʗ1.ᐸꟷ(sum);
    });
    return ᐸꟷ(total);
}

internal static void Main() {
    ref var m = ref heap(new box(), out var Ꮡm);
    run(() => {
        set(ref (Ꮡm).DerefOrNull());
    });
    fmt.Println("1:", m.x);

    ref var n = ref heap(new box(), out var Ꮡn);
    run(() => {
        set(ref (Ꮡn).DerefOrNull());
        Ꮡn.Value.y = Ꮡn.Value.x + 1;
    });
    fmt.Println("2:", n.x, n.y);

    ref var c = ref heap(new box(), out var Ꮡc);
    run(() => {
        var p = Ꮡc.of(box.Ꮡx);
        p.Value = 99;
    });
    fmt.Println("3:", c.x);

    ref var d = ref heap(new box(), out var Ꮡd);
    void f() {
        set(ref (Ꮡd).DerefOrNull());
    }
    f();
    fmt.Println("4:", d.x);

    ref var e = ref heap(new box(), out var Ꮡe);
    var pe = Ꮡe;
    var peʗ1 = pe;
    run(() => {
        peʗ1.x = 11;
        peʗ1.y = peʗ1.x + 1;
    });
    fmt.Println("5:", e.x, e.y);

    gPtr = Ꮡe;
    run(() => {
        gPtr.x = gVal.x;
    });
    fmt.Println("6:", e.x);

    var vals = new nint[]{5}.slice();
    var valsʗ1 = vals;
    nint adder(nint k) => k + valsʗ1[0];
    fmt.Println("7:", adder(10));

    var @base = new nint[]{3}.slice();
        var baseʗ1 = @base;
    var handlers = new Func<nint, nint>[]{
        (nint k) => k + baseʗ1[0]
    }.slice();
    fmt.Println("8:", handlers[0](100));

    var seed = new nint[]{2}.slice();

    var seedʗ1 = seed;
    Func<nint, nint> mul = (nint k) => k * seedʗ1[0];
    fmt.Println("9:", mul(21));

    (nint x, bool ok) pick(nint sel) {
        nint x = default;
        bool ok = default;
        if (sel > 0) {
            x = sel * 7;
            ok = true;
            return (x, ok);
        }
        return (x, ok);
    }
    var (a, b) = pick(3);
    var (zx, zok) = pick(-1);
    fmt.Println("10:", a, b, zx, zok);

    (nint n, @string s) zero() {
        nint nΔ1 = default;
        @string s = default;
        return (nΔ1, s);
    }
    var (n0, s0) = zero();
    fmt.Println("11:", n0, s0 == ""u8);

    fmt.Println("12:", nestedStructCapture());

    fmt.Println("13:", selfRefCapture());

    ref var g = ref heap(new box(), out var Ꮡg);
    deferArgCapture(Ꮡg);
    fmt.Println("14:", g.x);

    fmt.Println("15:", nestedArgLiteralCapture());
}

} // end main_package

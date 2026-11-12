namespace go;

using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;

partial class main_package {

partial struct req {
    internal nint n;
    internal channel<nint> reply;
}

internal static channel<req> reqs = new channel<req>(0);
internal static channel<req> reqs2 = new channel<req>(0);

internal static void pump() {
    for (;;) {
        var r = ᐸꟷ(reqs);
        var ack = new channel<nint>(0);
        reqs2.ᐸꟷ(new req(r.n, ack));
        nint v = ᐸꟷ(ack);
        r.reply.ᐸꟷ(v + 1);
    }
}

internal static void pump2() {
    for (;;) {
        var r = ᐸꟷ(reqs2);
        r.reply.ᐸꟷ(r.n);
    }
}

internal static nint park(nint n) {
    var reply = new channel<nint>(0);
    reqs.ᐸꟷ(new req(n, reply));
    nint v = ᐸꟷ(reply);
    return v;
}

partial struct point {
    internal nint x, y;
}

partial struct big {
    internal int64 a, b, c, d;
    internal @string s;
    internal float64 f;
    internal point p;
    internal array<int32> arr = new(4);
}

internal static error errOdd = errors.New("odd");

internal static nint retInt(nint n) {
    nint v = park(n);
    return v * 2;
}

internal static float64 retFloat(nint n) {
    nint v = park(n);
    return (float64)v / 4D;
}

internal static bool retBool(nint n) {
    nint v = park(n);
    return v % 2 == 0;
}

internal static point retPoint(nint n) {
    nint v = park(n);
    return new point(v, -v);
}

internal static big retBig(nint n) {
    nint v = park(n);
    var w = (int64)v;
    return new big(a: w, b: w * 2, c: w * 3, d: w * 4, s: fmt.Sprint("s", v), f: (float64)v + 0.5D, p: new point(v, v), arr: new int32[]{(int32)v, 2, 3, 4}.array());
}

internal static array<nint> retArr(nint n) {
    nint v = park(n);
    return new nint[]{v, v + 1, v + 2}.array();
}

internal static (nint, error) retPair(nint n) {
    nint v = park(n);

    if (v % 2 == 0) {
        return (v, default);
    }

    return (0, errOdd);
}

internal static (point, bool) retPointOk(nint n) {
    nint v = park(n);
    return (new point(v, v * v), v > 3);
}

internal static (nint, @string, float64) retTriple(nint n) {
    nint v = park(n);
    return (v, fmt.Sprint("t", v), (float64)v * 1.5D);
}

internal static T hold<T>(T v, nint n) {
    park(n);
    return v;
}

internal static (K, V) both<K, V>(K k, V v, nint n) {
    park(n);
    return (k, v);
}

partial interface number<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

internal static T total<T>(slice<T> xs, nint n)
    where T : /* number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T t = GoZero<T>();

    foreach (var (_, x) in range(xs)) {
        t += x;
    }

    park(n);
    return t;
}

internal static array<T> twice<T>(T v, nint n) {
    var a = hold(v, n);
    var b = hold(v, n + 1);
    return new T[]{a, b}.array();
}

partial struct box<T> {
    internal T v;
    internal nint n;
}

internal static T val<T>(this box<T> b, nint n) {
    park(n);
    return b.v;
}

internal static T swap<T>(this ref box<T> b, T v, nint n) {
    var old = b.v;
    nint w = park(n);
    b.n += w;
    b.v = v;
    return old;
}

partial interface holder<T> {
    T val(nint n);
}

internal static partial @string closureArm(big b) {
    b = b.ΔClone();

    ref var arr = ref heap<array<nint>>(new nint[]{7, 8, 9}.array(), out var Ꮡarr);
    ref var g = ref heap<point>(hold(new point(5, 6), 1), out var Ꮡg);
    var @out = new channel<big>(0);
    var pts = new channel<point>(1);

    var arrʗ1 = arr;
    var bʗ1 = b;
    var gʗ1 = g;
    var outʗ1 = @out;
    var ptsʗ1 = pts;
    goǃ(() => {
        nint w = park(arrʗ1[1]);
        ref var c = ref heap<big>(bʗ1.ΔClone(), out var Ꮡc);
        c.a += (int64)w;
        c.p = gʗ1;
        c.arr[0] = (int32)arrʗ1[2];
        ptsʗ1.ᐸꟷ(new point(w, len(c.s)));
        outʗ1.ᐸꟷ(c.ΔClone());
    });

    var r = ᐸꟷ(@out);
    var p = ᐸꟷ(pts);
    return fmt.Sprint(r, p);
}

internal static nint stormOne(nint i) {
    nint a = hold(i, i);
    var p = hold(new point(i, i), i);
    @string s = hold(fmt.Sprint(i), i);
    var (k, v) = both(i, new big(a: (int64)i), i);
    var t = total(slice<float64>([1.5D, (float64)i]), i);
    return a + p.x + len(s) + k + (nint)v.a + (nint)t;
}

internal static partial (nint, nint) storm(nint count) {
    var done = new channel<nint>(0);

    for (nint iᴛ1 = 0; iᴛ1 < count; iᴛ1++) {
        var i = iᴛ1;
        var doneʗ1 = done;
        goǃ(() => {
            doneʗ1.ᐸꟷ(stormOne(i));
        });
    }

    nint sum = 0;

    for (nint i = 0; i < count; i++) {
        sum += ᐸꟷ(done);
    }

    return (sum, count);
}

internal static partial @string nameLeaf(nint n) {
    nint v = park(n);
    var pcs = new slice<uintptr>(16);
    nint c = runtime.Callers(1, pcs);
    var frames = runtime.CallersFrames(pcs.slice(0, c));
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);

    for (;;) {
        var (f, more) = frames.Next();

        if (strings.HasPrefix(f.Function, "main.")) {
            Ꮡb.WriteString(" "u8 + f.Function);
        }

        if (!more) {
            break;
        }
    }

    return fmt.Sprint(v) + b.String();
}

internal static partial @string nameMid(nint n) {
    @string s = nameLeaf(n);
    return s + " |mid"u8;
}

internal static partial @string nameTop(nint n) {
    @string s = nameMid(n + 1);
    return s + " |top"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string strˢ = "str"u8;

internal static partial void main() {
    goǃ(pump);
    goǃ(pump);
    goǃ(pump2);
    goǃ(pump2);

    nint i = retInt(1);
    var f = retFloat(2);
    var ok = retBool(3);
    var p = retPoint(4);
    ref var bg = ref heap<big>(retBig(5), out var Ꮡbg);
    var arr = retArr(6);
    fmt.Println("value", i, f, ok, p, bg, arr);

    var (n1, e1) = retPair(1);
    var (n2, e2) = retPair(2);
    var (p3, ok3) = retPointOk(3);
    var (t1, t2, t3) = retTriple(4);
    fmt.Println("tuple", n1, e1, n2, e2, p3, ok3, t1, t2, t3);

    nint g1 = hold<nint>(41, 1);
    @string g2 = hold(strˢ, 2);
    var g3 = hold(new point(1, 2), 3);
    var pp = Ꮡ(new point(3, 4));
    var g4 = hold(pp, 4);
    var g5 = hold(bg, 5);
    var g6 = hold(new nint[]{5, 6}.array(), 6);
    var g7 = hold<error>(errOdd, 7);
    fmt.Println("generic", g1, g2, g3, g4.Value, g4 == pp, g5, g6, g7);

    var (k1, v1) = both((@string)"k", 1.5D, 1);
    var (k2, v2) = both(new point(1, 1), bg, 2);
    var (k3, v3) = both<nint, array<@string>>(7, new @string[]{"a", "b"}.array(), 3);
    fmt.Println("both", k1, v1, k2, v2, k3, v3);

    nint s1 = total(slice<nint>([1, 2, 3]), 1);
    var s2 = total(slice<float64>([1.5D, 2.5D]), 2);
    var w1 = twice(new point(9, 9), 1);
    var w2 = twice((@string)"z", 3);
    fmt.Println("total", s1, s2, w1, w2);

    var bx = new box<point>(new point(1, 2), 0);
    var bv = bx.val(1);
    var old = bx.swap(new point(7, 7), 2);
    var bb = Ꮡ(new box<big>(v: bg.ΔClone()));
    var bbv = bb.val(3);
    var bold = bb.swap(new big(s: "new"), 4);
    fmt.Println("box", bv, old, bx.v, bx.n, bbv, bold.s, bb.v.s, bb.n);

    holder<big> hl = bb.Value;
    var hv = hl.val(6);
    holder<@string> hs = new box<@string>(v: "hs");
    @string hsv = hs.val(7);
    fmt.Println("iface", hv.s, hsv);

    @string cl = closureArm(bg);
    fmt.Println("closure", cl);

    var (sum, count) = storm(64);
    fmt.Println("storm", sum, count);

    @string nm = nameTop(1);
    fmt.Println("names", nm);
    var @out = new channel<@string>(0);

    var outʗ1 = @out;
    goǃ(() => {
        @string s = nameTop(2);
        outʗ1.ᐸꟷ(s);
    });

    @string ng = ᐸꟷ(@out);
    fmt.Println("names go", ng);
}

} // end main_package

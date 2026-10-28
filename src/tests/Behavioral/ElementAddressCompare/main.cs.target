namespace go;

using fmt = fmt_package;
using testing = testing_package;

partial class main_package {

partial struct words /*[]uint64*/;

partial struct quad /*[4]nint*/;

partial struct holder {
    internal array<nint> f = new(4);
    internal quad q;
}

internal static (bool, bool) sameField(this ref holder h) {
    return (Ꮡelem(h.f, 1) == Ꮡelem(h.f, 1), Ꮡelem(h.f, 0) == Ꮡelem(h.f, 1));
}

internal static (bool, bool) sameNamedField(this ref holder h) {
    return (Ꮡelem(h.q.Value, 2) == Ꮡelem(h.q.Value, 2), Ꮡelem(h.q.Value, 2) != Ꮡelem(h.q.Value, 3));
}

internal static (bool, bool) sameParam(quad xʗp) {
    ref var x = ref heap(xʗp.Clone(), out var Ꮡx);

    return (Ꮡx.at<nint>(1) == Ꮡx.at<nint>(1), Ꮡx.at<nint>(0) == Ꮡx.at<nint>(1));
}

internal static bool alias(words x, words y) {
    return cap(x) > 0 && cap(y) > 0 && Ꮡelem<uint64>(x.slice(0, cap(x)), cap(x) - 1) == Ꮡelem<uint64>(y.slice(0, cap(y)), cap(y) - 1);
}

internal static bool overlaps(slice<uint64> z, slice<uint64> x) {
    return Ꮡelem(z, 0) != Ꮡelem(x, 0);
}

internal static bool besideElem(slice<nint> s) {
    ref var elem = ref heap<nint>(out var Ꮡelem);
    elem = 1;
    var p = Ꮡelem;
    p.Value = 2;
    return Ꮡ(s, 0) == Ꮡ(s, 0) && Ꮡ(s, 0) != Ꮡ(s, 1) && elem == 2;
}

partial struct empty {
}

internal static bool sink;

internal static void recovered(@string label, Func<bool> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println(label, "panic:", r);
                }
            }
        }, ref ᒐ);
        fmt.Println(label, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var s = new slice<nint>(4, 8);

    fmt.Println("self       :", Ꮡelem(s, 0) == Ꮡelem(s, 0), Ꮡelem(s, 2) == Ꮡelem(s, 2));
    fmt.Println("distinct   :", Ꮡelem(s, 0) == Ꮡelem(s, 1), Ꮡelem(s, 0) != Ꮡelem(s, 1));

    var t = s[1..];
    var u = t[1..];
    var r = s.slice(1, 2, 3);
    var h = s[..2];
    fmt.Println("reslice    :", Ꮡelem(t, 0) == Ꮡelem(s, 1), Ꮡelem(t, 0) == Ꮡelem(s, 0), Ꮡelem(u, 0) == Ꮡelem(s, 2), Ꮡelem(r, 0) == Ꮡelem(s, 1), Ꮡelem(h, 1) == Ꮡelem(s, 1));

    var w = append(s.slice(0, 0, 1), (nint)99);
    var g = append(s.slice(0, 4, 4), (nint)5);
    fmt.Println("append     :", Ꮡelem(w, 0) == Ꮡelem(s, 0), Ꮡelem(g, 0) == Ꮡelem(s, 0));

    ref var a = ref heap(new array<nint>(4), out var Ꮡa);
    var v = a[..];
    fmt.Println("array      :", Ꮡa.Ꮡelem<nint>(1) == Ꮡa.Ꮡelem<nint>(1), Ꮡelem(v, 2) == Ꮡa.Ꮡelem<nint>(2), Ꮡelem(v, 0) == Ꮡa.Ꮡelem<nint>(1));

    var p = Ꮡ(array<nint>.Alias(s[1..4], 3));
    var q = Ꮡ(new nint[]{}.array(3));
    fmt.Println("ptr array  :", p.Ꮡelem<nint>(0) == Ꮡelem(s, 1), p.Ꮡelem<nint>(2) == Ꮡelem(s, 3), q.Ꮡelem<nint>(0) == q.Ꮡelem<nint>(0), q.Ꮡelem<nint>(0) == q.Ꮡelem<nint>(1));

    var n = new words(3, 5);
    var m = n[1..];
    var @base = (slice<uint64>)n;
    fmt.Println("named slice:", Ꮡelem<uint64>(m, 0) == Ꮡelem<uint64>(n, 1), Ꮡelem<uint64>(n, 0) == Ꮡelem(@base, 0), alias(n, m), alias(n, new words(1)));
    fmt.Println("overlaps   :", overlaps(@base, @base), overlaps(@base[1..], @base));
    fmt.Println("elem name  :", besideElem(s));

    holder hv = new();
    fmt.Println("fields     :");
    var (ᴛ1, ᴛ2) = hv.sameField();
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = hv.sameNamedField();
    fmt.Println(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = sameParam(new quad(new nint[4].array()));
    fmt.Println(ᴛ5, ᴛ6);

    var ms = GoReflect.WithElemDims(new slice<array<nint>>(2, () => new(4)), 4);
    fmt.Println("nested     :", Ꮡ(ms, 1).at<nint>(2) == Ꮡ(ms, 1).at<nint>(2), Ꮡ(ms, 0).at<nint>(2) == Ꮡ(ms, 1).at<nint>(2));

    var es = new slice<empty>(3);
    var es2 = new slice<empty>(2);
    fmt.Println("zero size  :", Ꮡelem(es, 0) == Ꮡelem(es, 1), Ꮡelem(es, 0) == Ꮡelem(es2, 0));

    slice<nint> nilSlice = default;
    var nilSliceʗ1 = nilSlice;
    var sʗ1 = s;
    recovered("nil slice  :", () => Ꮡelem(nilSliceʗ1, 0) == Ꮡelem(sʗ1, 0));
    var sʗ2 = s;
    recovered("past len   :", () => Ꮡelem(sʗ2, 0) == Ꮡelem(sʗ2, 4));
    nint idx = 9;
    var sʗ3 = s;
    var tʗ1 = t;
    recovered("left first :", () => Ꮡelem(sʗ3, idx) == Ꮡelem(tʗ1, idx - 1));

    var sʗ4 = s;
    var tʗ2 = t;
    fmt.Println("allocs     :", testing.AllocsPerRun(100, () => {
        sink = Ꮡelem(sʗ4, 0) == Ꮡelem(tʗ2, 0);
    }));
    var mʗ1 = m;
    var nʗ1 = n;
    fmt.Println("allocs named:", testing.AllocsPerRun(100, () => {
        sink = Ꮡelem<uint64>(mʗ1, 0) == Ꮡelem<uint64>(nʗ1, 1);
    }));
    var pʗ1 = p;
    var sʗ5 = s;
    fmt.Println("allocs ptr :", testing.AllocsPerRun(100, () => {
        sink = pʗ1.Ꮡelem<nint>(1) == Ꮡelem(sʗ5, 2);
    }));

    var (x, y) = (s, t);
    fmt.Println("parens     :", (Ꮡelem(x, 1)) == (Ꮡelem(y, 0)), ((Ꮡelem(x, 0))) != Ꮡelem(y, 0));

    fmt.Println("boxed      :", AreEqual(((any)(Ꮡ(x, 1))), ((any)(Ꮡ(y, 0)))), Ꮡ(x, 0) == Ꮡ(y, 0));
    switch (Ꮡ(x, 1)) {
    case var ᴛ when ᴛ == Ꮡ(y, 0): {
        fmt.Println("switch     : y[0]");
        break;
    }
    default: {
        fmt.Println("switch     : default");
        break;
    }}

}

} // end main_package

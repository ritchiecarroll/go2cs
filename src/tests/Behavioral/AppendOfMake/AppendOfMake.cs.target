namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<nint> extend(slice<nint> s, nint n) {
    return appendꓸꓸꓸ(s, makeꓸꓸꓸ<nint>(n));
}

public static slice<nint> Grow(slice<nint> s, nint n) {
    {
        n -= cap(s) - len(s); if (n > 0) {
            s = appendꓸꓸꓸ(s.slice(0, cap(s)), makeꓸꓸꓸ<nint>(n)).slice(0, len(s));
        }
    }
    return s;
}

internal static S growGeneric<S, E>(S s, nint n)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return appendꓸꓸꓸ<S, E>(s, makeꓸꓸꓸ<E>(n));
}

partial struct point {
    public nint X, Y;
}

internal static slice<nint> withCap(slice<nint> s, nint n) {
    return appendꓸꓸꓸ(s, new slice<nint>(n, n + 1));
}

internal static slice<nint> named(slice<nint> s, nint n) {
    var m = new slice<nint>(n);
    return appendꓸꓸꓸ(s, m);
}

internal static slice<array<nint>> arrays(slice<array<nint>> s, nint n) {
    return appendꓸꓸꓸ(s, GoReflect.WithElemDims(new slice<array<nint>>(n, () => new(2)), 2));
}

internal static void Main() {
    var s = new slice<nint>(2, 4);
    (s[0], s[1]) = (1, 2);
    var stale = s[..4];
    (stale[2], stale[3]) = (99, 99);

    var e = extend(s, 2);
    fmt.Println("in place:", len(e), cap(e), e, Ꮡelem(e, 0) == Ꮡelem(s, 0));

    var f = extend(s, 3);
    fmt.Println("grows:", len(f), cap(f), f, Ꮡelem(f, 0) == Ꮡelem(s, 0));

    slice<nint> none = default!;
    var n = extend(none, 3);
    fmt.Println("nil:", len(n), cap(n), n);
    fmt.Println("zero:", len(extend(s, 0)), extend(default!, 0) == default!);

    var g = Grow(new nint[]{1, 2}.slice(), 5);
    fmt.Println("Grow:", len(g), cap(g) >= 7, g);

    var b = growGeneric<slice<byte>, byte>(slice<byte>("ab"u8), 2);
    fmt.Println("generic:", len(b), b);
    var p = growGeneric<slice<point>, point>(new point[]{new(1, 2)}.slice(), 1);
    fmt.Println("generic struct:", p);

    fmt.Println("refused:", withCap(new nint[]{1}.slice(), 2), named(new nint[]{1}.slice(), 1), arrays(default!, 2));

    var sʗ1 = s;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("negative:", recover() != default!);
            }, ref ᒐ);
            nint k = -1;
            _ = extend(sʗ1, k);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

} // end main_package

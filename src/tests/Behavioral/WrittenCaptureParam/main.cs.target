namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct label {
    internal @string name;
}

internal static @string String(this label l) {
    return l.name;
}

internal static (fmt.Stringer, nint) swap(fmt.Stringer s) {
    return (new label(name: "swapped:"u8 + s.String()), 1);
}

internal static (@string, @string) declRedeclare(fmt.Stringer sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    @string get() => Ꮡs.ValueSlot.String();
    @string before = get();
    (s, var n) = swap(s);
    _ = n;
    @string after = get();
    return (before, after);
}

internal static @string /*result*/ declDeferObserver(fmt.Stringer xʗp, fmt.Stringer y) {
    @string result = default!;
    GoFrame ᒐ = default;
    try {
        ref var x = ref heap(xʗp, out var Ꮡx);

        defer(() => {
            result = "final:"u8 + Ꮡx.ValueSlot.String() + "/"u8 + y.String();
        }, ref ᒐ);
        (x, y) = (y, x);
        (x, var n) = swap(x);
        _ = n;
        result = ""u8;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return result;
}

internal static @string declClosureWrite(fmt.Stringer sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    void set() {
        Ꮡs.ValueSlot = new label(name: "closure-set"u8);
    }
    (s, var n) = swap(s);
    set();
    return fmt.Sprintf("%s n=%d"u8, s.String(), n);
}

internal static (@string, @string) litRedeclare(fmt.Stringer seed) {
    (@string, @string) f(fmt.Stringer sʗp) {
        ref var s = ref heap(sʗp, out var Ꮡs);
        @string get() => Ꮡs.ValueSlot.String();
        @string before = get();
        (s, var n) = swap(s);
        _ = n;
        @string after = get();
        return (before, after);
    }
    return f(seed);
}

internal static (nint, nint) declSliceRedeclare(slice<nint> vʗp) {
    ref var v = ref heap(vʗp, out var Ꮡv);

    nint sum() {
        nint t = 0;
        foreach (var (_, e) in Ꮡv.ValueSlot) {
            t += e;
        }
        return t;
    }
    nint before = sum();
    (v, var extra) = grow(v);
    nint after = sum() + extra;
    return (before, after);
}

internal static (slice<nint>, nint) grow(slice<nint> v) {
    return (append(v.slice(0, len(v), len(v)), (nint)(99)), 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object declRedeclareˢ = (@string)"declRedeclare:"u8;
private static readonly object declDeferObserverˢ = (@string)"declDeferObserver:"u8;
private static readonly object declClosureWriteˢ = (@string)"declClosureWrite:"u8;
private static readonly object litRedeclareˢ = (@string)"litRedeclare:"u8;
private static readonly object declSliceRedeclareˢ = (@string)"declSliceRedeclare:"u8;

internal static void Main() {
    var a = new label(name: "alpha"u8);
    var b = new label(name: "beta"u8);
    var (before, after) = declRedeclare(a);
    fmt.Println(declRedeclareˢ, before, after);
    fmt.Println(declDeferObserverˢ, declDeferObserver(a, b));
    fmt.Println(declClosureWriteˢ, declClosureWrite(b));
    (before, after) = litRedeclare(a);
    fmt.Println(litRedeclareˢ, before, after);
    var (x, y) = declSliceRedeclare(new nint[]{1, 2, 3}.slice());
    fmt.Println(declSliceRedeclareˢ, x, y);
}

} // end main_package

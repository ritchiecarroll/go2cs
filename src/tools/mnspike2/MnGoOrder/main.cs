namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint x;
}

internal static channel<nint> ch = new channel<nint>(1);

public static nint Set(ж<nint> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    ch.ᐸꟷ(1);
    ᐸꟷ(ch);
    return 3;
}

internal static bool hit;

internal static nint effect() {
    hit = true;
    ch.ᐸꟷ(1);
    ᐸꟷ(ch);
    return 7;
}

internal static void param(nint pʗp) {
    ref var p = ref heap(pʗp, out var Ꮡp);

    fmt.Printf("parameter %d %d\n"u8, p, Set(Ꮡp, 2));
}

public static nint Grow(ж<slice<nint>> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s = append(s, (nint)9);
    ch.ᐸꟷ(1);
    ᐸꟷ(ch);
    return 1;
}

internal static void main() {
    ref var a = ref heap<nint>(1, out var Ꮡa);
    fmt.Printf("local %d %d\n"u8, a, Set(Ꮡa, 2));

    param(1);

    ref var s = ref heap<T>(new T(x: 1), out var Ꮡs);
    fmt.Printf("field-of-value %d %d\n"u8, s.x, Set(Ꮡs.Ꮡx, 2));

    var ps = Ꮡ(new T(x: 1));
    fmt.Printf("field-through-pointer %d %d\n"u8, ps.x, Set(ps.Ꮡx, 2));

    nilPointer();

    ref var u = ref heap<nint>(1, out var Ꮡu);
    fmt.Printf("unary %d %d\n"u8, -u, Set(Ꮡu, 2));

    ref var b = ref heap<nint>(1, out var Ꮡb);
    fmt.Printf("binary-left %d\n"u8, b + Set(Ꮡb, 2));

    ref var c = ref heap<nint>(1, out var Ꮡc);
    fmt.Printf("cast %v %d\n"u8, (float64)c, Set(Ꮡc, 2));

    sliceToArray();
}

internal static void nilPointer() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            fmt.Printf("nil-pointer panicked=%v effect-before-panic=%v\n"u8, r != nil, hit);
        }, ref ᒐ);
        ж<T> pn = default;
        fmt.Printf("nil-pointer %d %d\n"u8, pn.x, effect());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void sliceToArray() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            fmt.Printf("slice-to-array panicked=%v\n"u8, r != nil);
        }, ref ᒐ);
        ref var sl = ref heap<slice<nint>>(slice<nint>([1]), out var Ꮡsl);
        fmt.Printf("slice-to-array %v %d\n"u8, new array<nint>(sl, 2), Grow(Ꮡsl));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

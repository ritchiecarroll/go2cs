namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (ж<int64>, int64) next(int64 now) {
    ref var v = ref heap<int64>(out var Ꮡv);
    v = now * 2;
    return (Ꮡv, now + 1);
}

internal static (ж<int64>, int64) step(int64 now) {
    (var pp, now) = next(now);
    if (pp == nil) {
        return (default!, now);
    }
    return (pp, now);
}

internal static (ж<nint>, ж<nint>) pair() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 7;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 9;
    return (Ꮡa, Ꮡb);
}

internal static (ж<nint>, ж<nint>) shadow() {
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 100;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 200;
    var (px, py) = (Ꮡx, Ꮡy);
    if (px.Value > 0) {
        var (pxΔ1, pyΔ1) = pair();
        return (pxΔ1, pyΔ1);
    }
    return (px, py);
}

partial struct cursor {
    internal nint pos;
    internal @string tag;
}

internal static (cursor, error) makeCursor(nint pos, @string tag) {
    return (new cursor(pos: pos, tag: tag), default!);
}

internal static void bump(ref cursor c) {
    c.pos++;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string oneˢ = "one"u8;
private static readonly @string twoˢ = "two"u8;

internal static (nint, nint) streams() {
    var (rbr1, err) = makeCursor(10, oneˢ);
    if (err != default!) {
        return (0, 0);
    }
    (var rbr2, err) = makeCursor(20, twoˢ);
    if (err != default!) {
        return (0, 0);
    }
    bump(ref rbr1);
    bump(ref rbr2);
    bump(ref rbr2);
    return (rbr1.pos, rbr2.pos);
}

internal static (nint, bool) pairAt(nint i) {
    return (i * 11, i % 2 == 0);
}

internal static (slice<nint> hw, nint count) fill(nint n) {
    slice<nint> hw = default!;
    nint count = default!;

    hw = new slice<nint>(n);
    bool ok = default!;
    for (nint i = 0; i < n; i++) {
        {
            (hw[i], ok) = pairAt(i); if (!ok) {
                continue;
            }
        }
        count++;
    }
    return (hw, count);
}

internal static void Main() {
    var (a, b) = step(5);
    fmt.Println(a.Value, b);
    var (c, d) = shadow();
    fmt.Println(c.Value, d.Value);
    var (e, f) = streams();
    fmt.Println(e, f);
    var (vals, evens) = fill(4);
    fmt.Println(vals[1], vals[3], evens);
}

} // end main_package

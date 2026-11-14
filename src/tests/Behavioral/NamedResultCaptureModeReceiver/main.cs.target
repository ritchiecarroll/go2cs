namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Int128 {
    public int64 Lo, Hi;
}

public static (Int128 r, error err) SetBig(this ж<Int128> Ꮡx, int64 v) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (v > 1000) {
        var ᴛ1 = fmt.Errorf("%T.SetBig: overflow"u8, Ꮡx.OrTypedNil());
        return (x, ᴛ1);
    }
    (x.Lo, x.Hi) = (v, v >> 63);
    return (x, default);
}

internal static Int128 /*r*/ fromInt(int64 v) {
    ref var r = ref heap(new Int128(), out var Ꮡr);

    Ꮡr.SetBig(v);
    return r;
}

internal static (Int128 r, error err) fromIntBare(int64 v) {
    ref var r = ref heap(new Int128(), out var Ꮡr);
    error err = default;

    r.Hi = 7;
    (_, err) = Ꮡr.SetBig(v);
    return (r, err);
}

partial struct Tally {
    internal nint total;
    internal @string log;
}

public static void Add(this ж<Tally> Ꮡt, nint n) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            Ꮡt.Value.log = fmt.Sprintf("%s+%d"u8, Ꮡt.Value.log, n);
        }, ref ᒐ);
        t.total += n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct Holder {
    internal @string name;
    internal Tally inner;
}

internal static (Tally t, Holder h) tally(nint n) {
    ref var t = ref heap(new Tally(), out var Ꮡt);
    ref var h = ref heap(new Holder(), out var Ꮡh);

    t.log = "start";
    Ꮡt.Add(n);
    Ꮡt.Add(n * 10);
    h.name = "h";
    Ꮡh.Ꮡinner.Add(n * 100);
    return (t, h);
}

partial struct Stack /*[]nint*/;

internal static void push(this ж<Stack> Ꮡs, nint v) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (v < 0) {
        fmt.Printf("%T rejects %d\n"u8, Ꮡs.OrTypedNil(), v);
        return;
    }
    s = append(s, v);
}

internal static Stack /*s*/ build(nint n) {
    ref var s = ref heap<Stack>(out var Ꮡs);

    for (nint i = 1; i <= n; i++) {
        Ꮡs.push(i * i);
    }
    Ꮡs.push(-1);
    return s;
}

internal static void main() {
    var (_, err) = @new<Int128>().SetBig(5000);
    fmt.Println(fromInt(-5), fromInt(9), err);

    (var r, err) = fromIntBare(12);
    fmt.Println(r, err);
    (r, err) = fromIntBare(2000);
    fmt.Println(r, err);

    var (t, h) = tally(3);
    fmt.Println(t.total, t.log, h.name, h.inner.total, h.inner.log);

    var s = build(4);
    fmt.Println(len(s), s);
}

} // end main_package

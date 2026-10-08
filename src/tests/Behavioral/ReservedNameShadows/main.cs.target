namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Δany {
    internal nint x;
}

partial struct Δrune {
    internal nint r;
}

partial struct Δnint {
    internal nint d;
}

partial struct Δbuiltin {
    internal nint z;
}

partial struct Δsstring {
    internal nint n;
}

partial struct @required {
    internal nint c;
}

partial struct @scoped {
    internal nint b;
}

partial struct record {
    internal nint a;
}

partial struct ΔGoFrame {
    internal nint k;
}

internal static nint deferInShadowedPackage() {
    GoFrame ᒐ = default;
    try {
        ref var f = ref heap<ΔGoFrame>(out var Ꮡf);
        f = new ΔGoFrame(k: 1);
        defer(() => {
            Ꮡf.Value.k++;
        }, ref ᒐ);
        return f.k;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct box {
    internal slice<nint> items;
}

internal static nint len(this ref box b) {
    return builtin.len(b.items) + 1;
}

internal static nint freeLen() {
    var s = new nint[]{1, 2, 3}.slice();
    return builtin.len(s);
}

internal static nint nilShadow() {
    nint nil = 5;
    return nil + 1;
}

internal static nint keywordLocals() {
    nint @__arglist = 5;
    nint record = 1;
    nint @scoped = 2;
    nint @required = 3;
    nint nuint = 4;
    return @__arglist + record + @scoped + @required + nuint;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string boomˢ = "boom"u8;

internal static (nint, @string) predeclLocals() {
    nint iota = 6;
    @string error = boomˢ;
    nint comparable = 8;
    nint uintptr = 11;
    nint complex64 = 12;
    return (iota + comparable + uintptr + complex64, error);
}

internal static nint viewLen(slice<byte> b) {
    nint Δsstring = 3;
    sstring s = ((sstring)b);
    if (s == "ok"u8) {
        return Δsstring;
    }
    return builtin.len(s) + Δsstring;
}

internal static void Main() {
    var a = new Δany(x: 1);
    var vals = new any[]{(nint)(1), (@string)"two"u8, 3.5D}.slice();
    var c = (rune)'x';
    c++;
    var r = new Δrune(r: 2);
    var n = new Δnint(d: 4);
    var bi = new Δbuiltin(z: 9);
    var sv = new Δsstring(n: 7);
    var rq = new @required(c: 3);
    var sc = new @scoped(b: 2);
    var rc = new record(a: 1);
    var bx = new box(items: new nint[]{4, 5}.slice());
    fmt.Println(a.x, builtin.len(vals), vals[1], (nint)c, r.r, n.d, bi.z, sv.n, rq.c, sc.b, rc.a);
    var (pd, es) = predeclLocals();
    fmt.Println(freeLen(), bx.len(), nilShadow(), keywordLocals(), pd, es, viewLen(slice<byte>("xyz"u8)), deferInShadowedPackage());
}

} // end main_package

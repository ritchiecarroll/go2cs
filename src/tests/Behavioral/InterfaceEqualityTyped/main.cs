namespace go;

using errors = errors_package;
using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

partial struct errno /*num:uintptr*/;

internal static @string Error(this errno e) {
    return fmt.Sprintf("errno %d"u8, (uintptr)e);
}

internal static errno eagain => 11;
internal static errno eintr => 4;

partial struct celsius /*num:float64*/;

partial struct pathError {
    public @string Op;
    public @string Path;
    public error Err;
}

internal static @string Error(this pathError e) {
    return e.Op + " "u8 + e.Path + ": "u8 + e.Err.Error();
}

partial struct pair<K, V> {
    public K Key;
    public V Val;
}

partial struct node {
    internal nint n;
}

internal static bool eq(any a, any b) {
    return AreEqual(a, b);
}

internal static @string /*text*/ panicText(any a, any b) {
    @string text = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            text = fmt.Sprint(recover());
        }, ref ᒐ);
        _ = AreEqual(a, b);
        text = "no panic";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return text;
}

internal partial struct main_b /*dyn*/ {
    internal slice<nint> s;
}

internal partial struct main_a /*dyn*/ {
    internal any v;
}

internal static void Main() {
    error err = ((errno)eagain);
    fmt.Println("errno:", AreEqual(err, eagain), AreEqual(err, eintr), AreEqual(err, ((error)eagain)));

    var nan = (celsius)Δmath.NaN();
    fmt.Println("named float:", eq((celsius)1.5D, (celsius)1.5D), eq(nan, nan), eq((celsius)0D, (celsius)Δmath.Copysign(0D, -1D)));

    var boom = errors.New("boom");
    var a = new pathError("read", "p", boom);
    var b = new pathError("read", "p", boom);
    var c = new pathError("read", "p", errors.New("boom"));
    var d = new pathError("read", "p", eagain);
    var e = new pathError("read", "p", eagain);
    fmt.Println("struct with interface field:", eq(a, b), eq(a, c), eq(d, e), eq(a, d));
    error pe = a;
    fmt.Println("struct through error:", AreEqual(pe, ((error)b)), AreEqual(pe, ((error)c)));

    fmt.Println("generic struct:", eq(new pair<@string, nint>("k", 1), new pair<@string, nint>("k", 1)), eq(new pair<@string, nint>("k", 1), new pair<@string, nint>("k", 2)),
        eq(new pair<@string, float64>("k", Δmath.NaN()), new pair<@string, float64>("k", Δmath.NaN())));

    var (p, q) = (Ꮡ(new node(1)), Ꮡ(new node(1)));
    fmt.Println("pointer:", eq(p.OrTypedNil(), p.OrTypedNil()), eq(p.OrTypedNil(), q.OrTypedNil()));

    fmt.Println("string:", eq("go", "go"), eq("go", "c#"));
    fmt.Println("float and complex:", eq(Δmath.NaN(), Δmath.NaN()), eq(1.0D, 1.0D), eq(complex(Δmath.NaN(), 0D), complex(Δmath.NaN(), 0D)), eq((complex64)1F.i(), (complex64)1F.i()));

    fmt.Println("uncomparable:", panicText(new nint[]{1}.slice(), new nint[]{1}.slice()));
    fmt.Println("uncomparable field:", panicText(new pathError("x", "y", default!), new main_b()));
    fmt.Println("uncomparable in struct:", panicText(new main_a(new nint[]{1}.slice()), new main_a(new nint[]{1}.slice())));
}

} // end main_package

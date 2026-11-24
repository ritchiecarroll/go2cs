namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

partial class P /*ж<T>*/;

partial class IP /*ж<nint>*/;

partial struct holder {
    internal P p;
    internal IP ip;
}

internal static any boxP(P p) { return (p ?? (P)nil); }

internal static any /*r*/ retP() {
    any r = default;

    P p = default;
    r = (p ?? (P)nil);
    return r;
}

internal static any retPDirect() {
    P p = default;
    return (p ?? (P)nil);
}

internal static any boxE<E>(E e) { return (e).OrTypedNilOf(); }

internal static @string describe(any x) {
    switch (x.type()) {
    case null: {
        return "case nil";
    }
    case P v: {
        return fmt.Sprint("case P, nil=", v == nil);
    }
    case IP v: {
        return fmt.Sprint("case IP, nil=", v == nil);
    }
    case ж<T> v: {
        return fmt.Sprint("case *T, nil=", v == nil);
    }
    default: {
        var v = x;
        return "default";
    }}
}

internal static void report(@string label, any x) {
    var (_, okP) = x._<P>(ᐧ);
    var (_, okIP) = x._<IP>(ᐧ);
    var (_, okT) = x._<ж<T>>(ᐧ);
    fmt.Printf("%s: nil=%v %%T=%T %%v=%v P=%v IP=%v *T=%v %s\n"u8, label, x == nil, x, x, okP, okIP, okT, describe(x));
}

internal static void main() {
    P p = default;
    any x = (p ?? (P)nil);
    report("assign P", x);

    IP ip = default;
    any y = (ip ?? (IP)nil);
    report("assign IP", y);

    ж<T> up = default;
    any z = up.OrTypedNil();
    report("assign *T", z);

    report("conv P(nil)", (any)((P)nil));

    report("conv P(*T nil)", new P(up));
    report("conv P(P nil)", ((P)p) ?? (P)nil);

    report("arg P", (p ?? (P)nil));
    report("arg *T", up.OrTypedNil());

    report("ret P param", boxP(p));
    report("ret P named", retP());
    report("ret P direct", retPDirect());

    var m = new map<@string, any>{["p"] = (p ?? (P)nil), ["up"] = up.OrTypedNil()};
    report("map P", m["p"]);
    report("map *T", m["up"]);

    var mp = new map<@string, P>{};
    report("map miss P", (mp["absent"] ?? (P)nil));

    holder h = default;
    report("field P", (h.p ?? (P)nil));
    report("field IP", (h.ip ?? (IP)nil));

    var s = new slice<P>(1);
    report("elem P", (s[0] ?? (P)nil));

    var xs = slice<any>([(p ?? (P)nil), (ip ?? (IP)nil), up.OrTypedNil()]);
    foreach (var (i, e) in range(xs)) {
        report(fmt.Sprint("slice[", i, "]"), e);
    }

    report("generic P", boxE(p));
    report("generic IP", boxE(ip));
    report("generic *T", boxE(up));

    var (v, ok) = x._<P>(ᐧ); if (ok) {
        fmt.Println("comma-ok P:", ok, v == nil);
    } else {
        fmt.Println("comma-ok P: failed");
    }

    any x2 = ((P)nil);
    fmt.Println("x==x2", AreEqual(x, x2), "x==z", AreEqual(x, z), "x==y", AreEqual(x, y));

    var t = Ꮡ(new T(n: 7));
    P q = t;
    any w = (q ?? (P)nil);
    report("non-nil P", w);
    fmt.Println("non-nil P value:", w._<P>().n);
}

} // end main_package

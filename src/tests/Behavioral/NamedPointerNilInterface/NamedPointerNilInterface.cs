namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using ꓸꓸꓸany = Span<any>;

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

internal static P pkgP;
internal static any pkgAny = (pkgP ?? (P)nil);

internal static any boxVar(params ꓸꓸꓸany xsʗp) {
    var xs = xsʗp.sslice();

    return xs[0];
}

internal static (any, error) retPair() {
    P p = default;
    return ((p ?? (P)nil), default);
}

internal static any /*r*/ panicP(P p) {
    any r = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("panicP.func1")] () => {
            r = recover();
        }, ref ᒐ);
        throw panic((p ?? (P)nil));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void deferP(P p) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => report("defer P", ᴛ1), (p ?? (P)nil), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void reportDone(@string label, any x, channel<bool> done) {
    report(label, x);
    done.ᐸꟷ(true);
}

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

internal static partial void main() {
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

    report("conv P(unsafe nil)", (P)@unsafe.Pointer.To<T>((@unsafe.Pointer)default));
    report("conv *T(unsafe nil)", @unsafe.Pointer.To<T>((@unsafe.Pointer)default).OrTypedNil());

    var ch = new channel<any>(1);
    ch.ᐸꟷ((p ?? (P)nil));
    report("chan send P", ᐸꟷ(ch));
    var chᴛ1 = ch.ᐸꟷ((p ?? (P)nil), ꓸꓸꓸ);
    switch (select(chᴛ1)) {
    case 0: {
        break;
    }}
    report("select send P", ᐸꟷ(ch));

    var mk = new map<any, nint>{};
    mk[(p ?? (P)nil)] = 1;
    foreach (var (k, _) in mk) {
        report("map key store P", k);
    }
    var (v1, ok1) = mk[(p ?? (P)nil), ꟷ];
    var (_, okNil) = mk[default, ꟷ];
    fmt.Println("map key index P:", v1, ok1, "nil key:", okNil);
    var ml = new map<any, nint>{[(p ?? (P)nil)] = 2};
    foreach (var (k, _) in ml) {
        report("map key literal P", k);
    }

    report("variadic P", boxVar((p ?? (P)nil)));
    var @as = append(slice<any>([]), (any)(p ?? (P)nil));
    report("append P", @as[0]);

    var r = panicP(p);
    report("recover P", r);
    var (rp, okR) = r._<P>(ᐧ);
    fmt.Println("recover().(P):", okR, rp == nil);

    deferP(p);
    var done = new channel<bool>(0);
    goǃ((ᴛ1, ᴛ2) => reportDone("go P", ᴛ1, ᴛ2), (p ?? (P)nil), done);
    ᐸꟷ(done);

    var pʗ1 = p;
    [GoFunc("main.func1")] any f() => (pʗ1 ?? (P)nil);
    report("closure P", f());
    var (a, err) = retPair();
    report("ret pair P", a);
    fmt.Println("ret pair err nil:", err == nil);
    report("pkg var P", pkgAny);

    var (v, ok) = x._<P>(ᐧ); if (ok) {
        fmt.Println("comma-ok P:", ok, v == nil);
    } else {
        fmt.Println("comma-ok P: failed");
    }

    any x2 = ((P)nil);
    fmt.Println("x==x2", AreEqual(x, x2), "x==z", AreEqual(x, z), "x==y", AreEqual(x, y));

    fmt.Println("x==p", AreEqual(x, (p ?? (P)nil)), "z==up", AreEqual(z, up.OrTypedNil()));

    fmt.Println("P(nil)==P(up)", AreEqual(((any)((P)nil)), ((any)new P(up))), "x==P(up)", AreEqual(x, ((any)new P(up))));
    var keys = new map<any, nint>{};
    keys[((any)((P)nil))] = 1;
    keys[((any)new P(up))] = 2;
    keys[x] = 3;
    fmt.Println("nil P keys:", len(keys), keys[((any)((P)nil))]);

    var t = Ꮡ(new T(n: 7));
    P q = t;
    any w = (q ?? (P)nil);
    report("non-nil P", w);
    fmt.Println("non-nil P value:", w._<P>().n);
}

} // end main_package

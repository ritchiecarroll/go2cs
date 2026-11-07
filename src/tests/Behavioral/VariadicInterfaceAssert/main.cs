namespace go;

using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

partial struct TT {
    /*embed*/ public ж<testing_package.T> T;
}

partial struct L {
    internal @string tag;
}

public static void Error(this ref L l, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Println("L.Error", l.tag, len(args), fmt.Sprint(args.ꓸꓸꓸ));
}

partial struct LL {
    /*embed*/ public ж<L> L;
}

partial struct V {
    internal nint n;
}

public static void Error(this V v, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Println("V.Error", v.n, len(args), fmt.Sprint(args.ꓸꓸꓸ));
}

public static @string Join(this V v, @string sep, params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.slice();

    return fmt.Sprint(v.n) + ":"u8 + strings.Join(parts, sep);
}

partial struct G<T> {
    internal T v;
}

public static void Error<T>(this G<T> g, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Println("G.Error", (g.v).OrTypedNilOf(), len(args));
}

partial struct u {
    internal nint id;
}

internal static void logf(this u x, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Printf("u.logf %d "u8 + format + "\n"u8, appendꓸꓸꓸ(slice<any>([x.id]), args).ꓸꓸꓸ);
}

partial interface Errorer {
    void Error(params ꓸꓸꓸany ʗp);
}

internal static void check(@string label, bool ok) { fmt.Printf("%-28s: %v\n"u8, label, ok); }

internal partial interface main_type /*dyn*/ {
    void Error(params ꓸꓸꓸany ʗp);
}

internal partial interface main_typeᴛ1 /*dyn*/ {
    @string Name();
}

internal partial interface main_typeᴛ2 /*dyn*/ {
    void Logf(@string _Δp0, params ꓸꓸꓸany ʗp);
}

internal partial interface main_typeᴛ3 /*dyn*/ {
    bool Failed();
}

internal partial interface main_typeᴛ4 /*dyn*/ {
    @string Join(@string _Δp0, params ꓸꓸꓸstring ʗp);
}

internal partial interface main_typeᴛ5 /*dyn*/ {
    void logf(@string _Δp0, params ꓸꓸꓸany ʗp);
}

internal partial interface main_typeᴛ6 /*dyn*/ {
    void Error(params ꓸꓸꓸstring ʗp);
}

internal static void Main() {
    any tt = new TT();
    var (_, ok) = tt._<main_type>(ᐧ);
    check("TT    Error(...any)", ok);
    (_, ok) = tt._<main_typeᴛ1>(ᐧ);
    check("TT    Name() string", ok);
    (_, ok) = tt._<main_typeᴛ2>(ᐧ);
    check("TT    Logf(string, ...any)", ok);
    (_, ok) = tt._<main_typeᴛ3>(ᐧ);
    check("TT    Failed() bool", ok);
    (_, ok) = tt._<Errorer>(ᐧ);
    check("TT    Errorer", ok);

    any pt = Ꮡ(new TT());
    (_, ok) = pt._<main_type>(ᐧ);
    check("*TT   Error(...any)", ok);

    any t = Ꮡ(new testing.T());
    (_, ok) = t._<main_type>(ᐧ);
    check("*T    Error(...any)", ok);
    (_, ok) = t._<main_typeᴛ1>(ᐧ);
    check("*T    Name() string", ok);

    any ll = new LL(Ꮡ(new L("embedded")));
    {
        var (e, okΔ1) = ll._<main_type>(ᐧ); if (okΔ1) {
            check("LL    Error(...any)", okΔ1);
            e.Error("a", (nint)1, true);
        } else {
            check("LL    Error(...any)", okΔ1);
        }
    }

    any v = new V(7);
    {
        var (e, okΔ2) = v._<main_type>(ᐧ); if (okΔ2) {
            check("V     Error(...any)", okΔ2);
            e.Error();
            e.Error("x", "y");
        } else {
            check("V     Error(...any)", okΔ2);
        }
    }
    var (j, okΔ3) = v._<main_typeᴛ4>(ᐧ); if (okΔ3) {
        check("V     Join(string, ...string)", okΔ3);
        fmt.Println(j.Join("-", "a", "b", "c"));
    } else {
        check("V     Join(string, ...string)", okΔ3);
    }
    any pv = Ꮡ(new V(8));
    {
        var (e, okΔ4) = pv._<Errorer>(ᐧ); if (okΔ4) {
            check("*V    Errorer", okΔ4);
            e.Error("through", "pointer");
        } else {
            check("*V    Errorer", okΔ4);
        }
    }

    any g = new G<nint>(3);
    {
        var (e, okΔ5) = g._<main_type>(ᐧ); if (okΔ5) {
            check("G     Error(...any)", okΔ5);
            e.Error((nint)1, (nint)2);
        } else {
            check("G     Error(...any)", okΔ5);
        }
    }

    any x = new u(5);
    var (l, okΔ6) = x._<main_typeᴛ5>(ᐧ); if (okΔ6) {
        check("u     logf(string, ...any)", okΔ6);
        l.logf("%s=%d", "k", (nint)9);
    } else {
        check("u     logf(string, ...any)", okΔ6);
    }

    (_, ok) = v._<main_typeᴛ6>(ᐧ);
    check("V     Error(...string)", ok);
}

} // end main_package

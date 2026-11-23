namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct E {
    internal map<@string, nint> m;
}

public static @string Error(this E e) { throw panic("boom"); }

partial struct V {
    internal nint n;
}

public static @string Error(this V v) { return fmt.Sprint("V(", v.n, ")"); }

partial struct P {
    internal nint n;
}

public static @string Error(this ж<P> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return "nil *P";
    }
    return fmt.Sprint("P(", p.n, ")");
}

internal static void main() {
    any e = new E();
    fmt.Println(fmt.Sprintf("%v"u8, e));
    fmt.Println(fmt.Sprintf("%s"u8, e));

    var eʗ1 = e;
    func([GoFunc("main.func1")] () => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func1.1")] () => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            var (err, ok) = eʗ1._<error>(ᐧ); if (ok) {
                fmt.Println("unreached:", err.Error());
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    any v = new V(7);
    fmt.Println(fmt.Sprintf("%v"u8, v));
    {
        var (err, ok) = v._<error>(ᐧ); if (ok) {
            var (w, isV) = err._<V>(ᐧ);
            fmt.Println(isV, w.n, err.Error());
        }
    }

    ж<V> nv = default;
    any a = nv.OrTypedNil();
    fmt.Println(fmt.Sprintf("%v"u8, a));

    ж<P> np = default;
    any b = np.OrTypedNil();
    fmt.Println(fmt.Sprintf("%v"u8, b));

    any c = Ꮡ(new P(3));
    fmt.Println(fmt.Sprintf("%v"u8, c));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

internal static nint f(nint x) {
    if (x >= 2) {
        nint zΔ1 = default!;
        if (x > 5){
            zΔ1 = 100;
        } else {
            zΔ1 = 200;
        }
        return zΔ1;
    }
    nint z = x * x;
    return z;
}

partial struct tagErr {
    internal @string tag;
}

internal static @string Error(this ref tagErr e) {
    return "tag:"u8 + e.tag;
}

internal static (nint, error) check(@string s) {
    if (s == ""u8) {
        return (0, new tagErrжerror(Ꮡ(new tagErr(tag: "empty"u8))));
    }
    return (len(s), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ = "inner"u8;
private static readonly @string outerˢ = "outer"u8;

internal static @string g(@string s) {
    {
        nint n = len(s); if (n >= 0) {
            var (vΔ1, errΔ1) = check(""u8);
            if (errΔ1 != default!) {
                {
                    var (e, ok) = errΔ1._<ж<tagErr>>(ᐧ); if (ok) {
                        e.Value.tag = innerˢ;
                    }
                }
                return errΔ1.Error();
            }
            _ = vΔ1;
        }
    }
    var (v, err) = check(s);
    if (err != default!) {
        return outerˢ;
    }
    return fmt.Sprint(v);
}

internal static @string pkgShadow(@string s) {
    @string doubled = strings.Join(new @string[]{s, s}.slice(), "-"u8);
    nint stringsΔ1 = len(doubled);
    return fmt.Sprint(stringsΔ1);
}

internal static (nint, @string) pkgParamShadow(ж<strings.Reader> Ꮡstrings) {
    ref var stringsΔ1 = ref Ꮡstrings.DerefOrNull();

    var buf = new slice<byte>(2);
    var (n, _) = stringsΔ1.Read(buf);
    return (n, ((@string)buf));
}

partial struct @file {
    internal ж<strings.Reader> ΔΔfile;
    internal nint n;
}

internal static nint readOne(this ref @file f) {
    var buf = new slice<byte>(1);
    var (k, _) = f.ΔΔfile.Read(buf);
    f.n += k;
    return f.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string xyzˢ = "xyz"u8;

internal static void Main() {
    fmt.Println(f(10));
    fmt.Println(f(3));
    fmt.Println(f(1));
    fmt.Println(g("ab"u8));
    fmt.Println(pkgShadow("xy"u8));
    var rd = strings.NewReader("hi"u8);
    var (rn, rs) = pkgParamShadow(rd);
    fmt.Println(rn, rs);
    var fl = Ꮡ(new @file(ΔΔfile: strings.NewReader(xyzˢ)));
    fmt.Println(fl.readOne(), fl.readOne());
    fmt.Println(forwardOk());
}

internal static nint forwardOk() {
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2, ["c"u8] = 3};
    nint total = 0;
    var (v, ok) = m["a"u8, ꟷ];
    if (ok) {
        total += v;
    }
    for (nint n = 0; n < 2; n++) {
        {
            var (vΔ1, okΔ1) = m["b"u8, ꟷ]; if (okΔ1) {
                total += vΔ1;
                continue;
            }
        }
        var (vΔ2, okΔ2) = m["c"u8, ꟷ];
        if (!okΔ2) {
            break;
        }
        total += vΔ2;
    }
    return total;
}

} // end main_package

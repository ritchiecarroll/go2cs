namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal nint code;
}

internal static @string Error(this ref myErr e) {
    return fmt.Sprintf("myErr %d"u8, e.code);
}

partial struct valErr {
    internal @string tag;
}

internal static @string Error(this valErr e) {
    return "valErr "u8 + e.tag;
}

internal static error pointerSourced() {
    return new myErrжerror(Ꮡ(new myErr(code: 42)));
}

internal static error valueSourced() {
    return new valErr(tag: "v");
}

internal partial interface main_type /*dyn*/ {
    @string Error();
}

internal static void Main() {
    var err = pointerSourced();
    var p = err._<ж<myErr>>();
    p.Value.code = 7;
    fmt.Println("ptr-assert", (~p).code, err.Error());

    var v = valueSourced()._<valErr>();
    fmt.Println("val-assert", v.tag, v.Error());

    {
        var (_, ok) = valueSourced()._<ж<myErr>>(ᐧ); if (ok) {
            fmt.Println("ptr-miss-WRONG");
        } else {
            fmt.Println("ptr-miss-ok");
        }
    }
    {
        var (_, ok) = pointerSourced()._<valErr>(ᐧ); if (ok) {
            fmt.Println("val-miss-WRONG");
        } else {
            fmt.Println("val-miss-ok");
        }
    }

    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println("recovered");
                    }
                }
            }, ref ᒐ);
            _ = pointerSourced()._<valErr>();
            fmt.Println("unreachable");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();

    {
        var (s, ok) = pointerSourced()._<main_type>(ᐧ); if (ok) {
            fmt.Println("iface-assert", s.Error());
        } else {
            fmt.Println("iface-assert-missed");
        }
    }
}

} // end main_package

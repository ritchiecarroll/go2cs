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
    return new valErr(tag: "v"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ptrAssertˢ = (@string)"ptr-assert"u8;
private static readonly object valAssertˢ = (@string)"val-assert"u8;
private static readonly object ptrMissWrongˢ = (@string)"ptr-miss-WRONG"u8;
private static readonly object ptrMissOkˢ = (@string)"ptr-miss-ok"u8;
private static readonly object valMissWrongˢ = (@string)"val-miss-WRONG"u8;
private static readonly object valMissOkˢ = (@string)"val-miss-ok"u8;
private static readonly object recoveredˢ = (@string)"recovered"u8;
private static readonly object unreachableˢ = (@string)"unreachable"u8;
private static readonly object ifaceAssertˢ = (@string)"iface-assert"u8;
private static readonly object ifaceAssertMissedˢ = (@string)"iface-assert-missed"u8;

internal partial interface main_type /*dyn*/ {
    @string Error();
}

internal static void Main() {
    var err = pointerSourced();
    var p = err._<ж<myErr>>();
    p.Value.code = 7;
    fmt.Println(ptrAssertˢ, (~p).code, err.Error());
    var v = valueSourced()._<valErr>();
    fmt.Println(valAssertˢ, v.tag, v.Error());
    {
        var (_, ok) = valueSourced()._<ж<myErr>>(ᐧ); if (ok){
            fmt.Println(ptrMissWrongˢ);
        } else {
            fmt.Println(ptrMissOkˢ);
        }
    }
    {
        var (_, ok) = pointerSourced()._<valErr>(ᐧ); if (ok){
            fmt.Println(valMissWrongˢ);
        } else {
            fmt.Println(valMissOkˢ);
        }
    }
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Println(recoveredˢ);
                    }
                }
            }, ref ᒐ);
            _ = pointerSourced()._<valErr>();
            fmt.Println(unreachableˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    {
        var (s, ok) = pointerSourced()._<main_type>(ᐧ); if (ok){
            fmt.Println(ifaceAssertˢ, s.Error());
        } else {
            fmt.Println(ifaceAssertMissedˢ);
        }
    }
}

} // end main_package

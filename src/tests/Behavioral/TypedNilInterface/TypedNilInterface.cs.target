namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct AErr {
    internal nint n;
}

public static @string Error(this ref AErr e) {
    return "a"u8;
}

partial struct BErr {
    internal nint n;
}

public static @string Error(this ref BErr e) {
    return "b"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object xNilˢ = (@string)"x==nil"u8;
private static readonly object pTypednilˢ = (@string)"p==typednil"u8;
private static readonly object e1Nilˢ = (@string)"e1==nil"u8;
private static readonly object e2Nilˢ = (@string)"e2==nil"u8;
private static readonly object e1E2ˢ = (@string)"e1==e2"u8;
private static readonly object eaE1ˢ = (@string)"ea==e1"u8;
private static readonly object spNilˢ = (@string)"sp==nil"u8;
private static readonly object spSp2ˢ = (@string)"sp==sp2"u8;
private static readonly object spMpˢ = (@string)"sp==mp"u8;
private static readonly object stpNilˢ = (@string)"stp==nil"u8;
private static readonly object switchAErrˢ = (@string)"switch-AErr"u8;
private static readonly object switchOtherˢ = (@string)"switch-other"u8;
private static readonly @string missingˢ = "missing"u8;
private static readonly object printfVˢ = (@string)"printf-v"u8;
private static readonly object dpiNilˢ = (@string)"dpi==nil"u8;
private static readonly object dpiXˢ = (@string)"dpi==x"u8;
private static readonly object assignedNilˢ = (@string)"assigned==nil"u8;
private static readonly object returnedNilˢ = (@string)"returned==nil"u8;
private static readonly object elemNilˢ = (@string)"elem==nil"u8;
private static readonly object fieldNilˢ = (@string)"field==nil"u8;
private static readonly object mapvalNilˢ = (@string)"mapval==nil"u8;
private static readonly object mapkeyˢ = (@string)"mapkey"u8;
private static readonly object chanNilˢ = (@string)"chan==nil"u8;
private static readonly object assertOkˢ = (@string)"assert-ok"u8;
private static readonly object assertFailedˢ = (@string)"assert-failed"u8;
private static readonly object deleteˢ = (@string)"delete"u8;

internal partial struct main_type /*dyn*/ {
    internal nint r;
}

internal partial struct main_st /*dyn*/ {
    public ж<AErr> P;
}

internal partial struct main_holder /*dyn*/ {
    public any V;
}

internal partial struct main_rows /*dyn*/ {
    internal any value;
    internal bool want;
}

internal static void Main() {
    any x = ((ж<nint>)nil);
    fmt.Printf("%T\n"u8, x);
    fmt.Println(xNilˢ, x == default!);
    any y = ((ж<nint>)nil);
    fmt.Println((@string)"x==y"u8, AreEqual(x, y));
    ж<nint> p = default!;
    fmt.Println(pTypednilˢ, p == ((ж<nint>)nil));
    error e1 = new AErrжerror(((ж<AErr>)nil));
    error e2 = new BErrжerror(((ж<BErr>)nil));
    fmt.Println(e1Nilˢ, e1 == default!);
    fmt.Println(e2Nilˢ, e2 == default!);
    fmt.Println(e1E2ˢ, AreEqual(e1, e2));
    any ea = ((ж<AErr>)nil);
    fmt.Println(eaE1ˢ, AreEqual(ea, e1));
    any sp = ((ж<slice<byte>>)nil);
    any sp2 = ((ж<slice<byte>>)nil);
    any mp = ((ж<map<@string, nint>>)nil);
    any stp = ((ж<main_type>)nil);
    fmt.Println(spNilˢ, sp == default!);
    fmt.Println(spSp2ˢ, AreEqual(sp, sp2));
    fmt.Println(spMpˢ, AreEqual(sp, mp));
    fmt.Println(stpNilˢ, stp == default!);
    switch (e1.type()) {
    case ж<AErr> v: {
        fmt.Println(switchAErrˢ, v == nil);
        break;
    }
    default: {
        var v = e1;
        fmt.Println(switchOtherˢ);
        break;
    }}
    ж<nint> dp = default!;
    main_st st = default!;
    var sl = new slice<ж<BErr>>(1);
    var mm = new map<@string, ж<AErr>>{};
    fmt.Printf("%T %T %T %T\n"u8, dp.OrTypedNil(), st.P.OrTypedNil(), sl[0].OrTypedNil(), mm[missingˢ].OrTypedNil());
    fmt.Println(printfVˢ, fmt.Sprintf("%v|%v"u8, dp.OrTypedNil(), st.P.OrTypedNil()));
    any dpi = dp.OrTypedNil();
    fmt.Println(dpiNilˢ, dpi == default!, dpiXˢ, AreEqual(dpi, x));
    dpi = st.P.OrTypedNil();
    fmt.Println(assignedNilˢ, dpi == default!);
    fmt.Println(returnedNilˢ, boxed(sl[0]) == default!);
    var anys = new any[]{dp.OrTypedNil(), st.P.OrTypedNil()}.slice();
    var holder = new main_holder(V: dp.OrTypedNil());
    var byName = new map<@string, any>{["p"u8] = dp.OrTypedNil()};
    var byKey = new map<any, nint>{[dp.OrTypedNil()] = 7};
    fmt.Println(elemNilˢ, anys[0] == default!, fieldNilˢ, holder.V == default!);
    fmt.Println(mapvalNilˢ, byName["p"u8] == default!, mapkeyˢ, byKey[dp.OrTypedNil()], byKey[((ж<nint>)nil)]);
    var ch = new channel<any>(1);
    ch.ᐸꟷ(dp.OrTypedNil());
    fmt.Println(chanNilˢ, (ᐸꟷ(ch)) == default!);
    {
        var (q, ok) = dpi._<ж<AErr>>(ᐧ); if (ok){
            fmt.Println(assertOkˢ, q == nil);
        } else {
            fmt.Println(assertFailedˢ);
        }
    }
    fmt.Println(panicked());
    var acc = append(new any[]{}.slice(), (any)(dp.OrTypedNil()), (any)(st.P.OrTypedNil()));
    fmt.Printf("append %T %T %v %v\n"u8, acc[0], acc[1], acc[0] == default!, acc[1] == default!);
    var rows = new main_rows[]{
        new(dp.OrTypedNil(), true),
        new(st.P.OrTypedNil(), true),
        new((nint)(7), false)
    }.slice();
    foreach (var (_, r) in rows) {
        fmt.Printf("row %T %v %v\n"u8, r.value, r.value == default!, r.want);
    }
    var dm = new map<any, nint>{[dp.OrTypedNil()] = 1, [st.P.OrTypedNil()] = 2};
    delete(dm, dp.OrTypedNil());
    fmt.Println(deleteˢ, len(dm), dm[st.P.OrTypedNil()], dm[dp.OrTypedNil()]);
}

internal static any boxed(ж<BErr> Ꮡp) {
    return Ꮡp.OrTypedNil();
}

internal static @string /*msg*/ panicked() {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            {
                var (q, ok) = r._<ж<AErr>>(ᐧ); if (ok){
                    msg = fmt.Sprintf("panic-recovered *AErr nil=%v"u8, q == nil);
                } else {
                    msg = fmt.Sprintf("panic-recovered other %T %v"u8, r, r == default!);
                }
            }
        }, ref ᒐ);
        ж<AErr> p = default!;
        throw panic(p.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package

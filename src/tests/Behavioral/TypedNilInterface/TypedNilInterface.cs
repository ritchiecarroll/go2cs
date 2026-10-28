namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct AErr {
    internal nint n;
}

public static @string Error(this ref AErr e) {
    return "a";
}

partial struct BErr {
    internal nint n;
}

public static @string Error(this ref BErr e) {
    return "b";
}

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
    fmt.Println("x==nil", x == nil);

    any y = ((ж<nint>)nil);
    fmt.Println("x==y", AreEqual(x, y));

    ж<nint> p = default;
    fmt.Println("p==typednil", p == ((ж<nint>)nil));

    error e1 = new AErrжerror(((ж<AErr>)nil));
    error e2 = new BErrжerror(((ж<BErr>)nil));
    fmt.Println("e1==nil", e1 == nil);
    fmt.Println("e2==nil", e2 == nil);
    fmt.Println("e1==e2", AreEqual(e1, e2));

    any ea = ((ж<AErr>)nil);
    fmt.Println("ea==e1", AreEqual(ea, e1));

    any sp = ((ж<slice<byte>>)nil);
    any sp2 = ((ж<slice<byte>>)nil);
    any mp = ((ж<map<@string, nint>>)nil);
    any stp = ((ж<main_type>)nil);
    fmt.Println("sp==nil", sp == nil);
    fmt.Println("sp==sp2", AreEqual(sp, sp2));
    fmt.Println("sp==mp", AreEqual(sp, mp));
    fmt.Println("stp==nil", stp == nil);

    switch (e1.type()) {
    case ж<AErr> v: {
        fmt.Println("switch-AErr", v == nil);
        break;
    }
    default: {
        var v = e1;
        fmt.Println("switch-other");
        break;
    }}

    ж<nint> dp = default;
    main_st st = default;
    var sl = new slice<ж<BErr>>(1);
    var mm = new map<@string, ж<AErr>>{};

    fmt.Printf("%T %T %T %T\n"u8, dp.OrTypedNil(), st.P.OrTypedNil(), sl[0].OrTypedNil(), mm["missing"].OrTypedNil());
    fmt.Println("printf-v", fmt.Sprintf("%v|%v"u8, dp.OrTypedNil(), st.P.OrTypedNil()));

    any dpi = dp.OrTypedNil();
    fmt.Println("dpi==nil", dpi == nil, "dpi==x", AreEqual(dpi, x));
    dpi = st.P.OrTypedNil();
    fmt.Println("assigned==nil", dpi == nil);

    fmt.Println("returned==nil", boxed(sl[0]) == nil);

    var anys = new any[]{dp.OrTypedNil(), st.P.OrTypedNil()}.slice();
    var holder = new main_holder(V: dp.OrTypedNil());
    var byName = new map<@string, any>{["p"] = dp.OrTypedNil()};
    var byKey = new map<any, nint>{[dp.OrTypedNil()] = 7};
    fmt.Println("elem==nil", anys[0] == nil, "field==nil", holder.V == nil);
    fmt.Println("mapval==nil", byName["p"] == nil, "mapkey", byKey[dp.OrTypedNil()], byKey[((ж<nint>)nil)]);

    var ch = new channel<any>(1);
    ch.ᐸꟷ(dp.OrTypedNil());
    fmt.Println("chan==nil", (ᐸꟷ(ch)) == nil);

    {
        var (q, ok) = dpi._<ж<AErr>>(ᐧ); if (ok) {
            fmt.Println("assert-ok", q == nil);
        } else {
            fmt.Println("assert-failed");
        }
    }

    fmt.Println(panicked());

    var acc = append(new any[]{}.slice(), (any)(dp.OrTypedNil()), (any)(st.P.OrTypedNil()));
    fmt.Printf("append %T %T %v %v\n"u8, acc[0], acc[1], acc[0] == nil, acc[1] == nil);

    var rows = new main_rows[]{
        new(dp.OrTypedNil(), true),
        new(st.P.OrTypedNil(), true),
        new((nint)7, false)
    }.slice();
    foreach (var (_, r) in rows) {
        fmt.Printf("row %T %v %v\n"u8, r.value, r.value == nil, r.want);
    }

    var dm = new map<any, nint>{[dp.OrTypedNil()] = 1, [st.P.OrTypedNil()] = 2};
    delete(dm, dp.OrTypedNil());
    fmt.Println("delete", len(dm), dm[st.P.OrTypedNil()], dm[dp.OrTypedNil()]);
}

internal static any boxed(ж<BErr> p) {
    return p.OrTypedNil();
}

internal static @string /*msg*/ panicked() {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            {
                var (q, ok) = r._<ж<AErr>>(ᐧ); if (ok) {
                    msg = fmt.Sprintf("panic-recovered *AErr nil=%v"u8, q == nil);
                } else {
                    msg = fmt.Sprintf("panic-recovered other %T %v"u8, r, r == nil);
                }
            }
        }, ref ᒐ);

        ж<AErr> p = default;
        throw panic(p.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package

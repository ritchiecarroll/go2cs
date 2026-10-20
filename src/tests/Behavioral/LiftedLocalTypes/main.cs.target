namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static void describe(any f) {
    _ = f;
    fmt.Println("ok");
}

internal partial struct main_a /*dyn*/ {
    public nint X;
}

internal partial struct main_point /*dyn*/ {
    public nint X, Y;
}

internal static void Main() {
    var a = new main_a(X: 1);
    main_a b = default!;
    b = a;
    fmt.Println(a == b);
    ref var p = ref heap<main_point>(out var Ꮡp);
    p = new main_point(X: 1, Y: 2);
    fmt.Println(p.X + p.Y);
    fmt.Printf("%T %T\n"u8, p, Ꮡp);

    describe(nint (nint x) => 0);

    embeddedLocalTypes();
    foreignUnderlyingLocalTypes();
    foreignUnderlyingLocalTypesAgain();
    localInterfaceEmbed();
}

internal partial struct embeddedLocalTypes_myInt /*num:nint*/;

internal partial struct embeddedLocalTypes_MyInt /*num:nint*/;

internal partial struct embeddedLocalTypes_embed /*dyn*/ {
    public nint Q;
}

internal partial struct embeddedLocalTypes_holder /*dyn*/ {
    /*embed*/ internal embeddedLocalTypes_myInt myInt;
    /*embed*/ public embeddedLocalTypes_MyInt MyInt;
    /*embed*/ internal embeddedLocalTypes_embed embed;
}

internal partial struct embeddedLocalTypes_ptrHolder /*dyn*/ {
    /*embed*/ internal ж<embeddedLocalTypes_myInt> myInt;
    /*embed*/ internal ж<embeddedLocalTypes_embed> embed;
}

internal static void embeddedLocalTypes() {
    var h = new embeddedLocalTypes_holder(1, 2, new embeddedLocalTypes_embed(Q: 3));
    fmt.Println(h.myInt, h.MyInt, h.embed.Q);

    fmt.Println(h.Q);

    var k = new embeddedLocalTypes_holder(myInt: 4, MyInt: 5, embed: new embeddedLocalTypes_embed(Q: 6));
    fmt.Println(k.myInt, k.MyInt, k.Q);

    k.myInt = 7;
    fmt.Println(k.myInt);

    ref var i = ref heap<embeddedLocalTypes_myInt>(out var Ꮡi);
    i = (embeddedLocalTypes_myInt)8;
    ref var e = ref heap<embeddedLocalTypes_embed>(out var Ꮡe);
    e = new embeddedLocalTypes_embed(Q: 9);
    var pp = new embeddedLocalTypes_ptrHolder(myInt: Ꮡi, embed: Ꮡe);
    pp.myInt.Value = 10;
    pp.embed.Value.Q = 11;
    fmt.Println(pp.myInt.Value, (~pp.embed).Q, pp.Q);

    fmt.Printf("%T\n"u8, h.embed);
}

internal partial struct foreignUnderlyingLocalTypes_myTime /*global::go.time_package.Time*/;

internal partial struct foreignUnderlyingLocalTypes_myDur /*global::go.time_package.Duration*/;

internal partial struct foreignUnderlyingLocalTypes_inner /*global::go.time_package.Time*/;

internal static void foreignUnderlyingLocalTypes() {
    var m = new map<ж<foreignUnderlyingLocalTypes_myTime>, @string>();
    var t = @new<foreignUnderlyingLocalTypes_myTime>();
    m[t] = "seven";
    var (v, ok) = m[t, ꟷ];
    fmt.Println(v, ok, len(m));
    foreignUnderlyingLocalTypes_myDur d = default!;
    var pd = @new<foreignUnderlyingLocalTypes_myDur>();
    fmt.Println(d == pd.Value, pd != nil);

    {
        var pi = @new<foreignUnderlyingLocalTypes_inner>();
        fmt.Println(pi != nil);
    }
}

internal partial struct foreignUnderlyingLocalTypesAgain_myTime /*global::go.time_package.Time*/;

internal partial struct foreignUnderlyingLocalTypesAgain_myDur /*global::go.time_package.Duration*/;

internal static void foreignUnderlyingLocalTypesAgain() {
    var t = @new<foreignUnderlyingLocalTypesAgain_myTime>();
    foreignUnderlyingLocalTypesAgain_myDur d = default!;
    foreignUnderlyingLocalTypesAgain_myDur d2 = default!;
    fmt.Println(t != nil, d == d2);
}

internal partial interface localInterfaceEmbed_I /*dyn*/ {
    nint x();
}

internal partial interface localInterfaceEmbed_i /*dyn*/ :
    localInterfaceEmbed_I
{
    nint y();
}

internal static void localInterfaceEmbed() {
    localInterfaceEmbed_i v = new embedImpl(nil);
    fmt.Println(v.x(), v.y(), v.x() + v.y());
}

partial struct embedImpl {
}

internal static nint x(this embedImpl _) {
    return 3;
}

internal static nint y(this embedImpl _) {
    return 4;
}

} // end main_package

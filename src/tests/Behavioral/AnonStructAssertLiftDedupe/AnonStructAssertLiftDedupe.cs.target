namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object a1AssertIntoTheOriginalˢ = (@string)"A1 assert into the original var:"u8;
private static readonly object a2AssertIntoANewVarˢ = (@string)"A2 assert into a new var:"u8;
private static readonly object a3OriginalAssignsIntoˢ = (@string)"A3 original assigns into asserted var:"u8;
private static readonly object a4AssertedVarAssignsIntoˢ = (@string)"A4 asserted var assigns into original:"u8;
private static readonly object a5CommaOkˢ = (@string)"A5 comma-ok:"u8;
private static readonly object a6ThirdOccurrenceˢ = (@string)"A6 third occurrence:"u8;
private static readonly object a7EqualAcrossOccurrencesˢ = (@string)"A7 equal across occurrences:"u8;
private static readonly object b1PointerAssertionˢ = (@string)"B1 pointer assertion:"u8;
private static readonly object b2DifferentShapeˢ = (@string)"B2 different shape:"u8;
private static readonly object b3SameNamesDifferentˢ = (@string)"B3 same names different types:"u8;
private static readonly object b4MismatchedAssertionˢ = (@string)"B4 mismatched assertion:"u8;

internal partial struct main_p /*dyn*/ {
    public nint X, Y;
}

internal partial struct main_s /*dyn*/ {
    public @string A;
}

internal partial struct main_type /*dyn*/ {
    public @string X, Y;
}

internal static void Main() {
    ref var p = ref heap(new main_p(), out var Ꮡp);
    p.X = 1;
    p.Y = 2;
    any i = p;
    p = i._<main_p>();
    fmt.Println(a1AssertIntoTheOriginalˢ, p.X, p.Y);
    var q = i._<main_p>();
    fmt.Println(a2AssertIntoANewVarˢ, q.X, q.Y);
    q = p;
    fmt.Println(a3OriginalAssignsIntoˢ, q.X, q.Y);
    p = q;
    fmt.Println(a4AssertedVarAssignsIntoˢ, p.X, p.Y);
    var (r, ok) = i._<main_p>(ᐧ);
    fmt.Println(a5CommaOkˢ, r.X, r.Y, ok);
    any third = new main_p(X: 8, Y: 9);
    var u = third._<main_p>();
    p = u;
    fmt.Println(a6ThirdOccurrenceˢ, p.X, p.Y);
    fmt.Println(a7EqualAcrossOccurrencesˢ, q == u);
    var pp = Ꮡp;
    any j = pp.OrTypedNil();
    var pq = j._<ж<main_p>>();
    fmt.Println(b1PointerAssertionˢ, (~pq).X, (~pq).Y);
    main_s s = default!;
    s.A = "hi"u8;
    any k = s;
    var t = k._<main_s>();
    s = t;
    fmt.Println(b2DifferentShapeˢ, s.A);
    any w = new main_type(X: "a"u8, Y: "b"u8);
    var x = w._<main_type>();
    fmt.Println(b3SameNamesDifferentˢ, x.X, x.Y);
    var (_, bad) = i._<main_s>(ᐧ);
    fmt.Println(b4MismatchedAssertionˢ, bad);
}

} // end main_package

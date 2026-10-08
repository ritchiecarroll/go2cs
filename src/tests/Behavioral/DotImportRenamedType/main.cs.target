namespace go;

using fmt = fmt_package;
using static DotImportRenamedType.renamedlib_package;
using DotImportRenamedType;
using renamedlib = DotImportRenamedType.renamedlib_package;

partial class main_package {

partial struct ΔLocal {
    public @string Tag;
}

public static @string Local(this ΔLocal l) {
    return "L:"u8 + l.Tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object literalˢ = (@string)"literal:"u8;
private static readonly object ptrLiteralˢ = (@string)"ptr-literal:"u8;
private static readonly object detailLiteralˢ = (@string)"detail-literal:"u8;
private static readonly object detailOfˢ = (@string)"detail-of:"u8;
private static readonly @string deltaˢ = "delta"u8;
private static readonly object assertOkˢ = (@string)"assert-ok:"u8;
private static readonly @string epsilonˢ = "epsilon"u8;
private static readonly object assertOneˢ = (@string)"assert-one:"u8;
private static readonly @string zetaˢ = "zeta"u8;
private static readonly object assertMissˢ = (@string)"assert-miss:"u8;
private static readonly object plainˢ = (@string)"plain:"u8;
private static readonly @string thetaˢ = "theta"u8;
private static readonly object plainAssertˢ = (@string)"plain-assert:"u8;
private static readonly object localˢ = (@string)"local:"u8;
private static readonly object localAssertˢ = (@string)"local-assert:"u8;

internal static void Main() {
    var m = new renamedlibꓸMarker(Name: "alpha"u8, Size: 3);
    fmt.Println(literalˢ, m.Marker());
    var p = Ꮡ(new renamedlibꓸMarker(Name: "beta"u8, Size: 5));
    fmt.Println(ptrLiteralˢ, (~p).Marker());
    var d = new renamedlibꓸDetail(Label: "gamma"u8, Rank: 7);
    fmt.Println(detailLiteralˢ, d.Show());
    fmt.Println(detailOfˢ, m.Detail().Show());
    {
        var (got, ok) = Describe(deltaˢ, 9)._<renamedlibꓸMarker>(ᐧ); if (ok) {
            fmt.Println(assertOkˢ, got.Marker());
        }
    }
    var one = Wrap(epsilonˢ, 11)._<renamedlibꓸDetail>();
    fmt.Println(assertOneˢ, one.Show());
    var (_, notMarker) = Wrap(zetaˢ, 13)._<renamedlibꓸMarker>(ᐧ);
    fmt.Println(assertMissˢ, notMarker);
    var pl = new Plain(Note: "eta"u8);
    fmt.Println(plainˢ, pl.Note);
    {
        var (got, ok) = Boxed(thetaˢ)._<Plain>(ᐧ); if (ok) {
            fmt.Println(plainAssertˢ, got.Note);
        }
    }
    var l = new ΔLocal(Tag: "iota"u8);
    fmt.Println(localˢ, l.Local());
    {
        var (got, ok) = ((any)l)._<ΔLocal>(ᐧ); if (ok) {
            fmt.Println(localAssertˢ, got.Local());
        }
    }
}

} // end main_package

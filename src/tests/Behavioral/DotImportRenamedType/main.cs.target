namespace go;

using fmt = fmt_package;
using static DotImportRenamedType.renamedlib_package;
using DotImportRenamedType;
using renamedlib = DotImportRenamedType.renamedlib_package;
using static main_package;

partial class main_package {

partial struct Local {
    public @string Tag;
}

} partial class mainᴛLocal {

public static @string Local(this Local l) {
    return "L:"u8 + l.Tag;
}

} partial class main_package {

internal static void main() {
    var m = new Marker(Name: "alpha", Size: 3);
    fmt.Println("literal:", m.Marker());

    var p = Ꮡ(new Marker(Name: "beta", Size: 5));
    fmt.Println("ptr-literal:", p.Marker());

    var d = new Detail(Label: "gamma", Rank: 7);
    fmt.Println("detail-literal:", d.Show());

    fmt.Println("detail-of:", m.Detail().Show());

    {
        var (got, ok) = Describe("delta", 9)._<Marker>(ᐧ); if (ok) {
            fmt.Println("assert-ok:", got.Marker());
        }
    }

    var one = Wrap("epsilon", 11)._<Detail>();
    fmt.Println("assert-one:", one.Show());

    var (_, notMarker) = Wrap("zeta", 13)._<Marker>(ᐧ);
    fmt.Println("assert-miss:", notMarker);

    var pl = new Plain(Note: "eta");
    fmt.Println("plain:", pl.Note);

    {
        var (got, ok) = Boxed("theta")._<Plain>(ᐧ); if (ok) {
            fmt.Println("plain-assert:", got.Note);
        }
    }

    var l = new Local(Tag: "iota");
    fmt.Println("local:", l.Local());

    {
        var (got, ok) = ((any)l)._<Local>(ᐧ); if (ok) {
            fmt.Println("local-assert:", got.Local());
        }
    }
}

} // end main_package

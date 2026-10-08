namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct s {
    internal nint i;
}

partial class sPtr /*ж<s>*/;

partial struct pub {
    public nint I;
}

partial class pubPtr /*ж<pub>*/;

/*[3]*/ partial class arrPtr /*ж<array<nint>>*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object addrOfElemˢ = (@string)"Addr of Elem:"u8;
private static readonly object fieldWriteThroughˢ = (@string)"field write through:"u8;
private static readonly object indexWriteThroughˢ = (@string)"index write through:"u8;

internal static void Main() {
    var ps = Ꮡ(new s(1));
    var dps = new sPtr(ps);
    fmt.Println(fmt.Sprintf("%#v"u8, dps));
    fmt.Println(fmt.Sprintf("%v"u8, dps), fmt.Sprintf("%[1]T"u8, dps));
    fmt.Println(addrOfElemˢ, reflect.ValueOf(dps).Elem().Addr().Type());
    var pp = new pubPtr(Ꮡ(new pub(2)));
    var f = reflect.ValueOf(pp).Elem().Field(0);
    f.SetInt(5);
    fmt.Println(fieldWriteThroughˢ, (pp.Value).I, f.Addr().Type());
    var ap = new arrPtr(Ꮡ(new nint[]{1, 2, 3}.array()));
    reflect.ValueOf(ap).Elem().Index(1).SetInt(9);
    fmt.Println(indexWriteThroughˢ, (ap.Value)[1], ap.Value);
    fmt.Println(fmt.Sprintf("%#v"u8, ps.OrTypedNil()), reflect.ValueOf(ps.OrTypedNil()).Elem().Addr().Type());
}

} // end main_package

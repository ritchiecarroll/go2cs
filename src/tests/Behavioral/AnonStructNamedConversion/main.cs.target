namespace go;

using fmt = fmt_package;
using structs = AnonStructNamedConversion.structs_package;
using AnonStructNamedConversion;

partial class main_package {

[GoType] partial struct local {
    public nint A;
}

[GoType] partial struct tagged {
    [GoTag(@"json:""a""")]
    public nint A;
}

[GoType("dyn")] internal partial struct main_type {
    public nint A;
}

internal static void Main() {
    var b = ((structs.AssignB)new main_type(3));
    fmt.Println(b.A, b.Sum());
    main_type s = default!;
    s.A = 4;
    var l = ((local)s);
    fmt.Println(l.A);
    var c = ((structs.AssignB)s);
    fmt.Println(c.Sum());
    var t = ((tagged)s);
    fmt.Println(t.A);
}

} // end main_package

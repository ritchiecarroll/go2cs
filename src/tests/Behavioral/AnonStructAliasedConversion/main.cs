namespace go;

using fmt = fmt_package;
using st = AnonStructAliasedConversion.structs_package;
using AnonStructAliasedConversion;
using structs = AnonStructAliasedConversion.structs_package;

partial class main_package {

[GoType("dyn")] internal partial struct main_type {
    public nint A;
}

internal static void Main() {
    var b = ((st.AssignB)new main_type(3));
    fmt.Println(b.A, b.Sum());
    main_type s = default!;
    s.A = 4;
    fmt.Println(((st.AssignB)s).Sum());
}

} // end main_package

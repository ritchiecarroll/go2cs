namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct named {
    internal @string n;
}

internal static @string Name(this named v) {
    return "name:"u8 + v.n;
}

internal static @string Label(this named v) {
    return "label:"u8 + v.n;
}

[GoType("dyn")] public partial interface Report_r {
    @string Name();
    @string Label();
}

public static @string Report(Report_r r) {
    return r.Name() + ","u8 + r.Label();
}

[GoType("dyn")] internal partial interface report_r {
    @string Label();
}

internal static @string report(report_r r) {
    return r.Label();
}

internal static void Main() {
    fmt.Println(Report(new named("a"u8)));
    fmt.Println(report(new named("b"u8)));
}

} // end main_package

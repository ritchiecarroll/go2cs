namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct named {
    internal @string n;
}

} partial class mainᴛnamed {

internal static @string Name(this named v) { return "name:"u8 + v.n; }
internal static @string Label(this named v) { return "label:"u8 + v.n; }

} partial class main_package {

public partial interface Report_r /*dyn*/ {
    @string Name();
    @string Label();
}

public static @string Report(Report_r r) {
    return r.Name() + ","u8 + r.Label();
}

internal partial interface report_r /*dyn*/ {
    @string Label();
}

internal static @string report(report_r r) { return r.Label(); }

internal static void main() {
    fmt.Println(Report(new named("a")));
    fmt.Println(report(new named("b")));
}

} // end main_package

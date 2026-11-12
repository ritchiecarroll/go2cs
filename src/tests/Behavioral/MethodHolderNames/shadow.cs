namespace go;

using fmt = fmt_package;
using main = main_package;
using static main_package;

partial class main_package {

partial struct meter {
    internal nint main;
}

} partial class mainᴛmeter {

internal static nint Limit(this meter m) {
    nint main = m.main + 1;
    return main + (nint)main_package.Limit;
}

internal static nint Scale(this meter m, nint main) { return main_package.Scale(main) + m.main; }

internal static nint Count(this meter m) { return main.Count + m.main; }

} partial class main_package {

internal static void shadows() {
    var m = new meter(main: 2);
    fmt.Println("shadows", m.Limit(), m.Scale(3), m.Count());
}

} // end main_package

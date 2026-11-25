namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct meter {
    internal nint main;
}

internal static nint Limit⁔(/*this*/ meter m) {
    nint main = m.main + 1;
    return main + (nint)Limit;
}

internal static nint Scale⁔(/*this*/ meter m, nint main) { return Scale(main) + m.main; }

internal static nint Count⁔(/*this*/ meter m) { return Count + m.main; }

internal static Func<nint> limitPlus = [GoFunc("init.func1")] () => (nint)Limit + Scale(1);

internal static void shadows() {
    var m = new meter(main: 2);
    fmt.Println("shadows", m.Limit(), m.Scale(3), m.Count());
    fmt.Println("after a method", limitPlus());
}

} // end main_package

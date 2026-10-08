namespace go;

using fmt = fmt_package;
using CrossPkgLib = CrossPkgLib_package;

partial class main_package {

partial interface localLabel {
    @string Label();
}

internal static @string labelOf(localLabel l) {
    return l.Label();
}

internal static void Main() {
    CrossPkgLib.Labeled foreign = new CrossPkgLib.Sensor(Name: "adapter"u8, Temp: 21D);
    localLabel local = new CrossPkgLib_LabeledᴠlocalLabel(foreign);
    fmt.Println(labelOf(new CrossPkgLib_LabeledᴠlocalLabel(foreign)));
    fmt.Println(local.Label());
    fmt.Println(CrossPkgLib.Describe(foreign));
}

} // end main_package

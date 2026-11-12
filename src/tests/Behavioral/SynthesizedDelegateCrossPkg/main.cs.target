namespace go;

using fmt = fmt_package;
using CrossPkgFuncLib = CrossPkgFuncLib_package;
using CrossPkgLib = CrossPkgLib_package;

partial class main_package {

internal static void main() {
    fmt.Println(CrossPkgFuncLib.Count(new Func<CrossPkgLib.Status, bool>(CrossPkgFuncLib.Hot)));

    Func<CrossPkgLib.Status, bool> pick = CrossPkgFuncLib.Hot;
    fmt.Println(CrossPkgFuncLib.Count(pick));
}

} // end main_package

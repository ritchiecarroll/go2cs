namespace go;

using fmt = fmt_package;
using CrossPkgLib = CrossPkgLib_package;

partial class main_package {

partial struct opTable /*map[CrossPkgLib.Ticks, Func<nint, nint, nint>]*/;

internal static void Main() {
    var ops = new opTable(new map<CrossPkgLib.Ticks, Func<nint, nint, nint>>{
        [((CrossPkgLib.Ticks)2)] = (nint a, nint b) => a + b,
        [((CrossPkgLib.Ticks)3)] = (nint a, nint b) => a * b
    });
    fmt.Println(ops[((CrossPkgLib.Ticks)2)](4, 5), ops[((CrossPkgLib.Ticks)3)](4, 5), len(ops));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using lib = ExportedFuncOfUnexportedFuncType.lib_package;
using ExportedFuncOfUnexportedFuncType;
using ꓸꓸꓸcmpFunc = variadic<main_package.cmpFunc>;

partial class main_package {

public delegate nint cmpFunc(nint a, nint b);

public static nint Pick(nint a, nint b, params ꓸꓸꓸcmpFunc cmpsʗp) {
    var cmps = cmpsʗp.sslice();

    foreach (var (_, c) in range(cmps)) {
        nint r = c(a, b); if (r != 0) {
            return r;
        }
    }

    return 0;
}

internal static void main() {
    var byValue = [GoFunc("main.func1")] (nint a, nint b) => a < b;
    var byParity = [GoFunc("main.func2")] (nint a, nint b) => a % 2 == 0 && b % 2 != 0;

    var s = lib.OrderedBy(builtin.NilSafeDelegateConversion<lib.lessFunc, Func<nint, nint, bool>>(byParity), builtin.NilSafeDelegateConversion<lib.lessFunc, Func<nint, nint, bool>>(byValue));
    fmt.Println(s.Less(2, 3), s.Less(3, 2), s.Less(4, 6), s.Less(6, 4));
    fmt.Println(lib.Apply(builtin.NilSafeDelegateConversion<lib.lessFunc, Func<nint, nint, bool>>(byValue), 1, 2), lib.Apply(builtin.NilSafeDelegateConversion<lib.lessFunc, Func<nint, nint, bool>>(byValue), 2, 1));

    var h = new lib.Holder(Less: lib.Reverse(builtin.NilSafeDelegateConversion<lib.lessFunc, Func<nint, nint, bool>>(byValue)));
    fmt.Println(h.Less(1, 2), h.Less(2, 1));

    var same = [GoFunc("main.func3")] nint (nint a, nint b) => 0;
    var diff = [GoFunc("main.func4")] (nint a, nint b) => a - b;
    fmt.Println(Pick(5, 3, builtin.NilSafeDelegateConversion<cmpFunc, Func<nint, nint, nint>>(same), builtin.NilSafeDelegateConversion<cmpFunc, Func<nint, nint, nint>>(diff)), Pick(5, 5, builtin.NilSafeDelegateConversion<cmpFunc, Func<nint, nint, nint>>(same)));
}

} // end main_package

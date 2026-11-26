namespace go.ExportedFuncOfUnexportedFuncType;

using ꓸꓸꓸlessFunc = variadic<lib_package.lessFunc>;

partial class lib_package {

public delegate bool lessFunc(nint a, nint b);

public partial struct sorter {
    internal slice<lessFunc> less;
}

public static ж<sorter> OrderedBy(params ꓸꓸꓸlessFunc lessʗp) {
    var less = lessʗp.slice();

    return Ꮡ(new sorter(less: less));
}

internal static bool Less⁔(/*this*/ ref sorter s, nint a, nint b) {
    foreach (var (_, f) in range(s.less)) {
        if (f(a, b)) {
            return true;
        }

        if (f(b, a)) {
            return false;
        }
    }

    return false;
}

public static bool Apply(lessFunc f, nint a, nint b) {
    return f(a, b);
}

public static lessFunc Reverse(lessFunc f) {
    return [GoFunc("Reverse.func1")] (nint a, nint b) => f(b, a);
}

partial struct Holder {
    public lessFunc Less;
}

} // end lib_package

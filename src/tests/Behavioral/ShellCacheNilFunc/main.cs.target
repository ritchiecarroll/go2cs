namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate nint fn();

internal static nint Len(this fn f) {
    if (f == nil) {
        return -1;
    }

    return f();
}

internal partial interface main_type /*dyn*/ {
    nint Len();
}

internal static void Main() {
    fn none = default;
    var seven = new fn(() => 7);
    var eight = new fn(() => 8);
    var values = new any[]{(none).OrTypedNilFunc(), (seven).OrTypedNilFunc(), (eight).OrTypedNilFunc(), (none).OrTypedNilFunc()}.slice();

    for (nint round = 0; round < 3; round++) {
        foreach (var (i, a) in values) {
            {
                var (v, ok) = a._<main_type>(ᐧ); if (ok) {
                    fmt.Println("round", round, "value", i, "ok", v.Len());
                } else {
                    fmt.Println("round", round, "value", i, "MISS");
                }
            }
        }
    }
}

} // end main_package

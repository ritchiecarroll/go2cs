namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

internal delegate nint fn();

} partial class mainᴛfn {

internal static nint Len(this fn f) {
    if (f == nil) {
        return -1;
    }

    return f();
}

} partial class main_package {

internal partial interface main_type /*dyn*/ {
    nint Len();
}

internal static void main() {
    fn none = default;
    var seven = new fn(() => 7);
    var eight = new fn(() => 8);
    var values = slice<any>([(none).OrTypedNilFunc(), (seven).OrTypedNilFunc(), (eight).OrTypedNilFunc(), (none).OrTypedNilFunc()]);

    for (nint round = 0; round < 3; round++) {
        foreach (var (i, a) in range(values)) {
            var (v, ok) = a._<main_type>(ᐧ); if (ok) {
                fmt.Println("round", round, "value", i, "ok", v.Len());
            } else {
                fmt.Println("round", round, "value", i, "MISS");
            }
        }
    }
}

} // end main_package

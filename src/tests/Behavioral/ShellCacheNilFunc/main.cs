namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate nint fn();

internal static nint Len(this fn f) {
    if (f == default!) {
        return -1;
    }
    return f();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object roundˢ = (@string)"round"u8;
private static readonly object valueˢ = (@string)"value"u8;
private static readonly object missˢ = (@string)"MISS"u8;

internal partial interface main_type /*dyn*/ {
    nint Len();
}

internal static void Main() {
    fn none = default!;
    var seven = new fn(() => 7);
    var eight = new fn(() => 8);
    var values = new any[]{(none).OrTypedNilFunc(), (seven).OrTypedNilFunc(), (eight).OrTypedNilFunc(), (none).OrTypedNilFunc()}.slice();
    for (nint round = 0; round < 3; round++) {
        foreach (var (i, a) in values) {
            {
                var (v, ok) = a._<main_type>(ᐧ); if (ok){
                    fmt.Println(roundˢ, round, valueˢ, i, (@string)"ok"u8, v.Len());
                } else {
                    fmt.Println(roundˢ, round, valueˢ, i, missˢ);
                }
            }
        }
    }
}

} // end main_package

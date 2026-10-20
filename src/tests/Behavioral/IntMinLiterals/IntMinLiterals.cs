namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<int32> interesting32 = new int32[]{-2147483648, -2_147_483_648, -2147483647, -100663046, -32769, 2147483647}.slice();

internal static slice<int64> interesting64 = new int64[]{-9223372036854775808L, -9223372036854775807L, -2147483649L, 9223372036854775807L}.slice();

internal static void Main() {
    foreach (var (_, v) in interesting32) {
        fmt.Println(v);
    }

    foreach (var (_, v) in interesting64) {
        fmt.Println(v);
    }

    nint nmin = ((nint)(-9223372036854775808L));
    nint nctl = -unchecked((nint)9223372036854775807L);
    fmt.Println(nmin, nctl, nmin < nctl);

    var m = interesting32[0];
    fmt.Println(m == -2147483648, m + 1 == -2147483647);
}

} // end main_package

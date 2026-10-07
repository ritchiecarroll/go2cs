namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt maxInt => /* 1<<63 - 1 */ 9223372036854775807;

[GoType] partial struct splitTest {
    internal @string name;
    internal nint n;
}

internal static slice<splitTest> tests = new splitTest[]{
    new("quarter"u8, unchecked((nint)(2305843009213693951L))),
    new("eighth"u8, unchecked((nint)(1152921504606846976L))),
    new("boundary"u8, (nint)(2147483648L - 1)),
    new("efloat"u8, 1000000000 - 7),
    new("escale"u8, 500000000 * 2),
    new("zero"u8, maxInt - maxInt)
}.slice();

internal static partial nint half(nint n) {
    return n / 2;
}

internal static void Main() {
    foreach (var (_, tt) in tests) {
        fmt.Println(tt.name, tt.n);
    }
    fmt.Println(half(unchecked((nint)(4611686018427387903L))));
    fmt.Println(half(unchecked((nint)(2305843009213693948L))));
    fmt.Println(half((nint)(2147483648L - 1)));
    fmt.Println(half(2000000000 - 8));
    nint local = unchecked((nint)(576460752303423487L));
    fmt.Println(local);
    nint boundary = (nint)(2147483648L - 1);
    fmt.Println(boundary);
}

} // end main_package

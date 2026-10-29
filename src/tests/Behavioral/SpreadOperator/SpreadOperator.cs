namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static nint sum(params ꓸꓸꓸnint numsʗp) {
    var nums = numsʗp.sslice();

    nint total = 0;
    foreach (var (_, n) in nums) {
        total += n;
    }
    return total;
}

internal static void Main() {
    var values = slice<nint>([1, 2, 3]);

    fmt.Println(sum(values.ꓸꓸꓸ));

    fmt.Println(sum(1, 2, 3));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt levels => 2_000_000;

internal static nint depth(nint n) {
    if (n == 0) {
        return 0;
    }
    return depth(n - 1) + 1;
}

internal static partial void main() {
    var n = new channel<nint>(0);
    var nʗ1 = n;
    goǃ([GoFunc("main.func1")] () => {
        nʗ1.ᐸꟷ(levels);
    });
    fmt.Println("depth:", depth(ᐸꟷ(n)));
}

} // end main_package

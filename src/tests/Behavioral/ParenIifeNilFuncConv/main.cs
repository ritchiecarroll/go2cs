namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

internal delegate nint handlerFunc(@string _);

} partial class mainᴛhandlerFunc {

internal static nint call(this handlerFunc h, @string s) {
    if (h == nil) {
        return -1;
    }

    return h(s);
}

} partial class main_package {

internal static void main() {
    func(() => {
        fmt.Println("paren iife");
    })();

    nint sum = func(nint (nint a, nint b) => {
        return a + b;
    })(3, 4);
    fmt.Println(sum);

    func(() => {
        fmt.Println("bare iife");
    })();

    nint product = func(nint (nint a, nint b) => {
        return a * b;
    })(3, 4);
    fmt.Println(product);

    var h = default(handlerFunc)!;
    fmt.Println(h == nil, h.call("x"));

    h = new handlerFunc((@string s) => len(s));
    fmt.Println(h == nil, h.call("abcd"));

    var table = slice<handlerFunc>([default(handlerFunc)!, h]);
    foreach (var (_, entry) in range(table)) {
        fmt.Println(entry.call("zz"));
    }
}

} // end main_package

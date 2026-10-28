namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate nint handlerFunc(@string _);

internal static nint call(this handlerFunc h, @string s) {
    if (h == nil) {
        return -1;
    }

    return h(s);
}

internal static void Main() {
    ((Action)(() => {
        fmt.Println("paren iife");
    }))();

    nint sum = ((Func<nint, nint, nint>)((a, b) => {
        return a + b;
    }))(3, 4);
    fmt.Println(sum);

    ((Action)(() => {
        fmt.Println("bare iife");
    }))();

    nint product = ((Func<nint, nint, nint>)((a, b) => {
        return a * b;
    }))(3, 4);
    fmt.Println(product);

    var h = default(handlerFunc)!;
    fmt.Println(h == nil, h.call("x"));

    h = new handlerFunc((@string s) => len(s));
    fmt.Println(h == nil, h.call("abcd"));

    var table = new handlerFunc[]{default(handlerFunc)!, h}.slice();
    foreach (var (_, entry) in table) {
        fmt.Println(entry.call("zz"));
    }
}

} // end main_package

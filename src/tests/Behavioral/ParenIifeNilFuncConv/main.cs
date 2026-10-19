namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate nint handlerFunc(@string _);

internal static nint call(this handlerFunc h, @string s) {
    if (h == default!) {
        return -1;
    }

    return h(s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object parenIifeˢ = (@string)"paren iife"u8;
private static readonly object bareIifeˢ = (@string)"bare iife"u8;

internal static void Main() {
    ((Action)(() => {
        fmt.Println(parenIifeˢ);
    }))();

    nint sum = ((Func<nint, nint, nint>)((a, b) => {
        return a + b;
    }))(3, 4);
    fmt.Println(sum);

    ((Action)(() => {
        fmt.Println(bareIifeˢ);
    }))();

    nint product = ((Func<nint, nint, nint>)((a, b) => {
        return a * b;
    }))(3, 4);
    fmt.Println(product);

    var h = default(handlerFunc)!;
    fmt.Println(h == default!, h.call("x"));

    h = new handlerFunc((@string s) => len(s));
    fmt.Println(h == default!, h.call("abcd"));

    var table = new handlerFunc[]{default(handlerFunc)!, h}.slice();
    foreach (var (_, entry) in table) {
        fmt.Println(entry.call("zz"));
    }
}

} // end main_package

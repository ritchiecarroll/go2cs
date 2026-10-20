namespace go;

using fmt = fmt_package;

partial class main_package {

// type Compressor is a methodless func type — rendered inline as its base delegate

internal static Func<nint, nint> lookup(any i) {
    {
        var (c, ok) = i._<Func<nint, nint>>(ᐧ); if (ok) {
            return c;
        }
    }
    return default!;
}

internal static void Main() {
    any i = new Func<nint, nint>((nint w) => w * 2);
    var c = lookup(i);
    fmt.Println(c(21));

    any j = (nint)7;
    fmt.Println(lookup(j) == default!);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate nint Compressor(nint w);

internal static Compressor lookup(any i) {
    var (c, ok) = i._<Compressor>(ᐧ); if (ok) {
        return c;
    }
    return default;
}

internal static void main() {
    any i = new Compressor((nint w) => w * 2);
    var c = lookup(i);
    fmt.Println(c(21));

    any j = (nint)7;
    fmt.Println(lookup(j) == nil);
}

} // end main_package

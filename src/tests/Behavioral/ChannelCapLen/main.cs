namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var ch = new channel<nint>(3);
    fmt.Println(len(ch), cap(ch));
    ch.ᐸꟷ(1);
    fmt.Println(len(ch), cap(ch));
    ch.ᐸꟷ(2);
    ch.ᐸꟷ(3);
    fmt.Println(len(ch), cap(ch));
    fmt.Println(ᐸꟷ(ch), len(ch));
    ch.ᐸꟷ(4);
    fmt.Println(len(ch), cap(ch));
    fmt.Println(ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch));
    fmt.Println(len(ch), cap(ch));

    var u = new channel<@string>(0);
    fmt.Println(len(u), cap(u));

    channel<nint> n = default;
    fmt.Println(len(n), cap(n));

    var d = new channel<nint>(2);
    d.ᐸꟷ(7);
    d.ᐸꟷ(8);
    close(d);
    var (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
    (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
    (v, ok) = ᐸꟷ(d, ꟷ);
    fmt.Println(v, ok);
    fmt.Println(len(d), cap(d));
}

} // end main_package

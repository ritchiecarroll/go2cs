namespace go;

using fmt = fmt_package;
using ꓸꓸꓸbyte = Span<byte>;
using static main_package;

partial class main_package {

partial struct sink {
    internal slice<byte> buf;
}

} partial class mainᴛsink {

internal static void add(this ref sink s, params ꓸꓸꓸbyte bytesʗp) {
    var bytes = bytesʗp.sslice();

    s.buf = appendꓸꓸꓸ(s.buf, bytes);
}

} partial class main_package {

internal static void with(ж<sink> s, Action<ж<sink>> f) {
    f(s);
}

internal static void main() {
    var s = Ꮡ(new sink());
    with(s, (ж<sink> c) => {
        c.add(0xff);
        c.add(1, 2, 3);
        c.add();
        var more = slice<byte>([9, 8]);
        c.add(more.ꓸꓸꓸ);
    });
    fmt.Println(s.buf);

    var t = Ꮡ(new sink());
    t.add(7);
    t.add((byte)'A', (byte)'B');
    fmt.Println(t.buf);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸbyte = Span<byte>;

partial class main_package {

partial struct sink {
    internal slice<byte> buf;
}

internal static void add(this ref sink s, params ꓸꓸꓸbyte bytesʗp) {
    var bytes = bytesʗp.sslice();

    s.buf = appendꓸꓸꓸ(s.buf, bytes);
}

internal static void with(ж<sink> Ꮡs, Action<ж<sink>> f) {
    f(Ꮡs);
}

internal static void Main() {
    var s = Ꮡ(new sink(nil));
    with(s, (ж<sink> c) => {
        c.add(0xff);
        c.add(1, 2, 3);
        c.add();
        var more = new byte[]{9, 8}.slice();
        c.add(more.ꓸꓸꓸ);
    });
    fmt.Println((~s).buf);
    var t = Ꮡ(new sink(nil));
    t.add(7);
    t.add((byte)(rune)'A', (byte)(rune)'B');
    fmt.Println((~t).buf);
}

} // end main_package

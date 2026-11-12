namespace go;

using fmt = fmt_package;
using Δio = io_package;
using strings = strings_package;

partial class main_package {

partial struct Text {
    internal @string s;
}

internal static ж<strings.Reader> newReader(@string s) { return strings.NewReader(s); }

public static ж<strings.Reader> Reader(this ref Text stringsΔ1) { return newReader(stringsΔ1.s); }

public static ж<strings.Reader> Upper(this Text stringsΔ1) { return newReader(toUpper(stringsΔ1.s)); }

public static @string Prefix(this ref Text stringsΔ1, ж<strings.Reader> Ꮡr, nint n) {
    ref var r = ref Ꮡr.DerefOrNull();

    var buf = new slice<byte>(n);
    var (k, _) = r.Read(buf);
    return stringsΔ1.s + ":"u8 + ((@string)(buf.slice(0, k)));
}

public static ж<strings.Reader> CopyTo(this ref Text stringsΔ1, ж<Text> Ꮡdst) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.s = stringsΔ1.s + "!"u8;
    return newReader(dst.s);
}

public static (ж<Text>, ж<strings.Reader>) Addr(this Text stringsʗp) {
    ref var stringsΔ1 = ref heap(stringsʗp, out var ᏑstringsΔ1);

    var p = ᏑstringsΔ1;
    p.s += "?"u8;
    return (p, newReader(stringsΔ1.s));
}

public static nint Len(this ref Text strings) { return len(strings.s); }

internal static @string toUpper(@string s) { return strings.ToUpper(s); }

internal static @string readAll(Δio.Reader r) {
    var (b, _) = Δio.ReadAll(r);
    return (@string)b;
}

internal static void main() {
    var t = Ꮡ(new Text(s: "abc"));
    fmt.Println("reader:", readAll(new strings_ReaderжReader(t.Reader())));
    fmt.Println("upper:", readAll(new strings_ReaderжReader(t.Upper())));
    fmt.Println("prefix:", t.Prefix(strings.NewReader("xyz"), 2));

    ref var dst = ref heap(new Text(), out var Ꮡdst);
    fmt.Println("copy:", readAll(new strings_ReaderжReader(t.CopyTo(Ꮡdst))), dst.s);

    var (p, r) = t.Addr();
    fmt.Println("addr:", p.s, readAll(new strings_ReaderжReader(r)), t.s);
    fmt.Println("len:", t.Len());
}

} // end main_package

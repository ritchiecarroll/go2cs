namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

partial struct reader {
    internal @string src;
    internal nint pos;
}

internal static (nint, error) Read(this ж<reader> Ꮡr, slice<byte> p) {
    ref var r = ref Ꮡr.DerefOrNull();

    var q = Ꮡr.of(reader.Ꮡpos);
    if (q.Value >= len(r.src)) {
        return (0, default);
    }
    nint n = copy(p, r.src.slice(q.Value));
    q.Value += n;
    return (n, default);
}

internal static error Close(this ref reader r) {
    r.pos = len(r.src);
    return default;
}

partial struct writer {
    internal slice<byte> @out;
    internal nint flush;
}

internal static (nint, error) Write(this ref writer w, slice<byte> p) {
    w.@out = appendꓸꓸꓸ(w.@out, p);
    return (len(p), default);
}

internal static error Close(this ref writer w) {
    w.flush++;
    return default;
}

internal static @string String(this ref writer w) {
    return (@string)w.@out;
}

partial struct duplex {
    /*embed*/ internal ж<reader> reader;
    /*embed*/ internal ж<writer> writer;
}

internal static error Close(this ref duplex d) {
    {
        var err = d.reader.Close(); if (err != nil) {
            return err;
        }
    }
    return d.writer.Close();
}

partial interface readWriteCloser {
    (nint, error) Read(slice<byte> p);
    (nint, error) Write(slice<byte> p);
    error Close();
}

partial interface readWriter {
    (nint, error) Read(slice<byte> p);
    (nint, error) Write(slice<byte> p);
}

partial struct foreign {
    /*embed*/ public ж<strings_package.Reader> Reader;
    /*embed*/ public ж<strings_package.Builder> Builder;
}

partial interface readStringWriter {
    (nint, error) Read(slice<byte> p);
    (nint, error) WriteString(@string s);
}

internal static void Main() {
    var r = Ꮡ(new reader(src: "hello world"));
    var w = Ꮡ(new writer(nil));
    var d = Ꮡ(new duplex(r, w));

    readWriteCloser rwc = new duplexжreadWriteCloser(d);

    var buf = new slice<byte>(5);
    var (n, err) = rwc.Read(buf);
    fmt.Println("read:", n, (@string)(buf.slice(0, n)), err == nil);

    (n, err) = rwc.Write(buf.slice(0, n));
    fmt.Println("write:", n, err == nil);

    fmt.Println("aliased:", (~r).pos, w.String());

    fmt.Println("close:", rwc.Close() == nil, (~r).pos, (~w).flush);

    readWriter rw = d.Value;
    (n, _) = rw.Write(slice<byte>("!"u8));
    fmt.Println("value form:", n, w.String());

    var f = Ꮡ(new foreign(strings.NewReader("abc"), Ꮡ(new strings.Builder(nil))));
    readStringWriter rsw = new foreignжreadStringWriter(f);

    var fbuf = new slice<byte>(3);
    (n, _) = rsw.Read(fbuf);
    var (n2, _) = rsw.WriteString(((sstring)(fbuf.slice(0, n))) + "def"u8);
    fmt.Println("foreign:", n, n2, (~f).Builder.String());

    readStringWriter frsw = f.Value;
    var (n3, _) = frsw.WriteString("gh");
    fmt.Println("foreign value form:", n3, (~f).Builder.String());
}

} // end main_package

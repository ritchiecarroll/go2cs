namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using transform = golang.org.x.text.transform_package;
using golang.org.x.text;

partial class main_package {

partial struct upper {
    /*embed*/ public golang.org.x.text.transform_package.NopResetter NopResetter;
}

internal static (nint nDst, nint nSrc, error err) Transform(this upper _, slice<byte> dst, slice<byte> src, bool atEOF) {
    error err = default;

    nint n = copy(dst, src);

    for (nint i = 0; i < n; i++) {
        var c = dst[i]; if ('a' <= c && c <= 'z') {
            dst[i] = (byte)(c - 'a' + 'A');
        }
    }

    if (n < len(src)) {
        err = transform.ErrShortDst;
    }

    return (n, n, err);
}

internal static void Main() {
    var (s, n, err) = transform.String(new transform_SpanningTransformerᴠTransformer(transform.Nop), "abc");
    fmt.Println(s, n, err);

    (s, n, err) = transform.String(new upper(), "vendored path");
    fmt.Println(s, n, err);

    transform.Transformer t = new upper();
    var (_, spanning) = t._<transform.SpanningTransformer>(ᐧ);
    fmt.Println(spanning);

    (_, spanning) = ((transform.Transformer)transform.Nop)._<transform.SpanningTransformer>(ᐧ);
    fmt.Println(spanning);

    fmt.Println(reflect.TypeOf(new transform.NopResetter()).PkgPath());
    fmt.Println(transform.ErrShortDst);
}

} // end main_package

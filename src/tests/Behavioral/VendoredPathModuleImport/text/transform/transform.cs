namespace go.golang.org.x.text;

using errors = errors_package;

partial class transform_package {

public static error ErrShortDst = errors.New("transform: short destination buffer");

partial interface Transformer {
    (nint nDst, nint nSrc, error err) Transform(slice<byte> dst, slice<byte> src, bool atEOF);
    void Reset();
}

partial interface SpanningTransformer :
    Transformer
{
    (nint n, error err) Span(slice<byte> src, bool atEOF);
}

partial struct NopResetter {
}

public static void Reset(this NopResetter _) { }

partial struct nop {
    /*embed*/ public NopResetter NopResetter;
}

internal static (nint nDst, nint nSrc, error err) Transform(this nop _, slice<byte> dst, slice<byte> src, bool atEOF) {
    error err = default;

    nint n = copy(dst, src);

    if (n < len(src)) {
        err = ErrShortDst;
    }

    return (n, n, err);
}

internal static (nint n, error err) Span(this nop _, slice<byte> src, bool atEOF) {
    return (len(src), default);
}

public static SpanningTransformer Nop = new nop();

public static (@string result, nint n, error err) String(Transformer t, @string s) {
    nint n = default;

    t.Reset();

    var src = slice<byte>(s);
    var dst = new slice<byte>(0, len(src));
    var buf = new slice<byte>(len(src) + 1);

    while (len(src) > 0) {
        var (nDst, nSrc, errΔ1) = t.Transform(buf, src, true);
        dst = appendꓸꓸꓸ(dst, buf.slice(0, nDst));
        n += nSrc;
        src = src.slice(nSrc);

        if (AreEqual(errΔ1, ErrShortDst)) {
            buf = new slice<byte>(2 * len(buf));
            continue;
        }

        if (errΔ1 != nil) {
            return ((@string)dst, n, errΔ1);
        }

        if (nSrc == 0 && nDst == 0) {
            break;
        }
    }

    return ((@string)dst, n, default);
}

} // end transform_package

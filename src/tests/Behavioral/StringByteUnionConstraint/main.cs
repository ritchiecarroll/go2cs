namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 byteSum<T>(T s)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    uint32 h = default!;
    for (nint i = 0; i < len(s); i++) {
        h = h * 31 + (uint32)s[i];
    }
    return h;
}

internal static bool prefixMatch<T>(T s, T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    nint n = len(sep);
    if (len(s) < n) {
        return false;
    }
    return s.slice(0, n).ToGoString() == sep.ToGoString();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;
private static readonly @string helˢ = "hel"u8;
private static readonly object sumStringˢ = (@string)"sum(string):"u8;
private static readonly object sumByteˢ = (@string)"sum([]byte):"u8;
private static readonly object prefixStringˢ = (@string)"prefix string:"u8;
private static readonly object prefixByteˢ = (@string)"prefix []byte:"u8;
private static readonly object prefixMissˢ = (@string)"prefix miss:"u8;
private static readonly @string xyzˢ = "xyz"u8;
private static readonly object prefixTooLongˢ = (@string)"prefix too long:"u8;
private static readonly @string helloWorldˢ = "hello world"u8;
private static readonly object lastStringˢ = (@string)"last(string):"u8;
private static readonly object lastByteˢ = (@string)"last([]byte):"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object headˢ = (@string)"head:"u8;
private static readonly @string x9876ˢ = "x98:76"u8;
private static readonly object appendRunStringˢ = (@string)"appendRun(string):"u8;
private static readonly @string abcdeˢ = "abcde"u8;
private static readonly object appendRunByteˢ = (@string)"appendRun([]byte):"u8;
private static readonly object wholeStringˢ = (@string)"whole(string):"u8;
private static readonly object wholeByteˢ = (@string)"whole([]byte):"u8;

internal static void Main() {
    @string str = helloˢ;
    var bs = slice<byte>(str);
    @string hel = helˢ;
    var helBytes = slice<byte>(hel);
    fmt.Println(sumStringˢ, byteSum(str));
    fmt.Println(sumByteˢ, byteSum(bs));
    fmt.Println(prefixStringˢ, prefixMatch(str, hel));
    fmt.Println(prefixByteˢ, prefixMatch(bs, helBytes));
    fmt.Println(prefixMissˢ, prefixMatch(str, xyzˢ));
    fmt.Println(prefixTooLongˢ, prefixMatch(str, helloWorldˢ));
    fmt.Println(lastStringˢ, lastByte(str));
    fmt.Println(lastByteˢ, lastByte(bs));
    fmt.Println(sumˢ, digitSum((@string)"12:34"), digitSum(slice<byte>("56:78"u8)));
    fmt.Println(headˢ, headSum(x9876ˢ), headSum(slice<byte>("y10:23"u8)));
    fmt.Println(appendRunStringˢ, ((@string)appendRun(default!, abcdeˢ)));
    fmt.Println(appendRunByteˢ, ((@string)appendRun(slice<byte>("<"u8), slice<byte>("abcde"u8))));
    fmt.Println(wholeStringˢ, ((@string)wholeSpan(str)), wholeByteˢ, ((@string)wholeSpan(bs)));
}

internal static nint digitSum<T>(T s)
    where T : /* []byte | string */ IByteSeq<T, byte>, new()
{
    nint parse(T part) {
        nint n = 0;
        foreach (var (_, c) in part.ToSlice()) {
            n = n * 10 + (nint)((byte)(c - (rune)'0'));
        }
        return n;
    }
    return parse(s[0..2]) + parse(s[3..5]);
}

internal static T trimHead<T>(T s, nint n)
    where T : /* []byte | string */ IByteSeq<T, byte>, new()
{
    for (nint i = 0; i < n; i++) {
        if (len(s) > 1) {
            s = s[1..];
        }
    }
    return s[0..];
}

internal static nint headSum<T>(T s)
    where T : /* []byte | string */ IByteSeq<T, byte>, new()
{
    return digitSum(trimHead(s, 1));
}

internal static byte lastByte<T>(T s)
    where T : /* []byte | string */ IByteSeq<T, byte>, new()
{
    return s[len(s) - 1];
}

internal static T wholeSpan<T>(T s)
    where T : /* []byte | string */ IByteSeq<T, byte>, new()
{
    return s[..];
}

internal static slice<byte> appendRun<Bytes>(slice<byte> dst, Bytes src)
    where Bytes : /* []byte | string */ IByteSeq<Bytes, byte>, new()
{
    dst = append(dst, (byte)((rune)'['));
    if (len(src) > 1) {
        dst = append(dst, src.slice(1, len(src) - 1).ꓸꓸꓸ);
        dst = append(dst, src.slice(len(src) - 1).ꓸꓸꓸ);
    }
    dst = append(dst, (byte)((rune)']'));
    return dst;
}

} // end main_package

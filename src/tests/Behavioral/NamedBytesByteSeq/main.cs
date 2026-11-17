namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Val /*[]byte*/;

partial struct Ptr /*@string*/;

} partial class mainᴛVal {

public static Val Clone(this Val v) {
    return appendꓸꓸꓸ((Val)default, v);
}

} partial class main_package {

internal static @string prefix<Bytes>(Bytes b, nint n)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    return b.slice(0, n).ToGoString();
}

internal static nint sum<Bytes>(Bytes b)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    nint t = 0;
    for (nint i = 0; i < len(b); i++) {
        t += (nint)b[i];
    }
    return t;
}

internal static slice<byte> spread<Bytes>(slice<byte> dst, Bytes b)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    return append(dst, b[1..].ꓸꓸꓸ);
}

internal static slice<byte> toBytes<Bytes>(Bytes b)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    return b.ToSlice();
}

internal static Val toVal<Bytes>(Bytes b)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    return (Val)b.ToSlice();
}

internal static Ptr toPtr<Bytes>(Bytes b)
    where Bytes : /* ~[]byte | ~string */ IByteSeq<Bytes, byte>, new()
{
    return (Ptr)b.ToGoString();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string wxyzˢ = "wxyz"u8;
private static readonly @string xyzˢ = "xyz"u8;
private static readonly @string litˢ = "lit"u8;

internal static void main() {
    var v = (Val)slice<byte>((@string)"abcd"u8);
    Ptr p = (Ptr)(@string)wxyzˢ;

    fmt.Println(prefix(v, 2), prefix(v[1..], 2), prefix(p, 3), prefix(xyzˢ, 1));
    fmt.Println(sum(v), sum(p), sum(v[2..]), sum(slice<byte>("ab"u8)));
    fmt.Println((@string)spread(slice<byte>("<"u8), v), (@string)spread(slice<byte>("<"u8), p));

    var b = toBytes(v);
    b[0] = (byte)'A';
    fmt.Println((@string)(slice<byte>)v, (@string)b);

    b = toBytes(v[2..]);
    b[0] = (byte)'C';
    fmt.Println((@string)(slice<byte>)v, (@string)b);

    b = toBytes(p);
    b[0] = (byte)'W';
    fmt.Println((@string)p, (@string)b);

    var w = toVal(v);
    w[1] = (byte)'B';
    fmt.Println((@string)(slice<byte>)v, (@string)(slice<byte>)w);
    w = toVal(p);
    w[1] = (byte)'X';
    fmt.Println((@string)p, (@string)(slice<byte>)w);
    w = toVal((@string)"pq");
    fmt.Println((@string)(slice<byte>)w, len(w));

    fmt.Println(toPtr(v), toPtr(p), toPtr(litˢ), toPtr(v[1..3]) == "BC"u8, len(toPtr(p[1..])));

    var c = v.Clone();
    c[0] = (byte)'z';
    fmt.Println((@string)(slice<byte>)v, (@string)(slice<byte>)c);
}

} // end main_package

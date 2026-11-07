namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Stringish<ΔT> {
    //  Type constraints: ~[]byte | ~string
    // Derived operators: none
}

partial struct Iterator<T>
    where T : /* Stringish = ~[]byte | ~string */ new()
{
    internal T data;
    internal nint start, pos;
}

public static ж<Iterator<T>> NewIterator<T>(T data)
    where T : /* Stringish = ~[]byte | ~string */ new()
{
    return Ꮡ(new Iterator<T>(data: data));
}

public static bool Next<T>(this ref Iterator<T> it, nint width)
    where T : /* Stringish = ~[]byte | ~string */ new()
{
    if (it.pos >= len(it.data)) {
        return false;
    }
    it.start = it.pos;
    it.pos += width;
    if (it.pos > len(it.data)) {
        it.pos = len(it.data);
    }
    return true;
}

public static T Value<T>(this ref Iterator<T> it)
    where T : /* Stringish = ~[]byte | ~string */ new()
{
    return it.data.slice(it.start, it.pos);
}

internal static nint count<T>(T s, byte b)
    where T : /* ~string | ~[]byte */ new()
{
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == b) {
            n++;
        }
    }
    return n;
}

internal static T head<T>(T s, nint n)
    where T : /* Stringish = ~[]byte | ~string */ new()
{
    if (n > len(s)) {
        n = len(s);
    }
    return s.slice(0, n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloWorldˢ = "hello, world"u8;
private static readonly @string bananaˢ = "banana"u8;
private static readonly @string gopherˢ = "gopher"u8;

internal static void Main() {
    var it = NewIterator(helloWorldˢ);
    while (it.Next(5)) {
        fmt.Printf("%q\n"u8, it.Value());
    }

    var ib = NewIterator(slice<byte>("abcdefg"u8));
    while (ib.Next(3)) {
        fmt.Printf("%q\n"u8, ib.Value());
    }

    fmt.Println(count(bananaˢ, (rune)'a'), count(slice<byte>("banana"u8), (rune)'n'));
    fmt.Printf("%q %q\n"u8, head(gopherˢ, 3), head(slice<byte>("go"u8), 5));
}

} // end main_package

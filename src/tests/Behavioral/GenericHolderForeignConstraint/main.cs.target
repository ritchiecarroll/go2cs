namespace go;

using fmt = fmt_package;
using text = GenericHolderForeignConstraint.text_package;
using GenericHolderForeignConstraint;

partial class main_package {

partial struct Iterator<T>
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    internal T data;
    internal nint pos;
}

public static void SetText<T>(this ref Iterator<T> it, T data)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    it.data = data;
    it.pos = 0;
}

public static bool Next<T>(this ref Iterator<T> it)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    if (it.pos >= len(it.data)) {
        return false;
    }

    it.pos++;

    return true;
}

public static nint Pos<T>(this Iterator<T> it)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return it.pos; }

partial struct Window<K, T>
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    internal K key;
    internal T data;
}

public static nint Len<K, T>(this Window<K, T> w)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return len(w.data); }

public static K Key<K, T>(this ref Window<K, T> w)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return w.key; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gopherˢ = "gopher"u8;

internal static void main() {
    var it = Ꮡ(new Iterator<@string>());
    it.SetText(gopherˢ);

    nint steps = 0;
    while (it.Next()) {
        steps++;
    }

    fmt.Println("string steps:", steps, "pos:", it.Pos());

    var bytes = new Iterator<slice<byte>>();
    bytes.SetText(slice<byte>("go"u8));
    bytes.Next();
    fmt.Println("bytes pos:", bytes.Pos());

    var w = new Window<nint, slice<byte>>(key: 7, data: slice<byte>("abc"u8));
    fmt.Println("window:", w.Key(), w.Len());

    var named = new Window<@string, @string>(key: "k", data: "hello");
    fmt.Println("named window:", named.Key(), named.Len());
}

} // end main_package

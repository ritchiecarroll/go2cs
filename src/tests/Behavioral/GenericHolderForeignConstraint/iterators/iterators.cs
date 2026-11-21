namespace go.GenericHolderForeignConstraint;

using text = go.GenericHolderForeignConstraint.text_package;
using go.GenericHolderForeignConstraint;

partial class iterators_package {

partial struct Iterator<T>
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    internal T data;
    internal nint pos;
}

internal static void SetText⁔<T>(/*this*/ ref Iterator<T> it, T data)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    it.data = data;
    it.pos = 0;
}

internal static bool Next⁔<T>(/*this*/ ref Iterator<T> it)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    if (it.pos >= len(it.data)) {
        return false;
    }

    it.pos++;

    return true;
}

internal static nint Pos⁔<T>(/*this*/ Iterator<T> it)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return it.pos; }

partial struct Window<K, T>
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    internal K key;
    internal T data;
}

public static Window<K, T> NewWindow<K, T>(K key, T data)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{
    return new Window<K, T>(key: key, data: data);
}

internal static nint Len⁔<K, T>(/*this*/ Window<K, T> w)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return len(w.data); }

internal static K Key⁔<K, T>(/*this*/ ref Window<K, T> w)
    where T : /* text.Text */ IByteSeq<T, byte>, new()
{ return w.key; }

} // end iterators_package

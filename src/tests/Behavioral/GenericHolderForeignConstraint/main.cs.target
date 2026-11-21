namespace go;

using fmt = fmt_package;
using iterators = GenericHolderForeignConstraint.iterators_package;
using GenericHolderForeignConstraint;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gopherˢ = "gopher"u8;
private static readonly @string helloˢ = "hello"u8;

internal static void main() {
    var it = Ꮡ(new iterators.Iterator<@string>());
    it.SetText(gopherˢ);

    nint steps = 0;
    while (it.Next()) {
        steps++;
    }

    fmt.Println("string steps:", steps, "pos:", it.Pos());

    var bytes = new iterators.Iterator<slice<byte>>();
    bytes.SetText(slice<byte>("go"u8));
    bytes.Next();
    fmt.Println("bytes pos:", bytes.Pos());

    var w = iterators.NewWindow<nint, slice<byte>>(7, slice<byte>("abc"u8));
    fmt.Println("window:", w.Key(), w.Len());

    var named = iterators.NewWindow<@string, @string>((@string)"k", helloˢ);
    fmt.Println("named window:", named.Key(), named.Len());
}

} // end main_package

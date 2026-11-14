namespace go;

using fmt = fmt_package;
using boxlib = GenericInstClosedIfaceConsumer.boxlib_package;
using GenericInstClosedIfaceConsumer;

partial class main_package {

internal static void main() {
    var lib = boxlib.Wrap(1, 2);
    fmt.Println("library", lib.Val().X, lib.Val().Y);

    boxlib.Valuer own = new boxlib_BoxᴠValuer(new boxlib.Box<boxlib.Point>(V: new boxlib.Point(X: 1, Y: 2)));
    fmt.Println("consumer", own.Val().X, own.Val().Y);

    fmt.Println("equal", AreEqual(lib, own));

    var (back, ok) = lib._<boxlib.Box<boxlib.Point>>(ᐧ);
    fmt.Println("assert back", back.V.X, back.V.Y, ok);

    var other = boxlib.Wrap(3, 4);
    fmt.Println("not equal", AreEqual(other, own));
}

} // end main_package

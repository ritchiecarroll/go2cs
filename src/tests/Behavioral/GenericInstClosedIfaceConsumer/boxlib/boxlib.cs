namespace go.GenericInstClosedIfaceConsumer;

using static boxlib_package;

partial class boxlib_package {

partial struct Point {
    public nint X, Y;
}

partial struct Box<T> {
    public T V;
}

} partial class boxlibᴛBox {

public static T Val<T>(this Box<T> b) { return b.V; }

} partial class boxlib_package {

partial interface Valuer {
    Point Val();
}

public static Valuer Wrap(nint x, nint y) {
    return new BoxᴠValuer(new Box<Point>(new Point(x, y)));
}

} // end boxlib_package

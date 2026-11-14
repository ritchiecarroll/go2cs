namespace go.GenericInstClosedIfaceConsumer;

partial class boxlib_package {

partial struct Point {
    public nint X, Y;
}

partial struct Box<T> {
    public T V;
}

public static T Val<T>(this Box<T> b) { return b.V; }

partial interface Valuer {
    Point Val();
}

public static Valuer Wrap(nint x, nint y) {
    return new BoxᴠValuer(new Box<Point>(new Point(x, y)));
}

} // end boxlib_package

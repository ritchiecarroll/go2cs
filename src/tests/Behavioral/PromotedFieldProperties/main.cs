namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Point {
    public nint X, Y;
}

partial struct Named {
    public @string Name;
}

partial struct Shape {
    /*embed*/ public Point Point;
    /*embed*/ public Named Named;
    internal nint sides;
}

partial struct Labeled {
    /*embed*/ public ж<Shape> Shape;
    internal @string label;
}

partial struct Deep {
    /*embed*/ public Labeled Labeled;
    internal nint depth;
}

internal static void Main() {
    var shapes = slice<ж<Shape>>([Ꮡ(new Shape(Point: new Point(1, 2), Named: new Named("tri"), sides: 3)), Ꮡ(new Shape(Point: new Point(4, 5), Named: new Named("sq"), sides: 4))]);

    foreach (var (_, s) in shapes) {
        s.X += 10;
        s.Y *= 2;
        s.Name += "!"u8;
        fmt.Println(s.X, s.Y, s.Name, s.sides);
    }

    var labeled = slice<ж<Labeled>>([Ꮡ(new Labeled(Shape: shapes[0], label: "first"u8))]);
    labeled[0].X = 100;
    fmt.Println(shapes[0].X, labeled[0].Name, labeled[0].label);

    var deep = slice<ж<Deep>>([Ꮡ(new Deep(Labeled: new Labeled(Shape: shapes[1], label: "second"), depth: 2))]);
    deep[0].Y++;
    deep[0].label += "?"u8;
    fmt.Println(shapes[1].Y, deep[0].label, deep[0].Name, deep[0].depth);

    readNil();
}

internal static void readNil() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("recovered:", recover() != nil);
        }, ref ᒐ);

        var broken = slice<ж<Labeled>>([Ꮡ(new Labeled(label: "nil shape"u8))]);
        fmt.Println(broken[0].X);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

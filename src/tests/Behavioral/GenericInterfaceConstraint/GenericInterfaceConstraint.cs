namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Shape {
    float64 Area();
    @string Name();
}

partial interface Round :
    Shape
{
    float64 Diameter();
}

partial struct Circle {
    public float64 R;
}

public static float64 Area(this ref Circle c) {
    return 3.0D * c.R * c.R;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string circleˢ = "circle"u8;

public static @string Name(this ref Circle c) {
    return circleˢ;
}

public static float64 Diameter(this ref Circle c) {
    return 2.0D * c.R;
}

partial struct Square {
    public float64 S;
}

public static float64 Area(this ref Square s) {
    return s.S * s.S;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string squareˢ = "square"u8;

public static @string Name(this ref Square s) {
    return squareˢ;
}

internal static float64 totalArea<S>(slice<S> shapes)
    where S : Shape
{
    float64 sum = default!;
    foreach (var (_, s) in shapes) {
        sum += s.Area();
    }
    return sum;
}

internal static void walkAll<S>(slice<S> shapes)
    where S : Shape
{
    foreach (var (_, s) in shapes) {
        show(s);
    }
}

internal static void show(Shape s) {
    fmt.Printf("%s: %.2f\n"u8, s.Name(), s.Area());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noFactoryˢ = "no factory"u8;

internal static @string makeShape<S>(Func<S> factory)
    where S : Shape
{
    if (factory == default!) {
        return noFactoryˢ;
    }
    var (a, b) = (factory(), factory());
    return fmt.Sprintf("%s %.2f %.2f"u8, a.Name(), a.Area(), b.Area());
}

internal static ж<Circle> newUnitCircle() {
    return Ꮡ(new Circle(R: 1D));
}

partial interface Figure {
    float64 Area();
    @string Name();
}

internal static Figure newFigure() {
    return new SquareжFigure(Ꮡ(new Square(S: 2D)));
}

internal static void Main() {
    var circles = new ж<Circle>[]{Ꮡ(new Circle(R: 1D)), Ꮡ(new Circle(R: 2D))}.slice();
    var squares = new ж<Square>[]{Ꮡ(new Square(S: 3D))}.slice();
    var shapes = new Shape[]{new CircleжShape(Ꮡ(new Circle(R: 1D))), new SquareжShape(Ꮡ(new Square(S: 2D)))}.slice();
    var rounds = new Round[]{new CircleжRound(Ꮡ(new Circle(R: 4D)))}.slice();
    fmt.Printf("circles: %.2f\n"u8, totalArea(widen<ж<Circle>, Shape>(circles, elemᴛ0 => new CircleжShape(elemᴛ0))));
    fmt.Printf("squares: %.2f\n"u8, totalArea(widen<ж<Square>, Shape>(squares, elemᴛ0 => new SquareжShape(elemᴛ0))));
    fmt.Printf("shapes: %.2f\n"u8, totalArea(shapes));
    fmt.Printf("rounds: %.2f\n"u8, totalArea(rounds));
    walkAll(widen<ж<Circle>, Shape>(circles, elemᴛ0 => new CircleжShape(elemᴛ0)));
    walkAll(shapes);
    walkAll(rounds);
    var shared = Ꮡ(new Circle(R: 1D));
    Func<ж<Circle>> none = default!;
    fmt.Println(makeShape<Shape>(widen<ж<Circle>, Shape>(newUnitCircle, elemᴛ0 => new CircleжShape(elemᴛ0))));
    var sharedʗ1 = shared;
    fmt.Println(makeShape(widen<ж<Circle>, Shape>(ж<Circle> () => {
        sharedʗ1.Value.R++;
        return sharedʗ1;
    }, elemᴛ0 => new CircleжShape(elemᴛ0))));
    fmt.Println(makeShape(widen<ж<Circle>, Shape>(none, elemᴛ0 => new CircleжShape(elemᴛ0))));
    var fig = ((Figure)new CircleжFigure(Ꮡ(new Circle(R: 2D))));
    fmt.Println(makeShape<Shape>(widen<Figure, Shape>(newFigure, elemᴛ0 => new FigureᴠShape(elemᴛ0))));
    var figʗ1 = fig;
    fmt.Println(makeShape(widen<Figure, Shape>(Figure () => figʗ1, elemᴛ0 => new FigureᴠShape(elemᴛ0))));
    fmt.Println(makeShape(Round () => new CircleжRound(Ꮡ(new Circle(R: 1D)))));
}

} // end main_package

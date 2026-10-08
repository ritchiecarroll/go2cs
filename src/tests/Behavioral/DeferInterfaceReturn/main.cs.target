namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Shape {
    nint Area();
}

partial struct Circle {
    public nint R;
}

public static nint Area(this Circle c) {
    return 3 * c.R * c.R;
}

partial struct Square {
    public nint S;
}

public static nint Area(this Square s) {
    return s.S * s.S;
}

internal static Shape pick(nint kind) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            _ = recover();
        }, ref ᒐ);
        if (kind == 0) {
            return new Circle(R: 2);
        }
        return new Square(S: 5);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (Shape, bool) classify(nint kind) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            _ = recover();
        }, ref ᒐ);
        if (kind == 0) {
            return (new Circle(R: 3), true);
        }
        return (new Square(S: 4), false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    fmt.Println(pick(0).Area());
    fmt.Println(pick(1).Area());
    var (s0, ok0) = classify(0);
    fmt.Println(s0.Area(), ok0);
    var (s1, ok1) = classify(1);
    fmt.Println(s1.Area(), ok1);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial interface Shape {
    nint Area();
    nint Perimeter();
}

partial struct Rect {
    internal nint w, h;
}

partial struct Circle {
    internal nint r;
}

partial struct Tri {
    internal nint a, b, c;
}

public static nint Area(this Rect r) {
    return r.w * r.h;
}

public static nint Perimeter(this Rect r) {
    return 2 * (r.w + r.h);
}

public static nint Area(this Circle c) {
    return 3 * c.r * c.r;
}

public static nint Perimeter(this Circle c) {
    return 6 * c.r;
}

public static nint Area(this Tri t) {
    return t.a * t.b / 2;
}

public static nint Perimeter(this Tri t) {
    return t.a + t.b + t.c;
}

internal static nint run(nint n) {
    var shapes = new Shape[]{new Rect(3, 4), new Circle(2), new Tri(6, 3, 7), new Rect(1, 9), new Circle(5), new Tri(4, 4, 6)}.slice();
    nint total = 0;

    for (nint i = 0; i < n; i++) {
        var s = shapes[i % len(shapes)];
        total += s.Area();
        total += s.Perimeter();
    }

    return total;
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint total = run(50000000);

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package

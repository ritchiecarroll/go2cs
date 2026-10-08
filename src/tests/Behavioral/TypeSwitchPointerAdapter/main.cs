namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
}

partial struct circle {
    internal nint r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string circleˢ = "circle"u8;

internal static @string name(this ref circle c) {
    return circleˢ;
}

partial struct square {
    internal nint s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string squareˢ = "square"u8;

internal static @string name(this ref square q) {
    return squareˢ;
}

partial struct dot {
    internal nint tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dotˢ = "dot"u8;

internal static @string name(this dot d) {
    return dotˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilShapeˢ = "nil shape"u8;
private static readonly @string unknownˢ = "unknown"u8;

internal static @string classify(shape v) {
    switch (v.type()) {
    case ж<circle> t: {
        return "ptr "u8 + t.name();
    }
    case ж<square> t: {
        return "ptr "u8 + t.name();
    }
    case dot t: {
        return "val "u8 + t.name();
    }
    case null: {
        return nilShapeˢ;
    }
    default: {
        var t = v;
        return unknownˢ;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string multiOtherˢ = "multi other"u8;

internal static @string classifyMulti(shape v) {
    switch (v.type()) {
    case ж<circle> _:
    case ж<square> _: {
        var t = v;
        return "multi ptr "u8 + t.name();
    }
    case dot t: {
        return "multi val "u8 + t.name();
    }
    default: {
        var t = v;
        return multiOtherˢ;
    }}
}

internal static void grow(shape v) {
    switch (v.type()) {
    case ж<circle> t: {
        t.Value.r += 10;
        break;
    }
    case ж<square> t: {
        t.Value.s += 10;
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anyHoldsCircleˢ = (@string)"any holds *circle"u8;
private static readonly object anyMissˢ = (@string)"any miss"u8;
private static readonly object shapeHoldsSquareˢ = (@string)"shape holds *square"u8;
private static readonly object shapeMissˢ = (@string)"shape miss"u8;

internal static void Main() {
    var c = Ꮡ(new circle(r: 1));
    var q = Ꮡ(new square(s: 2));
    var d = new dot(tag: 3);
    fmt.Println(classify(new circleжshape(c)));
    fmt.Println(classify(new squareжshape(q)));
    fmt.Println(classify(d));
    fmt.Println(classify(default!));
    fmt.Println(classifyMulti(new circleжshape(c)));
    fmt.Println(classifyMulti(new squareжshape(q)));
    fmt.Println(classifyMulti(d));
    grow(new circleжshape(c));
    grow(new squareжshape(q));
    fmt.Println((~c).r, (~q).s);
    any x = c.OrTypedNil();
    switch (x.type()) {
    case ж<circle>: {
        fmt.Println(anyHoldsCircleˢ);
        break;
    }
    default: {
        fmt.Println(anyMissˢ);
        break;
    }}

    shape v = new squareжshape(q);
    switch (v.type()) {
    case ж<square>: {
        fmt.Println(shapeHoldsSquareˢ);
        break;
    }
    default: {
        fmt.Println(shapeMissˢ);
        break;
    }}

}

} // end main_package

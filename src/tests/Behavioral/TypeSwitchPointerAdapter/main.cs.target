namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
}

partial struct circle {
    internal nint r;
}

internal static @string name(this ref circle c) {
    return "circle";
}

partial struct square {
    internal nint s;
}

internal static @string name(this ref square q) {
    return "square";
}

partial struct dot {
    internal nint tag;
}

internal static @string name(this dot d) {
    return "dot";
}

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
        return "nil shape";
    }
    default: {
        var t = v;
        return "unknown";
    }}
}

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
        return "multi other";
    }}
}

internal static void grow(shape v) {
    switch (v.type()) {
    case ж<circle> t: {
        t.r += 10;
        break;
    }
    case ж<square> t: {
        t.s += 10;
        break;
    }}
}

internal static void Main() {
    var c = Ꮡ(new circle(r: 1));
    var q = Ꮡ(new square(s: 2));
    var d = new dot(tag: 3);

    fmt.Println(classify(new circleжshape(c)));
    fmt.Println(classify(new squareжshape(q)));
    fmt.Println(classify(d));
    fmt.Println(classify(default));

    fmt.Println(classifyMulti(new circleжshape(c)));
    fmt.Println(classifyMulti(new squareжshape(q)));
    fmt.Println(classifyMulti(d));

    grow(new circleжshape(c));
    grow(new squareжshape(q));
    fmt.Println(c.r, q.s);

    any x = c.OrTypedNil();
    switch (x.type()) {
    case ж<circle>: {
        fmt.Println("any holds *circle");
        break;
    }
    default: {
        fmt.Println("any miss");
        break;
    }}

    shape v = new squareжshape(q);
    switch (v.type()) {
    case ж<square>: {
        fmt.Println("shape holds *square");
        break;
    }
    default: {
        fmt.Println("shape miss");
        break;
    }}

}

} // end main_package

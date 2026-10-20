namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
}

partial struct circle {
    internal nint r;
}

partial struct square {
    internal nint s;
}

partial struct dot {
}

internal static @string name(this circle c) {
    return "circle";
}

internal static @string name(this square s) {
    return "square";
}

internal static @string name(this dot d) {
    return "dot";
}

internal static @string describe(shape v) {
    switch (v.type()) {
    case circle _:
    case square _: {
        var t = v;
        return "both:"u8 + t.name();
    }
    case dot t: {
        return "val:"u8 + t.name();
    }
    default: {
        var t = v;
        return "none";
    }}
}

internal static @string classify(any x) {
    switch (x.type()) {
    case nint _:
    case int32 _:
    case int64 _: {
        var v = x;
        return fmt.Sprintf("integer %v"u8, v);
    }
    case @string _:
    case bool _: {
        var v = x;
        return fmt.Sprintf("text-or-flag %v"u8, v);
    }}
    return "unknown";
}

internal static @string ptrKind(any x) {
    switch (x.type()) {
    case ж<circle> _:
    case ж<square> _: {
        var t = x;
        _ = t;
        return "shape-ptr";
    }
    case ж<dot> t: {
        return "dot-ptr";
    }}
    return "other";
}

internal static @string kind(shape v) {
    switch (v.type()) {
    case null:
    case dot _: {
        var t = v;
        _ = t;
        return "nil-or-dot";
    }
    case circle t: {
        return "circle-val";
    }
    default: {
        var t = v;
        return "boxed";
    }}
}

internal static @string tag(any x) {
    switch (x.type()) {
    case nint _:
    case int32 _:
    case @string _: {
        return "common";
    }
    case float64: {
        return "float";
    }}

    return "rare";
}

internal static void Main() {
    fmt.Println(describe(new circle(1)));
    fmt.Println(describe(new square(2)));
    fmt.Println(describe(new dot(nil)));
    fmt.Println(classify((nint)1));
    fmt.Println(classify((int64)2));
    fmt.Println(classify("s"));
    fmt.Println(classify(false));
    fmt.Println(classify(3.5D));
    fmt.Println(ptrKind(Ꮡ(new circle(1))));
    fmt.Println(ptrKind(Ꮡ(new square(2))));
    fmt.Println(ptrKind(Ꮡ(new dot(nil))));
    fmt.Println(ptrKind((nint)7));
    fmt.Println(kind(default!));
    fmt.Println(kind(new dot(nil)));
    fmt.Println(kind(new circle(3)));
    fmt.Println(kind(new square(4)));
    fmt.Println(tag((nint)7));
    fmt.Println(tag("x"));
    fmt.Println(tag(1.5D));
    fmt.Println(tag(true));
}

} // end main_package

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string circleˢ = "circle"u8;

internal static @string name(this circle c) {
    return circleˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string squareˢ = "square"u8;

internal static @string name(this square s) {
    return squareˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dotˢ = "dot"u8;

internal static @string name(this dot d) {
    return dotˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noneˢ = "none"u8;

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
        return noneˢ;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unknownˢ = "unknown"u8;

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
    return unknownˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string shapePtrˢ = "shape-ptr"u8;
private static readonly @string dotPtrˢ = "dot-ptr"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string ptrKind(any x) {
    switch (x.type()) {
    case ж<circle> _:
    case ж<square> _: {
        var t = x;
        _ = t;
        return shapePtrˢ;
    }
    case ж<dot> t: {
        return dotPtrˢ;
    }}
    return otherˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilOrDotˢ = "nil-or-dot"u8;
private static readonly @string circleValˢ = "circle-val"u8;
private static readonly @string boxedˢ = "boxed"u8;

internal static @string kind(shape v) {
    switch (v.type()) {
    case null:
    case dot _: {
        var t = v;
        _ = t;
        return nilOrDotˢ;
    }
    case circle t: {
        return circleValˢ;
    }
    default: {
        var t = v;
        return boxedˢ;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string commonˢ = "common"u8;
private static readonly @string floatˢ = "float"u8;
private static readonly @string rareˢ = "rare"u8;

internal static @string tag(any x) {
    switch (x.type()) {
    case nint _:
    case int32 _:
    case @string _: {
        return commonˢ;
    }
    case float64: {
        return floatˢ;
    }}

    return rareˢ;
}

internal static void Main() {
    fmt.Println(describe(new circle(1)));
    fmt.Println(describe(new square(2)));
    fmt.Println(describe(new dot(nil)));
    fmt.Println(classify((nint)(1)));
    fmt.Println(classify((int64)2));
    fmt.Println(classify((@string)"s"u8));
    fmt.Println(classify(false));
    fmt.Println(classify(3.5D));
    fmt.Println(ptrKind(Ꮡ(new circle(1))));
    fmt.Println(ptrKind(Ꮡ(new square(2))));
    fmt.Println(ptrKind(Ꮡ(new dot(nil))));
    fmt.Println(ptrKind((nint)(7)));
    fmt.Println(kind(default!));
    fmt.Println(kind(new dot(nil)));
    fmt.Println(kind(new circle(3)));
    fmt.Println(kind(new square(4)));
    fmt.Println(tag((nint)(7)));
    fmt.Println(tag((@string)"x"u8));
    fmt.Println(tag(1.5D));
    fmt.Println(tag(true));
}

} // end main_package

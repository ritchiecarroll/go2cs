namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
    void grow(nint n);
}

partial interface sizer {
    nint size();
}

partial struct circle {
    internal nint r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string circleˢ = "circle"u8;

internal static @string name(this ref circle c) {
    return circleˢ;
}

internal static void grow(this ref circle c, nint n) {
    c.r += n;
}

internal static nint size(this ref circle c) {
    return c.r;
}

partial struct dot {
    internal nint tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dotˢ = "dot"u8;

internal static @string name(this dot d) {
    return dotˢ;
}

internal static void grow(this dot d, nint n) {
    d.tag += n;
}

partial struct errMark {
    internal nint code;
}

internal static @string Error(this ref errMark e) {
    return fmt.Sprintf("errMark(%d)"u8, e.code);
}

internal static @string describe(any x) {
    switch (x.type()) {
    case {} Δt when Δt._<shape>(out var t): {
        return "shape:"u8 + t.name();
    }
    case {} Δt when Δt._<error>(out var t): {
        return "error:"u8 + t.Error();
    }
    default: {
        var t = x;
        return fmt.Sprintf("other:%T"u8, t);
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noneˢ = "none"u8;

internal static @string firstConcrete(any x) {
    switch (x.type()) {
    case ж<circle> t: {
        return fmt.Sprintf("concrete r=%d"u8, (~t).r);
    }
    case {} Δt when Δt._<shape>(out var t): {
        return "iface "u8 + t.name();
    }
    default: {
        var t = x;
        return noneˢ;
    }}
}

internal static @string firstInterface(any x) {
    switch (x.type()) {
    case {} Δt when Δt._<shape>(out var t): {
        return "iface "u8 + t.name();
    }
    case ж<circle> t: {
        return fmt.Sprintf("concrete r=%d"u8, (~t).r);
    }
    default: {
        var t = x;
        return noneˢ;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string multiShapeOrErrorˢ = "multi shape-or-error"u8;
private static readonly @string nilˢ = "nil"u8;

internal static @string multi(any x) {
    switch (x.type()) {
    case {} ᴛ0 when ᴛ0._<shape>(out var _):
    case {} ᴛ1 when ᴛ1._<error>(out var _): {
        var t = x;
        return multiShapeOrErrorˢ;
    }
    case null: {
        return nilˢ;
    }
    default: {
        var t = x;
        return fmt.Sprintf("single? %v"u8, t);
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notASizerˢ = "not a sizer"u8;

internal static @string viaInterfaceTag(shape v) {
    switch (v.type()) {
    case {} Δt when Δt._<sizer>(out var t): {
        return fmt.Sprintf("sizer %d"u8, t.size());
    }
    default: {
        var t = v;
        return notASizerˢ;
    }}
}

internal static void Main() {
    var c = Ꮡ(new circle(r: 1));
    shape s = new circleжshape(c);
    sizer z = new circleжsizer(c);
    _ = z;
    any x = s;
    fmt.Println(describe(x));
    any raw = Ꮡ(new circle(r: 7));
    fmt.Println(describe(raw));
    fmt.Println(describe(new dot(tag: 3)));
    fmt.Println(describe((nint)(42)));
    error e = new errMarkжerror(Ꮡ(new errMark(code: 5)));
    any anyErr = e;
    fmt.Println(describe(anyErr));
    fmt.Println(describe(Ꮡ(new errMark(code: 6))));
    fmt.Println(firstConcrete(c.OrTypedNil()));
    fmt.Println(firstInterface(c.OrTypedNil()));
    fmt.Println(multi(c.OrTypedNil()));
    fmt.Println(multi(Ꮡ(new errMark(code: 8))));
    fmt.Println(multi(default!));
    fmt.Println(multi(1.5D));
    fmt.Println(viaInterfaceTag(new circleжshape(c)));
    fmt.Println(viaInterfaceTag(new dot(tag: 4)));
    switch (x.type()) {
    case {} Δt when Δt._<shape>(out var t): {
        t.grow(10);
        break;
    }}
    fmt.Println((~c).r);
}

} // end main_package

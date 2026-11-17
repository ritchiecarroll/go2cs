namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Map /*map[@string, any]*/;

partial struct point {
    internal nint x, y;
}

partial struct named {
    internal @string name;
}

} partial class mainᴛnamed {

internal static @string String(this named n) { return "named "u8 + n.name; }

} partial class main_package {

internal static @string classify(any x) {
    switch (x.type()) {
    case ж<point> v: {
        return fmt.Sprint("pointer ", v.x + v.y);
    }
    case Map _:
    case nint _: {
        var v = x;
        return fmt.Sprint("map or int ", v);
    }
    case {} Δv when Δv._<fmt.Stringer>(out var v): {
        return v.String();
    }
    case null: {
        return "nil";
    }}
    return "other";
}

internal static @string kind(any x) {
    switch (x.type()) {
    case point: {
        return "point value";
    }
    case slice<nint>: {
        return "int slice";
    }}

    return "unmatched";
}

internal static void main() {
    any data = new Map(new map<@string, any>{["a"] = (nint)1});

    var (mΔ1, okΔ1) = data._<Map>(ᐧ); if (okΔ1) {
        fmt.Println("comma-ok:", len(mΔ1), mΔ1["a"]);
    }

    var m = data._<Map>();
    fmt.Println("single:", m["a"]);

    any other = (nint)5;
    var (_, ok) = other._<Map>(ᐧ);
    fmt.Println("mismatch:", ok);

    any p = Ꮡ(new point(1, 2));
    var (pt, okΔ2) = p._<ж<point>>(ᐧ); if (okΔ2) {
        fmt.Println("pointer:", pt.x + pt.y);
    }

    foreach (var (_, x) in range(slice<any>([Ꮡ(new point(3, 4)), new Map(new map<@string, any>{["b"] = (nint)2}), (nint)7, new named("n"), default, 2.5D]))) {
        fmt.Println("switch:", classify(x));
    }

    foreach (var (_, x) in range(slice<any>([new point(1, 1), slice<nint>([1]), (@string)"s"]))) {
        fmt.Println("kind:", kind(x));
    }
}

} // end main_package

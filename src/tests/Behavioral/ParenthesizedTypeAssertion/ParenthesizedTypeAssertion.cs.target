namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Map /*map[@string, any]*/;

partial struct point {
    internal nint x, y;
}

internal static void Main() {
    any data = new Map(new map<@string, any>{["a"] = (nint)1});

    {
        var (mΔ1, okΔ1) = data._<Map>(ᐧ); if (okΔ1) {
            fmt.Println("comma-ok:", len(mΔ1), mΔ1["a"]);
        }
    }

    var m = data._<Map>();
    fmt.Println("single:", m["a"]);

    any other = (nint)5;
    var (_, ok) = other._<Map>(ᐧ);
    fmt.Println("mismatch:", ok);

    any p = Ꮡ(new point(1, 2));
    {
        var (pt, okΔ2) = p._<ж<point>>(ᐧ); if (okΔ2) {
            fmt.Println("pointer:", (~pt).x + (~pt).y);
        }
    }
}

} // end main_package

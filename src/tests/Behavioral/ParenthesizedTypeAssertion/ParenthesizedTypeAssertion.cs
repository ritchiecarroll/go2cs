namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Map /*map[@string, any]*/;

partial struct point {
    internal nint x, y;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object commaOkˢ = (@string)"comma-ok:"u8;
private static readonly object singleˢ = (@string)"single:"u8;
private static readonly object mismatchˢ = (@string)"mismatch:"u8;
private static readonly object pointerˢ = (@string)"pointer:"u8;

internal static void Main() {
    any data = new Map(new map<@string, any>{["a"u8] = (nint)(1)});
    {
        var (mΔ1, okΔ1) = data._<Map>(ᐧ); if (okΔ1) {
            fmt.Println(commaOkˢ, len(mΔ1), mΔ1["a"u8]);
        }
    }
    var m = data._<Map>();
    fmt.Println(singleˢ, m["a"u8]);
    any other = (nint)(5);
    var (_, ok) = other._<Map>(ᐧ);
    fmt.Println(mismatchˢ, ok);
    any p = Ꮡ(new point(1, 2));
    {
        var (pt, okΔ2) = p._<ж<point>>(ᐧ); if (okΔ2) {
            fmt.Println(pointerˢ, (~pt).x + (~pt).y);
        }
    }
}

} // end main_package

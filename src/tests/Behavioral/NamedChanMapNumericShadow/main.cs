namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Pipe /*chan nint*/;

} partial class mainᴛPipe {

public static nint Capacity(this Pipe p) { return -1; }
public static nint Length(this Pipe p) { return -2; }
public static @string IsClosed(this Pipe p) { return "never closed"; }
public static @string IsUnbuffered(this Pipe p) { return "who knows"; }
public static nint Receive(this Pipe p) { return 77; }
public static @string Receiving(this Pipe p) { return "receiving"; }
public static @string Sending(this Pipe p, nint v) { return fmt.Sprint("sending ", v); }
public static bool Received(this Pipe p, nint v) { return v > 0; }
public static Pipe Make(this Pipe p, nint n) { return new Pipe(n); }
public static nint GetEnumerator(this Pipe p) { return 3; }

} partial class main_package {

partial struct Counts /*map[@string, nint]*/;

} partial class mainᴛCounts {

public static nint Length(this Counts c) { return 100 + len(c); }
public static @string IsNil(this Counts c) { return "not nil"; }
public static nint Count(this Counts c) { return -len(c); }
public static nint Make(this Counts c) { return 1; }
public static @string GetEnumerator(this Counts c) { return "map enumerator"; }

} partial class main_package {

partial struct Level /*num:nint*/;

public static nint CompareTo(this Level l, Level o) { return 42; }

partial class IsNull /*ж<nint>*/;
partial struct Capacity /*chan @string*/;
partial struct Count /*map[nint, bool]*/;

internal static void main() {
    var p = new Pipe(2);
    fmt.Println("Capacity/Length:", p.Capacity(), p.Length());
    fmt.Println("IsClosed/IsUnbuffered:", p.IsClosed(), p.IsUnbuffered());
    fmt.Println("Receive/Receiving/Sending/Received:", p.Receive(), p.Receiving(), p.Sending(4), p.Received(3));
    var m = p.Make(5);
    fmt.Println("Make:", cap(m), "GetEnumerator:", p.GetEnumerator());

    p.ᐸꟷ(1);
    var pᴛ1 = p;
    switch (trySelect(ᐸꟷ<nint>(pᴛ1, ꓸꓸꓸ))) {
    case 0 when pᴛ1.ꟷᐳ(out var v): {
        fmt.Println("selected receive:", v);
        break;
    }
    default: {
        fmt.Println("no receive");
        break;
    }}
    var pᴛ2 = p;
    switch (trySelect(ᐸꟷ<nint>(pᴛ2, ꓸꓸꓸ))) {
    case 0 when pᴛ2.ꟷᐳ(out var v, out var ok): {
        fmt.Println("selected receive ok:", v, ok);
        break;
    }
    default: {
        fmt.Println("no receive ok");
        break;
    }}
    var pᴛ3 = p.ᐸꟷ(9, ꓸꓸꓸ);
    switch (trySelect(pᴛ3)) {
    case 0: {
        fmt.Println("selected send");
        break;
    }
    default: {
        fmt.Println("no send");
        break;
    }}
    fmt.Println("builtin len/cap:", len(p), cap(p), ᐸꟷ<nint>(p));

    var c = new Counts(new map<@string, nint>{["a"] = 1, ["b"] = 2});
    fmt.Println("Counts:", c.Length(), c.IsNil(), c.Count(), c.Make(), c.GetEnumerator(), len(c));

    Level a = 1;
    Level b = 2;
    fmt.Println("CompareTo:", a.CompareTo(b), a < b);

    ref var x = ref heap<nint>(5, out var Ꮡx);
    IsNull n = Ꮡx;
    fmt.Println("type IsNull:", n.Value, n != nil);
    var cc = new Capacity(1);
    cc.ᐸꟷ("c");
    fmt.Println("type Capacity:", len(cc), ᐸꟷ<@string>(cc));
    var k = new Count(new map<nint, bool>{[1] = true});
    fmt.Println("type Count:", len(k), k[1]);
}

} // end main_package

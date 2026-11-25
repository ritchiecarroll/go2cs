namespace go;

using fmt = fmt_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct T {
    internal nint x;
}

partial struct P {
    internal slice<nint> q;
}

internal static nint g;
internal static slice<@string> log;
internal static readonly ж<array<nint>> Ꮡarr = new StaticBox<array<nint>>(new array<nint>(4));
internal static ref array<nint> arr => ref ((StaticBox<array<nint>>)Ꮡarr).ValueSlot;
internal static readonly ж<ж<P>> Ꮡp = new StaticBox<ж<P>>(default(ж<P>));
internal static ref ж<P> p => ref ((StaticBox<ж<P>>)Ꮡp).ValueSlot;

internal static nint f(@string tag) {
    log = append(log, tag);
    g++;
    return g;
}

internal static void reset() {
    (g, log, var arrᴛ1, p) = (0, default, new nint[]{}.array(4), Ꮡ(new P(q: new slice<nint>(4))));
    arr.Assign(arrᴛ1);
}

internal static void show(@string name, params ꓸꓸꓸany valuesʗp) {
    var values = valuesʗp.slice();

    fmt.Println(name, values, strings.Join(log, ","));
    reset();
}

internal static void h3(nint a, ref T t, nint b) { fmt.Println(a, t.x, b); }

internal static void he(nint a, ref nint e, nint b) {
    e = 100;
    fmt.Println(a, b);
}

internal static void hm(nint a, nint b) { fmt.Println(a, b); }

internal static void main() {
    reset();

    var evalᴛ1 = f("lit");
    var evalᴛ2 = f("last");

    var ᴛ1 = new T(x: evalᴛ1);
    h3(g, ref ᴛ1, evalᴛ2);
    show("literal");

    var evalᴛ3 = f("idx");
    var evalᴛ4 = f("last");
    he(g, ref arr[evalᴛ3 % 4], evalᴛ4);
    show("array element", arr);

    var evalᴛ5 = new map<@string, nint>{["k"] = f("m1")};
    var evalᴛ6 = f("m2");
    fmt.Println(g, evalᴛ5["k"], evalᴛ6);
    show("map literal");

    var evalᴛ7 = f("pq");
    var evalᴛ8 = f("last");
    he(g, ref (Ꮡ(p.q, evalᴛ7 % 4)).DerefOrNull(), evalᴛ8);
    show("slice element", p.q);

    var evalᴛ9 = f("a");

    var ᴛ2 = new T(x: g);
    h3(0, ref ᴛ2, evalᴛ9);
    show("literal read");

    var evalᴛ10 = new map<@string, nint>{["k"] = f("m")};
    hm(g, evalᴛ10["k"]);
    show("map unit");

    var evalᴛ11 = new map<@string, nint>{["k"] = g};
    var evalᴛ12 = f("r");
    fmt.Println(g, evalᴛ11["k"], evalᴛ12);
    show("map read");

    var evalᴛ13 = f("s");
    fmt.Println(slice<nint>([g])[0], evalᴛ13);
    show("slice literal read");

    var t = Ꮡ(new T(x: f("only")));
    show("control literal", t.x);

    nint n = new map<@string, nint>{["k"] = f("only")}["k"];
    show("control map", n);
}

} // end main_package

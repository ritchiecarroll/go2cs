namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct flagSet /*[2]uint8*/;

/*[2]*/ partial class Features /*ж<flagSet>*/;

public partial struct point {
    internal nint x, y;
}

partial class PP /*ж<point>*/;

partial struct holder {
    internal Features f;
}

internal static uint8 first(ref flagSet o) { return o[0]; }

internal static void main() {
    var p = new PP(Ꮡ(new point(x: 4)));
    p.y = p.x + 1;
    fmt.Println(p.x, p.y);

    var g = new Features(Ꮡ(new flagSet(new uint8[]{7, 8}.array())));
    g.Value[1] = (uint8)(g.Value[0] + 1);
    fmt.Println(g.Value[0], g.Value[1], 2);

    nint sum = 0;
    foreach (var (i, b) in range(g.Value)) {
        sum += i + (nint)b;
    }
    fmt.Println(sum);

    var h = new holder(f: g);
    fmt.Println(first(ref (g).DerefOrNull()), first(ref (h.f).DerefOrNull()), h.f.Value[1]);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static int8 h = 101;

internal static @string genericKey<M>(M m)
    where M : /* ~map[int8]string */ IMap<int8, @string>, ISupportMake<M>, new()
{
    return m[(int8)((h >> (int)(1)))];
}

internal static void Main() {
    uint8 u = 200;
    uint8 d = 250;
    int16 w = 30000;

    var mk = new map<uint8, @string>{[144] = "wrapped"};
    fmt.Println("map key:", mk[(uint8)(u + u)]);

    nint n = 0;
    foreach (var _ᴛ1 in range<uint8>((uint8)(d + 10))) {
        n++;
    }
    fmt.Println("range bound:", n);

    var ch = new channel<uint8>(1);
    ch.ᐸꟷ((uint8)(u + u));
    fmt.Println("chan send:", ᐸꟷ(ch));

    var m = new map<@string, int16>{["k"] = (int16)(w + w)};
    fmt.Println("map value:", m["k"]);

    var arr = new array<uint8>(2){[1] = (uint8)(u * 2)};
    fmt.Println("keyed array element:", arr);

    uint8 x = default!;
    x = (uint8)(u + u);
    fmt.Println("paren assign:", x);

    var mk2 = new map<uint8, @string>{[(uint8)(u + u)] = "wrapped key"};
    fmt.Println("map-literal key:", mk2[144]);

    fmt.Println("type-parameter map key:", genericKey(new map<int8, @string>{[50] = "ok"}));
}

} // end main_package

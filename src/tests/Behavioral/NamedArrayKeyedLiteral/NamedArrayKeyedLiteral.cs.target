namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct args /*[4]uint64*/;

internal static args keyed(uint64 a, uint64 b) {
    return new args(new array<uint64>(4){[1] = a, [3] = b});
}

internal static args positional(uint64 w, uint64 x, uint64 y, uint64 z) {
    return new args(new uint64[]{w, x, y, z}.array());
}

internal static void Main() {
    var k = keyed(11, 22);
    fmt.Println(k[0], k[1], k[2], k[3]);
    var p = positional(5, 6, 7, 8);
    fmt.Println(p[0], p[1], p[2], p[3]);
    var one = new args(new array<uint64>(4){[2] = 99});
    fmt.Println(one[0], one[1], one[2], one[3]);
}

} // end main_package

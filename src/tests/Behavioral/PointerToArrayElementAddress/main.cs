namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct row /*[4]uint32*/;

partial struct grid /*[3]row*/;

internal static void populate(ж<row> Ꮡt, uint32 @base) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint i = 0; i < 4; i++) {
        t[i] = @base + (uint32)i;
    }
}

internal static void Main() {
    var g = @new<grid>();
    for (nint j = 0; j < 3; j++) {
        populate(g.at<row>(j), (uint32)(j * 10));
    }
    for (nint j = 0; j < 3; j++) {
        fmt.Println(g.Value[j][0], g.Value[j][1], g.Value[j][2], g.Value[j][3]);
    }
    var first = g.at<row>(0);
    first.Value[0] = 99;
    fmt.Println(g.Value[0][0]);
}

} // end main_package

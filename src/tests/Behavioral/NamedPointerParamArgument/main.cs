namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

public partial struct flagSet /*[2]uint8*/;

/*[2]*/ partial class Features /*ж<flagSet>*/;

} partial class mainᴛflagSet {

internal static bool hasSetP(this ref flagSet s, ж<flagSet> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return (uint8)(s.Value[0] & o[0]) == o[0];
}

} partial class main_package {

partial struct CPU {
    internal flagSet fs;
}

} partial class mainᴛCPU {

public static bool HasAll(this ref CPU c, Features f) { return c.fs.hasSetP(f); }

} partial class main_package {

internal static @string describe(Features f) {
    if (f == nil) {
        return "nil";
    }

    return fmt.Sprint((f.Value)[0], (f.Value)[1]);
}

internal static void reset(Features f) { f.Value = new flagSet(new uint8[]{9, 9}.array()); }

internal static void main() {
    var c = Ꮡ(new CPU(fs: new flagSet(new uint8[]{3, 0}.array())));
    fmt.Println(c.HasAll(Ꮡ(new flagSet(new uint8[]{1, 0}.array()))), c.HasAll(Ꮡ(new flagSet(new uint8[]{4, 0}.array()))));
    fmt.Println(describe(Ꮡ(new flagSet(new uint8[]{5, 6}.array()))), describe(nil));

    var fs = Ꮡ(new flagSet(new uint8[]{1, 2}.array()));
    reset(fs);
    fmt.Println(fs.Value[0], fs.Value[1]);
}

} // end main_package

namespace go.ForeignValueImplementSuppression;

using static hue_package;

partial class hue_package {

partial interface Tint {
    (uint32 l, uint32 a) Shade();
}

partial struct Shade {
    public uint8 L, A;
}

} partial class hueᴛShade {

public static (uint32, uint32) Shade(this Shade s) {
    return ((uint32)s.L * 0x101, (uint32)s.A * 0x101);
}

} partial class hue_package {

partial struct Gray {
    public uint8 Y;
}

} partial class hueᴛGray {

public static (uint32, uint32) Shade(this Gray g) {
    var y = (uint32)g.Y * 0x101;
    return (y, 0xffff);
}

} partial class hue_package {

public delegate (uint32 l, uint32 a) Tinter();

} partial class hueᴛTinter {

public static (uint32, uint32) Shade(this Tinter f) { return f(); }

} partial class hue_package {

public static Tint Default = new Shade(L: 1, A: 2);
public static Tint DefaultGray = new Gray(Y: 4);
public static Tint DefaultFunc = new TinterᴠTint(new Tinter(() => ((uint32)(8), (uint32)(9))));

public static Tint Describe(Shade s) { return s; }

} // end hue_package

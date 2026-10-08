namespace go.ForeignValueImplementSuppression;

partial class hue_package {

partial interface Tint {
    (uint32 l, uint32 a) Shade();
}

partial struct ΔShade {
    public uint8 L, A;
}

public static (uint32, uint32) Shade(this ΔShade s) {
    return ((uint32)s.L * 0x101, (uint32)s.A * 0x101);
}

partial struct Gray {
    public uint8 Y;
}

public static (uint32, uint32) Shade(this Gray g) {
    var y = (uint32)g.Y * 0x101;
    return (y, 0xffff);
}

public delegate (uint32 l, uint32 a) Tinter();

public static (uint32, uint32) Shade(this Tinter f) {
    return f();
}

public static Tint Default = new ΔShade(L: 1, A: 2);
public static Tint DefaultGray = new Gray(Y: 4);
public static Tint DefaultFunc = new TinterᴠTint(new Tinter(() => ((uint32)(8), (uint32)(9))));

public static Tint Describe(ΔShade s) {
    return s;
}

} // end hue_package

namespace go.ValueAdapterDynamicType;

partial class colorlike_package {

partial interface Color {
    (uint32 r, uint32 g, uint32 b, uint32 a) RGBA();
}

partial struct NRGBA {
    public uint8 R, G, B, A;
}

public static (uint32, uint32, uint32, uint32) RGBA(this NRGBA c) {
    return ((uint32)c.R * 0x101, (uint32)c.G * 0x101, (uint32)c.B * 0x101, (uint32)c.A * 0x101);
}

partial struct Gray {
    public uint8 Y;
}

public static (uint32, uint32, uint32, uint32) RGBA(this Gray c) {
    var y = (uint32)c.Y * 0x101;
    return (y, y, y, 0xffff);
}

} // end colorlike_package

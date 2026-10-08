namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Tag /*num:uint8*/;

internal static UntypedInt classConstructed => 0x20;

internal static UntypedInt classContext => 0x80;

public static Tag Constructed(this Tag t) {
    return (Tag)(t | (uint8)classConstructed);
}

public static Tag Context(this Tag t) {
    return (Tag)(t | (uint8)classContext);
}

partial struct word /*num:uint64*/;

internal static UntypedInt tagBits => 19;

internal static word low(this word w) {
    return (word)(w & (uint64)(((1 << (int)(tagBits)) - 1)));
}

internal static void Main() {
    Tag t = 0x10;
    fmt.Println((uint8)t.Constructed());
    fmt.Println((uint8)t.Context());
    word w = 0xABCDEF012345UL;
    fmt.Println((uint64)w.low());
    fmt.Println((uint8)mask((Tag)(~((Tag)((Tag)0)))));
    fmt.Println((uint64)maskFor(4), (uint64)maskFor(19));
    word wv = 0xF0;
    fmt.Println((uint64)((word)((word)(wv & 0x30) | ((word)0x01))), (uint64)((word)(wv ^ ((word)0xFF))));
    var c = blend(new channels(R: 60000, G: 40000, B: 20000, A: 65535), new channels(R: 9000, G: 1, B: 2, A: 3), 3, 4);
    fmt.Println(c.R, c.G, c.B, c.A);
}

internal static Tag mask(Tag t) {
    return (Tag)(t & 0xFF);
}

internal static word maskFor(nuint s) {
    var m = (((word)1) << (int)(s)) - 1;
    return m;
}

partial struct channels {
    public uint16 R, G, B, A;
}

internal static channels blend(channels d, channels s, uint32 a, uint32 m) {
    return new channels(
        R: (uint16)((uint16)((uint32)d.R * a / m) + s.R),
        G: (uint16)((uint16)((uint32)d.G * a / m) + s.G),
        B: (uint16)((uint16)((uint32)d.B * a / m) + s.B),
        A: (uint16)((uint16)((uint32)d.A * a / m) + s.A)
    );
}

} // end main_package

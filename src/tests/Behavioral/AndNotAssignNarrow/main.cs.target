namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt writing => 4;

partial struct hmap {
    internal uint8 flags;
}

internal static void Main() {
    var h = Ꮡ(new hmap(flags: 7));
    h.Value.flags &= unchecked((uint8)~(uint8)(writing));
    h.Value.flags |= (uint8)(8);
    fmt.Println((~h).flags);
    uint16 u = 0xFFFF;
    u &= unchecked((uint16)~(uint16)(0x0F0));
    fmt.Println(u);
    uint32 x = 0xFFFFFFFFU;
    x &= unchecked((uint32)~(uint32)(0xFF));
    fmt.Println(x);
    nint i = 0xFF;
    i &= ~(nint)(0x0F);
    fmt.Println(i);
    nint length = 5;
    fmt.Println((int32)(((uint16)(~(uint16)length))));
    fmt.Println((uint64)(((uint16)(~(uint16)length))));
    uint8 b8 = 5;
    fmt.Println((int32)(((uint8)(~b8))), (uint64)(((uint8)(~b8))));
    uint32 u32 = 5;
    int16 i16 = 5;
    fmt.Println(~u32, (int32)(~i16), (int16)(~i16));
    uint16 u16 = 5;
    u16 = (uint16)(((uint16)(~u16)));
    fmt.Println(u16);
    fmt.Println((nint)(unchecked((uint16)(~(uint16)0))));
    fmt.Println((nint)(unchecked((uint8)(~(uint8)0))));
    const uint16 seed = 5;
    fmt.Println((uint64)(unchecked((uint16)(~seed))));
}

} // end main_package

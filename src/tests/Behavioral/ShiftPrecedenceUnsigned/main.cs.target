namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 lowestSetBit(uint32 x) {
    return (uint32)(x & ((uint32)0 - x));
}

internal partial struct main_bitfield /*dyn*/ {
    internal uint64 cache;
}

internal static void Main() {
    fmt.Println(lowestSetBit(12));
    fmt.Println(lowestSetBit(40));
    uint64 z = 6;
    fmt.Println((uint64)(z & ((uint64)0 - z)));
    uint64 x = 0x100;
    fmt.Println((x >> (int)(4)) + x);
    fmt.Println((nint)((1 << (int)(15)) - 1));
    fmt.Println(x - ((uint64)1 << (int)(4)));
    nuint s = 3;
    uint64 y = 1;
    y.LshAssign(s);
    fmt.Println(y);
    y.RshAssign(s);
    fmt.Println(y);
    var bf = new main_bitfield(cache: 0xFF);
    nuint n = 4;
    bf.cache.RshAssign(n);
    fmt.Println(bf.cache);
    bf.cache.LshAssign(n);
    fmt.Println(bf.cache);
    uint64 hi = 0xFFFFFFFFFFFFFFFFUL;
    fmt.Println((uint64)(hi & ~(((uint64)1 << (int)(63)))));
    uint32 u = 0xFFFFFFFFU;
    fmt.Println((uint32)(u & ~(((uint32)1 << (int)(31)))));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint64 copysign(uint64 f, uint64 sign) {
    const uint64 signBit = /* 1 << 63 */ 9223372036854775808;
    return (uint64)(f & ~signBit) | (uint64)(sign & signBit);
}

internal static uint64 clearLow(uint64 x) {
    return x & ~(uint64)1;
}

internal static uint32 clearLow32(uint32 u) {
    return (uint32)(u & ~1);
}

internal static void Main() {
    fmt.Println(copysign(0xFF, 0x8000000000000000UL));
    fmt.Println(copysign(0x8000000000000042UL, 0));
    fmt.Println(clearLow(0xFFFFFFFFFFFFFFFFUL));
    fmt.Println(clearLow(1));
    fmt.Println(clearLow32(0xFFFFFFFFU));
}

} // end main_package

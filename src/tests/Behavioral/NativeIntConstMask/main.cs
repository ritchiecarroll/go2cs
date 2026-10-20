namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uintptr shift => 3;

internal static uintptr blockSize => 16;

internal static uintptr low(uintptr i) {
    return (uintptr)(i & (uintptr)((1 << (int)(shift)) - 1));
}

internal static uintptr align(uintptr i) {
    return (uintptr)(i & ~(uintptr)(blockSize - 1));
}

internal static uintptr maskAddr(uintptr i) {
    return (uintptr)(i & (uintptr)(nuint)0x00ffffffffffUL);
}

internal static uintptr alignSmall(uintptr i) {
    return (uintptr)(i & ~(uintptr)15);
}

internal static UntypedInt ptrWords => 8;

internal static uintptr fieldAddr(uintptr @base) {
    return @base + (uintptr)(4 * ptrWords);
}

internal static void Main() {
    fmt.Println((uint64)low(0b1011));
    fmt.Println((uint64)align(0b1011));
    fmt.Println((uint64)align(0b11011));
    fmt.Println((uint64)fieldAddr(0x1000));
    uintptr addr = 1;
    addr <<= (int)(47);
    addr |= (uintptr)(0xABCD);
    fmt.Println((uint64)maskAddr(addr));
    fmt.Println((uint64)alignSmall(27));
}

} // end main_package

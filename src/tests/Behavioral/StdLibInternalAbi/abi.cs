namespace go;

using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct RegArgs {
    public array<uintptr> Ints = new(IntArgRegs);
    public array<uint64> Floats = new(FloatArgRegs);
    public array<@unsafe.Pointer> Ptrs = new(IntArgRegs);
    public IntArgRegBitmap ReturnIsPtr;
}

[GoRecv] public static void Dump(this ref RegArgs r) {
    print((@string)"Ints:"u8);
    foreach (var (_, x) in r.Ints.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
    print((@string)"Floats:"u8);
    foreach (var (_, x) in r.Floats.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
    print((@string)"Ptrs:"u8);
    foreach (var (_, x) in r.Ptrs.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
}

[GoRecv] public static @unsafe.Pointer IntRegArgAddr(this ref RegArgs r, nint reg, uintptr argSize) {
    if (argSize > PtrSize || argSize == 0 || (uintptr)(argSize & (argSize - 1)) != 0) {
        throw panic("invalid argSize");
    }
    var offset = (uintptr)0;
    if (BigEndian) {
        offset = (uintptr)PtrSize - argSize;
    }
    return (@unsafe.Pointer)((uintptr)@unsafe.Pointer.FromBox(Ꮡ(r.Ints, reg)) + offset);
}

[GoType("[2]uint8")] /* [(IntArgRegs + 7) / 8]uint8 */
partial struct IntArgRegBitmap;

[GoRecv] public static void Set(this ref IntArgRegBitmap b, nint i) {
    b.Value[i / 8] |= (uint8)((uint8)((uint8)1 << (int)((i % 8))));
}

[GoRecv] public static bool Get(this ref IntArgRegBitmap b, nint i) {
    return (uint8)(b.Value[i / 8] & ((uint8)((uint8)1 << (int)((i % 8))))) != 0;
}

} // end main_package

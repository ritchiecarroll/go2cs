namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myLen /*num:uintptr*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object uintptrBlankKeyˢ = (@string)"uintptr blank-key iterations:"u8;
private static readonly object uintptrOffsetsˢ = (@string)"uintptr offsets:"u8;
private static readonly object uint8Sumˢ = (@string)"uint8 sum:"u8;
private static readonly object int64Sumˢ = (@string)"int64 sum:"u8;
private static readonly object uint64Sumˢ = (@string)"uint64 sum:"u8;
private static readonly object runeSumˢ = (@string)"rune sum:"u8;
private static readonly object namedUintptrSumˢ = (@string)"named uintptr sum:"u8;
private static readonly object nonPositiveIterationsˢ = (@string)"non-positive iterations:"u8;
private static readonly object intAndUntypedSumˢ = (@string)"int and untyped sum:"u8;

internal static void Main() {
    uintptr n = 4;
    nint count = 0;
    foreach (var _ᴛ1 in range<uintptr>(n)) {
        count++;
    }
    fmt.Println(uintptrBlankKeyˢ, count);
    uintptr stride = 16;
    uintptr offset = default!;
    var offsets = new uintptr[]{}.slice();
    foreach (var _ᴛ2 in range<uintptr>(n)) {
        offsets = append(offsets, offset);
        offset += stride;
    }
    fmt.Println(uintptrOffsetsˢ, offsets);
    uint8 b = 5;
    nint sum8 = 0;
    foreach (var i in range<uint8>(b)) {
        sum8 += (nint)i;
    }
    fmt.Println(uint8Sumˢ, sum8);
    int64 big = 6;
    var sum64 = (int64)0;
    foreach (var i in range<int64>(big)) {
        sum64 += i;
    }
    fmt.Println(int64Sumˢ, sum64);
    uint64 u = 3;
    uint64 sumU = default!;
    foreach (var i in range<uint64>(u)) {
        sumU += i;
    }
    fmt.Println(uint64Sumˢ, sumU);
    rune r = 4;
    nint sumR = 0;
    foreach (var i in range<rune>(r)) {
        sumR += (nint)i;
    }
    fmt.Println(runeSumˢ, sumR);
    nint named = 0;
    foreach (var i in range<myLen>(((myLen)3))) {
        named += (nint)(uintptr)i;
    }
    fmt.Println(namedUintptrSumˢ, named);
    nint zero = 0;
    foreach (var _ᴛ3 in range<int64>((int64)0)) {
        zero++;
    }
    foreach (var _ᴛ4 in range<int64>((int64)(-9))) {
        zero++;
    }
    foreach (var _ᴛ5 in range<uintptr>((uintptr)0)) {
        zero++;
    }
    fmt.Println(nonPositiveIterationsˢ, zero);
    nint plain = 0;
    foreach (var i in range(3)) {
        plain += i;
    }
    nint size = 4;
    foreach (var i in range(size)) {
        plain += i;
    }
    fmt.Println(intAndUntypedSumˢ, plain);
}

} // end main_package

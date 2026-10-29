namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myLen /*num:uintptr*/;

internal static void Main() {
    uintptr n = 4;
    nint count = 0;
    foreach (var _ᴛ1 in range<uintptr>(n)) {
        count++;
    }
    fmt.Println("uintptr blank-key iterations:", count);

    uintptr stride = 16;
    uintptr offset = default;
    var offsets = slice<uintptr>([]);
    foreach (var _ᴛ2 in range<uintptr>(n)) {
        offsets = append(offsets, offset);
        offset += stride;
    }
    fmt.Println("uintptr offsets:", offsets);

    uint8 b = 5;
    nint sum8 = 0;
    foreach (var i in range<uint8>(b)) {
        sum8 += (nint)i;
    }
    fmt.Println("uint8 sum:", sum8);

    int64 big = 6;
    var sum64 = (int64)0;
    foreach (var i in range<int64>(big)) {
        sum64 += i;
    }
    fmt.Println("int64 sum:", sum64);

    uint64 u = 3;
    uint64 sumU = default;
    foreach (var i in range<uint64>(u)) {
        sumU += i;
    }
    fmt.Println("uint64 sum:", sumU);

    rune r = 4;
    nint sumR = 0;
    foreach (var i in range<rune>(r)) {
        sumR += (nint)i;
    }
    fmt.Println("rune sum:", sumR);

    nint named = 0;
    foreach (var i in range<myLen>(((myLen)3))) {
        named += (nint)i;
    }
    fmt.Println("named uintptr sum:", named);

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
    fmt.Println("non-positive iterations:", zero);

    nint plain = 0;
    foreach (var i in range(3)) {
        plain += i;
    }
    nint size = 4;
    foreach (var i in range(size)) {
        plain += i;
    }
    fmt.Println("int and untyped sum:", plain);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Celsius /*num:float64*/;

partial struct Counter /*num:nint*/;

internal partial struct main_table /*dyn*/ {
    internal any v;
    internal bool want;
}

internal static void Main() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(@new<nint>());
    fmt.Println("A1 paren, fresh allocation, non-nil:", p != nil);

    var n = @new<nint>();
    n.Value = 42;
    @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(n);
    fmt.Println("A2 paren, existing pointer, non-nil:", q != nil);
    fmt.Println("A3 round-trips to the same value:", ~(ж<nint>)(uintptr)(q));

    var table = slice<main_table>([
        new(@unsafe.Pointer.FromPinnedBox(@new<nint>()), false),
        new(@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default), true)
    ]);
    foreach (var (i, row) in table) {
        fmt.Println("A4 row", i, "isNil:", AreEqual(row.v, ((any)@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default))), "want:", row.want);
    }

    @unsafe.Pointer r = @unsafe.Pointer.FromPinnedBox(@new<nint>());
    fmt.Println("B1 bare, fresh allocation, non-nil:", r != nil);

    @unsafe.Pointer s = @unsafe.Pointer.FromPinnedBox(n);
    fmt.Println("B2 bare, existing pointer, round-trips:", ~(ж<nint>)(uintptr)(s));

    var c1 = (Celsius)36.6D;
    var c2 = (Celsius)36.6D;
    fmt.Println("C1 named float both spellings:", c1 == c2, c1);

    Counter k1 = (Counter)7;
    Counter k2 = (Counter)7;
    fmt.Println("C2 named int both spellings:", k1 == k2, k1);

    var i1 = (int64)5;
    var i2 = (int64)5;
    fmt.Println("C3 builtin both spellings:", i1 == i2, i1);

    @string b1 = (@string)slice<byte>([104, 105]);
    @string b2 = (@string)slice<byte>([104, 105]);
    fmt.Println("C4 string both spellings:", b1 == b2, b1);

    ref var f = ref heap(new float64(), out var Ꮡf);
    f = 1.5D;
    var pf = Ꮡf;
    fmt.Println("C5 paren pointer conversion:", pf.Value);

    fmt.Println("C6 uintptr of paren pointer is non-zero:", (uintptr)n != 0);
}

} // end main_package

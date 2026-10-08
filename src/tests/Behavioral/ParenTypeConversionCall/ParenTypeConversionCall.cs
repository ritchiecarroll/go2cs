namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Celsius /*num:float64*/;

partial struct Counter /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object a1ParenFreshAllocationˢ = (@string)"A1 paren, fresh allocation, non-nil:"u8;
private static readonly object a2ParenExistingPointerˢ = (@string)"A2 paren, existing pointer, non-nil:"u8;
private static readonly object a3RoundTripsToTheSameˢ = (@string)"A3 round-trips to the same value:"u8;
private static readonly object a4Rowˢ = (@string)"A4 row"u8;
private static readonly object isNilˢ = (@string)"isNil:"u8;
private static readonly object wantˢ = (@string)"want:"u8;
private static readonly object b1BareFreshAllocationNonˢ = (@string)"B1 bare, fresh allocation, non-nil:"u8;
private static readonly object b2BareExistingPointerˢ = (@string)"B2 bare, existing pointer, round-trips:"u8;
private static readonly object c1NamedFloatBothˢ = (@string)"C1 named float both spellings:"u8;
private static readonly object c2NamedIntBothSpellingsˢ = (@string)"C2 named int both spellings:"u8;
private static readonly object c3BuiltinBothSpellingsˢ = (@string)"C3 builtin both spellings:"u8;
private static readonly object c4StringBothSpellingsˢ = (@string)"C4 string both spellings:"u8;
private static readonly object c5ParenPointerConversionˢ = (@string)"C5 paren pointer conversion:"u8;
private static readonly object c6UintptrOfParenPointerˢ = (@string)"C6 uintptr of paren pointer is non-zero:"u8;

internal partial struct main_table /*dyn*/ {
    internal any v;
    internal bool want;
}

internal static void Main() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(@new<nint>());
    fmt.Println(a1ParenFreshAllocationˢ, p != nil);
    var n = @new<nint>();
    n.Value = 42;
    @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(n);
    fmt.Println(a2ParenExistingPointerˢ, q != nil);
    fmt.Println(a3RoundTripsToTheSameˢ, ~(ж<nint>)(uintptr)(q));
    var table = new main_table[]{
        new(@unsafe.Pointer.FromPinnedBox(@new<nint>()), false),
        new(@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default!), true)
    }.slice();
    foreach (var (i, row) in table) {
        fmt.Println(a4Rowˢ, i, isNilˢ, AreEqual(row.v, ((any)@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default!))), wantˢ, row.want);
    }
    @unsafe.Pointer r = @unsafe.Pointer.FromPinnedBox(@new<nint>());
    fmt.Println(b1BareFreshAllocationNonˢ, r != nil);
    @unsafe.Pointer s = @unsafe.Pointer.FromPinnedBox(n);
    fmt.Println(b2BareExistingPointerˢ, ~(ж<nint>)(uintptr)(s));
    var c1 = ((Celsius)36.6D);
    var c2 = ((Celsius)36.6D);
    fmt.Println(c1NamedFloatBothˢ, c1 == c2, c1);
    Counter k1 = ((Counter)7);
    Counter k2 = ((Counter)7);
    fmt.Println(c2NamedIntBothSpellingsˢ, k1 == k2, k1);
    var i1 = (int64)5;
    var i2 = (int64)5;
    fmt.Println(c3BuiltinBothSpellingsˢ, i1 == i2, i1);
    @string b1 = ((@string)new byte[]{104, 105}.slice());
    @string b2 = ((@string)new byte[]{104, 105}.slice());
    fmt.Println(c4StringBothSpellingsˢ, b1 == b2, b1);
    ref var f = ref heap(new float64(), out var Ꮡf);
    f = 1.5D;
    var pf = Ꮡf;
    fmt.Println(c5ParenPointerConversionˢ, pf.Value);
    fmt.Println(c6UintptrOfParenPointerˢ, (uintptr)n != 0);
}

} // end main_package

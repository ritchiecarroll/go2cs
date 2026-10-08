namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct IntSlice /*[]nint*/;

partial struct StrIntMap /*map[@string, nint]*/;

partial struct IntChan /*chan nint*/;

internal static void probe(@string name, bool isNil) {
    fmt.Println(name, isNil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sliceZeroˢ = "sliceZero"u8;
private static readonly @string sliceEmptyLiteralˢ = "sliceEmptyLiteral"u8;
private static readonly @string sliceMakeZeroˢ = "sliceMakeZero"u8;
private static readonly @string sliceFilledˢ = "sliceFilled"u8;
private static readonly @string sliceResliceTailCapZeroˢ = "sliceResliceTailCapZero"u8;
private static readonly @string sliceNilResliceˢ = "sliceNilReslice"u8;
private static readonly @string mapZeroˢ = "mapZero"u8;
private static readonly @string mapEmptyLiteralˢ = "mapEmptyLiteral"u8;
private static readonly @string chanZeroˢ = "chanZero"u8;
private static readonly @string chanMadeˢ = "chanMade"u8;

internal static void Main() {
    IntSlice zeroSlice = default!;
    probe(sliceZeroˢ, zeroSlice == default!);
    var emptySlice = new IntSlice(new nint[]{}.slice());
    probe(sliceEmptyLiteralˢ, emptySlice == default!);
    var madeSlice = new IntSlice(0);
    probe(sliceMakeZeroˢ, madeSlice == default!);
    var filledSlice = new IntSlice(new nint[]{1, 2}.slice());
    probe(sliceFilledˢ, filledSlice == default!);
    var tail = filledSlice[2..2];
    probe(sliceResliceTailCapZeroˢ, tail == default!);
    var nilReslice = zeroSlice[0..0];
    probe(sliceNilResliceˢ, nilReslice == default!);
    StrIntMap zeroMap = default!;
    probe(mapZeroˢ, zeroMap == default!);
    var emptyMap = new StrIntMap(new map<@string, nint>{});
    probe(mapEmptyLiteralˢ, emptyMap == default!);
    IntChan zeroChan = default!;
    probe(chanZeroˢ, zeroChan == default!);
    var madeChan = new IntChan(0);
    probe(chanMadeˢ, madeChan == default!);
}

} // end main_package

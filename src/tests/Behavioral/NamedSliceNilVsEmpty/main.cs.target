namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct IntSlice /*[]nint*/;

partial struct StrIntMap /*map[@string, nint]*/;

partial struct IntChan /*chan nint*/;

internal static void probe(@string name, bool isNil) {
    fmt.Println(name, isNil);
}

internal static void Main() {
    IntSlice zeroSlice = default;
    probe("sliceZero", zeroSlice == nil);

    var emptySlice = new IntSlice(new nint[]{}.slice());
    probe("sliceEmptyLiteral", emptySlice == nil);

    var madeSlice = new IntSlice(0);
    probe("sliceMakeZero", madeSlice == nil);

    var filledSlice = new IntSlice(new nint[]{1, 2}.slice());
    probe("sliceFilled", filledSlice == nil);

    var tail = filledSlice[2..2];
    probe("sliceResliceTailCapZero", tail == nil);

    var nilReslice = zeroSlice[0..0];
    probe("sliceNilReslice", nilReslice == nil);

    StrIntMap zeroMap = default;
    probe("mapZero", zeroMap == nil);

    var emptyMap = new StrIntMap(new map<@string, nint>{});
    probe("mapEmptyLiteral", emptyMap == nil);

    IntChan zeroChan = default;
    probe("chanZero", zeroChan == nil);

    var madeChan = new IntChan(0);
    probe("chanMade", madeChan == nil);
}

} // end main_package

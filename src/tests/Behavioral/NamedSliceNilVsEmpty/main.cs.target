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
    probe("sliceZero", zeroSlice == default);

    var emptySlice = new IntSlice(new nint[]{}.slice());
    probe("sliceEmptyLiteral", emptySlice == default);

    var madeSlice = new IntSlice(0);
    probe("sliceMakeZero", madeSlice == default);

    var filledSlice = new IntSlice(new nint[]{1, 2}.slice());
    probe("sliceFilled", filledSlice == default);

    var tail = filledSlice[2..2];
    probe("sliceResliceTailCapZero", tail == default);

    var nilReslice = zeroSlice[0..0];
    probe("sliceNilReslice", nilReslice == default);

    StrIntMap zeroMap = default;
    probe("mapZero", zeroMap == default);

    var emptyMap = new StrIntMap(new map<@string, nint>{});
    probe("mapEmptyLiteral", emptyMap == default);

    IntChan zeroChan = default;
    probe("chanZero", zeroChan == default);

    var madeChan = new IntChan(0);
    probe("chanMade", madeChan == default);
}

} // end main_package

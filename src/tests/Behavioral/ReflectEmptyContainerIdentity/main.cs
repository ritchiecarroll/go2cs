namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object observedSliceOfArrayˢ = (@string)"observed slice-of-array identical:"u8;
private static readonly object observed6Equals8ˢ = (@string)"observed [][6] equals [][8]:"u8;
private static readonly object observedElemLenˢ = (@string)"observed elem len:"u8;
private static readonly object emptySliceOfArrayˢ = (@string)"empty slice-of-array identical:"u8;
private static readonly object emptyElemLenˢ = (@string)"empty elem len:"u8;
private static readonly object empty3EqualsSliceOfˢ = (@string)"empty [][3] equals SliceOf(ArrayOf(4)):"u8;
private static readonly object ambiguous3Identicalˢ = (@string)"ambiguous 3 identical:"u8;
private static readonly object ambiguous4Identicalˢ = (@string)"ambiguous 4 identical:"u8;
private static readonly object ambiguous3Equals4ˢ = (@string)"ambiguous [][3] equals [][4]:"u8;
private static readonly object ambiguousLensˢ = (@string)"ambiguous lens:"u8;

internal static void Main() {
    var byteT = reflect.TypeOf((uint8)0);
    var (arr3, arr4) = (reflect.ArrayOf(3, byteT), reflect.ArrayOf(4, byteT));

    fmt.Println(observedSliceOfArrayˢ, AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.slice(), 3))));
    fmt.Println(observed6Equals8ˢ, AreEqual(reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(8)}.slice(), 8))));
    fmt.Println(observedElemLenˢ, reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.slice(), 3)).Elem().Len());

    fmt.Println(emptySliceOfArrayˢ, AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));
    fmt.Println(emptyElemLenˢ, reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3)).Elem().Len());
    fmt.Println(empty3EqualsSliceOfˢ, AreEqual(reflect.SliceOf(arr4), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));

    var (e3, e4) = (GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3), GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 4));
    fmt.Println(ambiguous3Identicalˢ, AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(e3)));
    fmt.Println(ambiguous4Identicalˢ, AreEqual(reflect.SliceOf(arr4), reflect.TypeOf(e4)));
    fmt.Println(ambiguous3Equals4ˢ, AreEqual(reflect.TypeOf(e3), reflect.TypeOf(e4)));
    fmt.Println(ambiguousLensˢ, reflect.TypeOf(e3).Elem().Len(), reflect.TypeOf(e4).Elem().Len());
}

} // end main_package

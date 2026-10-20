namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static void Main() {
    var byteT = reflect.TypeOf((uint8)0);
    var (arr3, arr4) = (reflect.ArrayOf(3, byteT), reflect.ArrayOf(4, byteT));

    fmt.Println("observed slice-of-array identical:", AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.slice(), 3))));
    fmt.Println("observed [][6] equals [][8]:", AreEqual(reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(8)}.slice(), 8))));
    fmt.Println("observed elem len:", reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.slice(), 3)).Elem().Len());

    fmt.Println("empty slice-of-array identical:", AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));
    fmt.Println("empty elem len:", reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3)).Elem().Len());
    fmt.Println("empty [][3] equals SliceOf(ArrayOf(4)):", AreEqual(reflect.SliceOf(arr4), reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));

    var (e3, e4) = (GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3), GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 4));
    fmt.Println("ambiguous 3 identical:", AreEqual(reflect.SliceOf(arr3), reflect.TypeOf(e3)));
    fmt.Println("ambiguous 4 identical:", AreEqual(reflect.SliceOf(arr4), reflect.TypeOf(e4)));
    fmt.Println("ambiguous [][3] equals [][4]:", AreEqual(reflect.TypeOf(e3), reflect.TypeOf(e4)));
    fmt.Println("ambiguous lens:", reflect.TypeOf(e3).Elem().Len(), reflect.TypeOf(e4).Elem().Len());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Celsius /*num:float64*/;

partial struct pair {
    public int32 A;
    public array<uint8> B = new(2);
}

internal static void describe(@string label, reflectꓸType t) {
    fmt.Printf("%s: %v | kind=%v len=%d elem=%v size=%d align=%d name=%q\n"u8,
        label, t, t.Kind(), t.Len(), t.Elem(), t.Size(), t.Align(), t.Name());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "no panic"u8;

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

internal static reflectꓸType nest(nint depth, reflectꓸType elem) {
    for (nint i = 0; i < depth; i++) {
        elem = reflect.ArrayOf(2, elem);
    }
    return elem;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string arrayOf3Uint8ˢ = "ArrayOf(3, uint8)"u8;
private static readonly @string declared3Uint8ˢ = "declared [3]uint8"u8;
private static readonly object identicalˢ = (@string)"identical:"u8;
private static readonly object elemIdenticalˢ = (@string)"| elem identical:"u8;
private static readonly object assignableˢ = (@string)"| assignable:"u8;
private static readonly object comparableˢ = (@string)"| comparable:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object identicalˢ2 = (@string)"| identical:"u8;
private static readonly object lenˢ = (@string)"| len:"u8;
private static readonly object elemˢ = (@string)"| elem:"u8;
private static readonly object elemLenˢ = (@string)"| elem len:"u8;
private static readonly object sizeˢ = (@string)"| size:"u8;
private static readonly @string arrayOf2Celsiusˢ = "ArrayOf(2, Celsius)"u8;
private static readonly object celsiusIdenticalˢ = (@string)"celsius identical:"u8;
private static readonly object pointerElemˢ = (@string)"pointer elem:"u8;
private static readonly object structElemˢ = (@string)"struct elem:"u8;
private static readonly object zeroLengthˢ = (@string)"zero length:"u8;
private static readonly object deepequalˢ = (@string)"deepequal:"u8;
private static readonly object nest5ˢ = (@string)"nest(5):"u8;
private static readonly @string sliceOfUint8ˢ = "SliceOf(uint8)"u8;
private static readonly object sliceIdenticalˢ = (@string)"slice identical:"u8;
private static readonly object sliceOfArrayIdenticalToˢ = (@string)"slice of array identical to declared:"u8;
private static readonly object arrayOfSliceˢ = (@string)"array of slice:"u8;
private static readonly object sliceOfSliceˢ = (@string)"| slice of slice:"u8;
private static readonly object sliceOfPointerˢ = (@string)"| slice of pointer:"u8;
private static readonly object negativeLengthˢ = (@string)"negative length:"u8;

internal static void Main() {
    var byteT = reflect.TypeOf((uint8)0);
    array<array<uint8>> nestedDecl = new(2, () => new(3));
    array<array<array<array<array<uint8>>>>> deepDecl = new(2, () => new(2, () => new(2, () => new(2, () => new(2)))));
    var made = reflect.ArrayOf(3, byteT);
    var declared = reflect.TypeOf(new uint8[]{}.array(3));
    describe(arrayOf3Uint8ˢ, made);
    describe(declared3Uint8ˢ, declared);
    fmt.Println(identicalˢ, AreEqual(made, declared), elemIdenticalˢ, AreEqual(made.Elem(), byteT),
        assignableˢ, made.AssignableTo(declared), comparableˢ, made.Comparable());
    var outer = reflect.ArrayOf(2, reflect.ArrayOf(3, byteT));
    fmt.Println(nestedˢ, outer, identicalˢ2, AreEqual(outer, reflect.TypeOf(nestedDecl)),
        lenˢ, outer.Len(), elemˢ, outer.Elem(), elemLenˢ, outer.Elem().Len(), sizeˢ, outer.Size());
    var celsius = reflect.ArrayOf(2, reflect.TypeOf(((Celsius)0D)));
    describe(arrayOf2Celsiusˢ, celsius);
    fmt.Println(celsiusIdenticalˢ, AreEqual(celsius, reflect.TypeOf(new Celsius[]{}.array(2))));
    var ptr = reflect.ArrayOf(2, reflect.PointerTo(byteT));
    fmt.Println(pointerElemˢ, ptr, elemˢ, ptr.Elem(), identicalˢ2, AreEqual(ptr, reflect.TypeOf(new ж<uint8>[]{}.array(2))));
    var structs = reflect.ArrayOf(2, reflect.TypeOf(new pair(nil)));
    fmt.Println(structElemˢ, structs, lenˢ, structs.Len(), sizeˢ, structs.Size(),
        identicalˢ2, AreEqual(structs, reflect.TypeOf(new pair[]{}.array(2, () => new()))));
    var empty = reflect.ArrayOf(0, byteT);
    fmt.Println(zeroLengthˢ, empty, lenˢ, empty.Len(), sizeˢ, empty.Size(),
        identicalˢ2, AreEqual(empty, reflect.TypeOf(new uint8[]{}.array())));
    var v = reflect.New(made).Elem();
    for (nint i = 0; i < v.Len(); i++) {
        v.Index(i).SetUint((uint64)(10 * (i + 1)));
    }
    fmt.Printf("new+index: %v | type=%v | asserted=%v\n"u8, v.Interface(), v.Type(), v.Interface()._<array<uint8>>());
    fmt.Printf("zero: %v | %v\n"u8, reflect.Zero(made).Interface(), reflect.Zero(outer).Interface());
    fmt.Println(deepequalˢ, reflect.DeepEqual(reflect.Zero(made).Interface(), new uint8[]{}.array(3)));
    var deep = nest(5, byteT);
    fmt.Println(nest5ˢ, deep, sizeˢ, deep.Size(), lenˢ, deep.Len(),
        identicalˢ2, AreEqual(deep, reflect.TypeOf(deepDecl)));
    var sl = reflect.SliceOf(byteT);
    describeSlice(sliceOfUint8ˢ, sl);
    fmt.Println(sliceIdenticalˢ, AreEqual(sl, reflect.TypeOf(new uint8[]{}.slice())), elemIdenticalˢ, AreEqual(sl.Elem(), byteT));
    var slArr = reflect.SliceOf(reflect.ArrayOf(3, byteT));
    fmt.Println(sliceOfArrayIdenticalToˢ, AreEqual(slArr, reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));
    fmt.Println(arrayOfSliceˢ, reflect.ArrayOf(2, sl),
        identicalˢ2, AreEqual(reflect.ArrayOf(2, sl), reflect.TypeOf(new slice<uint8>[]{}.array(2))),
        sliceOfSliceˢ, AreEqual(reflect.SliceOf(sl), reflect.TypeOf(new slice<uint8>[]{}.slice())),
        sliceOfPointerˢ, AreEqual(reflect.SliceOf(reflect.PointerTo(byteT)), reflect.TypeOf(new ж<uint8>[]{}.slice())));
    var sv = reflect.MakeSlice(sl, 2, 4);
    sv.Index(0).SetUint(5);
    sv.Index(1).SetUint(6);
    fmt.Printf("makeslice: %v | len=%d cap=%d | asserted=%v\n"u8, sv.Interface(), sv.Len(), sv.Cap(),
        sv.Interface()._<slice<uint8>>());
    var byteTʗ1 = byteT;
    fmt.Println(negativeLengthˢ, recovered(() => {
        reflect.ArrayOf(-1, byteTʗ1);
    }));
}

internal static void describeSlice(@string label, reflectꓸType t) {
    fmt.Printf("%s: %v | kind=%v elem=%v size=%d align=%d name=%q\n"u8,
        label, t, t.Kind(), t.Elem(), t.Size(), t.Align(), t.Name());
}

} // end main_package

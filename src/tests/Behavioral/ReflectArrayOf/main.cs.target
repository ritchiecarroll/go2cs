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

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = "no panic";
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

internal static void Main() {
    var byteT = reflect.TypeOf((uint8)0);

    array<array<uint8>> nestedDecl = new(2, () => new(3));
    array<array<array<array<array<uint8>>>>> deepDecl = new(2, () => new(2, () => new(2, () => new(2, () => new(2)))));

    var made = reflect.ArrayOf(3, byteT);
    var declared = reflect.TypeOf(new uint8[]{}.array(3));
    describe("ArrayOf(3, uint8)", made);
    describe("declared [3]uint8", declared);
    fmt.Println("identical:", AreEqual(made, declared), "| elem identical:", AreEqual(made.Elem(), byteT),
        "| assignable:", made.AssignableTo(declared), "| comparable:", made.Comparable());

    var outer = reflect.ArrayOf(2, reflect.ArrayOf(3, byteT));
    fmt.Println("nested:", outer, "| identical:", AreEqual(outer, reflect.TypeOf(nestedDecl)),
        "| len:", outer.Len(), "| elem:", outer.Elem(), "| elem len:", outer.Elem().Len(), "| size:", outer.Size());

    var celsius = reflect.ArrayOf(2, reflect.TypeOf((Celsius)0D));
    describe("ArrayOf(2, Celsius)", celsius);
    fmt.Println("celsius identical:", AreEqual(celsius, reflect.TypeOf(new Celsius[]{}.array(2))));

    var ptr = reflect.ArrayOf(2, reflect.PointerTo(byteT));
    fmt.Println("pointer elem:", ptr, "| elem:", ptr.Elem(), "| identical:", AreEqual(ptr, reflect.TypeOf(new ж<uint8>[]{}.array(2))));

    var structs = reflect.ArrayOf(2, reflect.TypeOf(new pair()));
    fmt.Println("struct elem:", structs, "| len:", structs.Len(), "| size:", structs.Size(),
        "| identical:", AreEqual(structs, reflect.TypeOf(new pair[]{}.array(2, () => new()))));

    var empty = reflect.ArrayOf(0, byteT);
    fmt.Println("zero length:", empty, "| len:", empty.Len(), "| size:", empty.Size(),
        "| identical:", AreEqual(empty, reflect.TypeOf(new uint8[]{}.array())));

    var v = reflect.New(made).Elem();
    for (nint i = 0; i < v.Len(); i++) {
        v.Index(i).SetUint((uint64)(10 * (i + 1)));
    }
    fmt.Printf("new+index: %v | type=%v | asserted=%v\n"u8, v.Interface(), v.Type(), v.Interface()._<array<uint8>>());
    fmt.Printf("zero: %v | %v\n"u8, reflect.Zero(made).Interface(), reflect.Zero(outer).Interface());
    fmt.Println("deepequal:", reflect.DeepEqual(reflect.Zero(made).Interface(), new uint8[]{}.array(3)));

    var deep = nest(5, byteT);
    fmt.Println("nest(5):", deep, "| size:", deep.Size(), "| len:", deep.Len(),
        "| identical:", AreEqual(deep, reflect.TypeOf(deepDecl)));

    var sl = reflect.SliceOf(byteT);
    describeSlice("SliceOf(uint8)", sl);
    fmt.Println("slice identical:", AreEqual(sl, reflect.TypeOf(new uint8[]{}.slice())), "| elem identical:", AreEqual(sl.Elem(), byteT));

    var slArr = reflect.SliceOf(reflect.ArrayOf(3, byteT));
    fmt.Println("slice of array identical to declared:", AreEqual(slArr, reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{}.slice(), 3))));

    fmt.Println("array of slice:", reflect.ArrayOf(2, sl),
        "| identical:", AreEqual(reflect.ArrayOf(2, sl), reflect.TypeOf(new slice<uint8>[]{}.array(2))),
        "| slice of slice:", AreEqual(reflect.SliceOf(sl), reflect.TypeOf(new slice<uint8>[]{}.slice())),
        "| slice of pointer:", AreEqual(reflect.SliceOf(reflect.PointerTo(byteT)), reflect.TypeOf(new ж<uint8>[]{}.slice())));

    var sv = reflect.MakeSlice(sl, 2, 4);
    sv.Index(0).SetUint(5);
    sv.Index(1).SetUint(6);
    fmt.Printf("makeslice: %v | len=%d cap=%d | asserted=%v\n"u8, sv.Interface(), sv.Len(), sv.Cap(),
        sv.Interface()._<slice<uint8>>());

    var byteTʗ1 = byteT;
    fmt.Println("negative length:", recovered(() => {
        reflect.ArrayOf(-1, byteTʗ1);
    }));
}

internal static void describeSlice(@string label, reflectꓸType t) {
    fmt.Printf("%s: %v | kind=%v elem=%v size=%d align=%d name=%q\n"u8,
        label, t, t.Kind(), t.Elem(), t.Size(), t.Align(), t.Name());
}

} // end main_package

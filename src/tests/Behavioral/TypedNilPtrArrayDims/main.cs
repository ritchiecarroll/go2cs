namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct pkgNamed3 /*[3]byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object distinctTypesˢ = (@string)"distinct types:"u8;
private static readonly object t3ElemLenˢ = (@string)"t3 elem len:"u8;
private static readonly object valueLenOfNil3Intˢ = (@string)"value Len of nil *[3]int:"u8;
private static readonly object valueCapOfNil3Intˢ = (@string)"value Cap of nil *[3]int:"u8;
private static readonly object sameTypeEqualˢ = (@string)"same type equal:"u8;
private static readonly object sameTypeSameReflectTypeˢ = (@string)"same type, same reflect.Type:"u8;
private static readonly object boxedNilIsNonNilˢ = (@string)"boxed nil is non-nil interface:"u8;
private static readonly object packageLevelNamedArrayˢ = (@string)"package-level named array elem len:"u8;
private static readonly object localNamedArrayElemLenˢ = (@string)"local named array elem len:"u8;

internal partial struct main_named3 /*[3]byte*/;

internal static void Main() {
    var t0 = reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(0L));
    var t3 = reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(3L));
    fmt.Println(distinctTypesˢ, !AreEqual(t0, t3));
    fmt.Println((@string)"t0:"u8, t0, (@string)"t3:"u8, t3);
    fmt.Println(t3ElemLenˢ, t3.Elem().Len());
    fmt.Println(valueLenOfNil3Intˢ, reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(3L)).Len());
    fmt.Println(valueCapOfNil3Intˢ, reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(3L)).Cap());
    any a = ж<array<byte>>.NilBoxOfDims(3L);
    any b = ж<array<byte>>.NilBoxOfDims(3L);
    fmt.Println(sameTypeEqualˢ, AreEqual(a, b));
    fmt.Println(sameTypeSameReflectTypeˢ, AreEqual(reflect.TypeOf(a), reflect.TypeOf(b)));
    fmt.Println(boxedNilIsNonNilˢ, a != default!);
    fmt.Printf("dynamic type: %T\n"u8, a);
    fmt.Println(packageLevelNamedArrayˢ, reflect.TypeOf(ж<pkgNamed3>.NilBoxOfDims(3L)).Elem().Len());
    fmt.Println(localNamedArrayElemLenˢ, reflect.TypeOf(ж<main_named3>.NilBoxOfDims(3L)).Elem().Len());
}

} // end main_package

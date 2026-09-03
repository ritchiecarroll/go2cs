namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sliceOfArrayOf6Uint8ˢ = (@string)"1 SliceOf(ArrayOf(6,uint8)) == TypeOf([][6]uint8):"u8;
private static readonly object typeOf6Uint8TypeOf8Uint8ˢ = (@string)"2 TypeOf([][6]uint8) != TypeOf([][8]uint8):        "u8;
private static readonly object pointerToArrayOf6Uint8ˢ = (@string)"3 PointerTo(ArrayOf(6,uint8)) == TypeOf(&[6]uint8):"u8;
private static readonly object mapOfArrayOf2IntIntˢ = (@string)"4 MapOf(ArrayOf(2,int),int) == TypeOf(map[[2]int]int):"u8;
private static readonly object typeOfMap2IntIntKeyLen2ˢ = (@string)"5 TypeOf(map[[2]int]int).Key().Len() == 2:        "u8;
private static readonly object mapOfStringArrayOf3Intˢ = (@string)"6 MapOf(string,ArrayOf(3,int)) == TypeOf(map[string][3]int):"u8;
private static readonly object typeOf23IntElemElemLen3ˢ = (@string)"7 TypeOf([][2][3]int).Elem().Elem().Len() == 3:   "u8;
private static readonly object typeOf4ByteElemElemLen4ˢ = (@string)"8 TypeOf([]*[4]byte).Elem().Elem().Len() == 4:    "u8;
private static readonly object deepEqual6Uint88Uint8ˢ = (@string)"9 DeepEqual([][6]uint8, [][8]uint8) == false:     "u8;
private static readonly object typeOfMapStringStringˢ = (@string)"10 TypeOf(map[string][]string{len1}) == TypeOf(map[string][]string{len2}):"u8;
private static readonly object typeOfIntLen1TypeOfIntˢ = (@string)"11 TypeOf([][]int{len1}) == TypeOf([][]int{len2}):"u8;
private static readonly object deepEqualSameMapˢ = (@string)"12 DeepEqual(same map, different insertion order):"u8;

internal static void Main() {
    var sixes = new array<uint8>[]{new uint8[]{}.array(6)}.slice();
    var eights = new array<uint8>[]{new uint8[]{}.array(8)}.slice();
    var t6 = reflect.TypeOf(sixes);
    fmt.Println(sliceOfArrayOf6Uint8ˢ, AreEqual(reflect.SliceOf(reflect.ArrayOf(6, reflect.TypeOf((uint8)0))), t6));
    fmt.Println(typeOf6Uint8TypeOf8Uint8ˢ, !AreEqual(t6, reflect.TypeOf(eights)));
    fmt.Println(pointerToArrayOf6Uint8ˢ, AreEqual(reflect.PointerTo(reflect.ArrayOf(6, reflect.TypeOf((uint8)0))), reflect.TypeOf(Ꮡ(new uint8[]{}.array(6)))));
    var m = new map<array<nint>, nint>{[new nint[]{}.array(2)] = 0};
    fmt.Println(mapOfArrayOf2IntIntˢ, AreEqual(reflect.MapOf(reflect.ArrayOf(2, reflect.TypeOf((nint)(0))), reflect.TypeOf((nint)(0))), reflect.TypeOf(m)));
    fmt.Println(typeOfMap2IntIntKeyLen2ˢ, reflect.TypeOf(m).Key().Len() == 2);
    var me = new map<@string, array<nint>>{[""u8] = new nint[]{}.array(3)};
    fmt.Println(mapOfStringArrayOf3Intˢ, AreEqual(reflect.MapOf(reflect.TypeOf((@string)""u8), reflect.ArrayOf(3, reflect.TypeOf((nint)(0)))), reflect.TypeOf(me)));
    var nested = new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice();
    fmt.Println(typeOf23IntElemElemLen3ˢ, reflect.TypeOf(nested).Elem().Elem().Len() == 3);
    var ptrs = new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice();
    fmt.Println(typeOf4ByteElemElemLen4ˢ, reflect.TypeOf(ptrs).Elem().Elem().Len() == 4);
    fmt.Println(deepEqual6Uint88Uint8ˢ, !reflect.DeepEqual(sixes, eights));
    var m1 = new map<@string, slice<@string>>{["a"u8] = new @string[]{"x"u8}.slice()};
    var m2 = new map<@string, slice<@string>>{["b"u8] = new @string[]{"x"u8, "y"u8}.slice()};
    fmt.Println(typeOfMapStringStringˢ, AreEqual(reflect.TypeOf(m1), reflect.TypeOf(m2)));
    var s1 = new slice<nint>[]{new nint[]{1}.slice()}.slice();
    var s2 = new slice<nint>[]{new nint[]{1, 2}.slice()}.slice();
    fmt.Println(typeOfIntLen1TypeOfIntˢ, AreEqual(reflect.TypeOf(s1), reflect.TypeOf(s2)));
    var h1 = new map<@string, slice<@string>>{["k"u8] = new @string[]{"x"u8, "y"u8}.slice(), ["j"u8] = new @string[]{"z"u8}.slice()};
    var h2 = new map<@string, slice<@string>>{["j"u8] = new @string[]{"z"u8}.slice(), ["k"u8] = new @string[]{"x"u8, "y"u8}.slice()};
    fmt.Println(deepEqualSameMapˢ, reflect.DeepEqual(h1, h2));
}

} // end main_package

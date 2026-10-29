namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static void Main() {
    var sixes = GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6);
    var eights = GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(8)}.slice(), 8);
    var t6 = reflect.TypeOf(sixes);

    fmt.Println("1 SliceOf(ArrayOf(6,uint8)) == TypeOf([][6]uint8):", AreEqual(reflect.SliceOf(reflect.ArrayOf(6, reflect.TypeOf((uint8)0))), t6));
    fmt.Println("2 TypeOf([][6]uint8) != TypeOf([][8]uint8):        ", !AreEqual(t6, reflect.TypeOf(eights)));
    fmt.Println("3 PointerTo(ArrayOf(6,uint8)) == TypeOf(&[6]uint8):", AreEqual(reflect.PointerTo(reflect.ArrayOf(6, reflect.TypeOf((uint8)0))), reflect.TypeOf(Ꮡ(new uint8[]{}.array(6)))));

    var m = new map<array<nint>, nint>{[new nint[]{}.array(2)] = 0};
    fmt.Println("4 MapOf(ArrayOf(2,int),int) == TypeOf(map[[2]int]int):", AreEqual(reflect.MapOf(reflect.ArrayOf(2, reflect.TypeOf((nint)0)), reflect.TypeOf((nint)0)), reflect.TypeOf(m)));
    fmt.Println("5 TypeOf(map[[2]int]int).Key().Len() == 2:        ", reflect.TypeOf(m).Key().Len() == 2);

    var me = new map<@string, array<nint>>{[""] = new nint[]{}.array(3)};
    fmt.Println("6 MapOf(string,ArrayOf(3,int)) == TypeOf(map[string][3]int):", AreEqual(reflect.MapOf(reflect.TypeOf(""), reflect.ArrayOf(3, reflect.TypeOf((nint)0))), reflect.TypeOf(me)));

    var nested = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice(), 2, 3);
    fmt.Println("7 TypeOf([][2][3]int).Elem().Elem().Len() == 3:   ", reflect.TypeOf(nested).Elem().Elem().Len() == 3);

    var ptrs = slice<ж<array<byte>>>([Ꮡ(new byte[]{}.array(4))]);
    fmt.Println("8 TypeOf([]*[4]byte).Elem().Elem().Len() == 4:    ", reflect.TypeOf(ptrs).Elem().Elem().Len() == 4);

    fmt.Println("9 DeepEqual([][6]uint8, [][8]uint8) == false:     ", !reflect.DeepEqual(sixes, eights));
    var m1 = new map<@string, slice<@string>>{["a"] = slice<@string>(["x"])};
    var m2 = new map<@string, slice<@string>>{["b"] = slice<@string>(["x", "y"])};
    fmt.Println("10 TypeOf(map[string][]string{len1}) == TypeOf(map[string][]string{len2}):", AreEqual(reflect.TypeOf(m1), reflect.TypeOf(m2)));
    var s1 = slice<slice<nint>>([slice<nint>([1])]);
    var s2 = slice<slice<nint>>([slice<nint>([1, 2])]);
    fmt.Println("11 TypeOf([][]int{len1}) == TypeOf([][]int{len2}):", AreEqual(reflect.TypeOf(s1), reflect.TypeOf(s2)));
    var h1 = new map<@string, slice<@string>>{["k"] = slice<@string>(["x", "y"]), ["j"] = slice<@string>(["z"])};
    var h2 = new map<@string, slice<@string>>{["j"] = slice<@string>(["z"]), ["k"] = slice<@string>(["x", "y"])};
    fmt.Println("12 DeepEqual(same map, different insertion order):", reflect.DeepEqual(h1, h2));
}

} // end main_package

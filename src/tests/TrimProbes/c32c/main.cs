namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Point {
    public nint X, Y;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object new4Intˢ = (@string)"new [4]int:"u8;
private static readonly object newIntElemNilˢ = (@string)"new []int elem nil:"u8;
private static readonly object newMapElemNilˢ = (@string)"new map elem nil:"u8;
private static readonly object newChanElemNilˢ = (@string)"new chan elem nil:"u8;
private static readonly object newPointLenˢ = (@string)"new []Point len:"u8;
private static readonly object zero4IntIsNilˢ = (@string)"zero *[4]int is nil:"u8;
private static readonly object zeroIntIsNilˢ = (@string)"zero *[]int is nil:"u8;

internal static void Main() {
    var a = reflect.New(reflect.TypeOf(new nint[]{}.array(4)));
    a.Elem().Index(2).SetInt(7);
    fmt.Println(new4Intˢ, a.Interface()._<ж<array<nint>>>().Value);
    var s = reflect.New(reflect.TypeOf(new nint[]{}.slice()));
    fmt.Println(newIntElemNilˢ, s.Elem().IsNil());
    var m = reflect.New(reflect.TypeOf(new map<@string, nint>{}));
    fmt.Println(newMapElemNilˢ, m.Elem().IsNil());
    var c = reflect.New(reflect.TypeOf(new channel<nint>(0)));
    fmt.Println(newChanElemNilˢ, c.Elem().IsNil());
    var ps = reflect.New(reflect.TypeOf(new Point[]{}.slice()));
    ps.Elem().Set(reflect.ValueOf(new Point[]{new(1, 2)}.slice()));
    fmt.Println(newPointLenˢ, ps.Elem().Len());
    fmt.Println(zero4IntIsNilˢ, reflect.Zero(reflect.TypeOf(Ꮡ(new nint[]{}.array(4)))).IsNil());
    fmt.Println(zeroIntIsNilˢ, reflect.Zero(reflect.TypeOf(Ꮡ(new nint[]{}.slice()))).IsNil());
}

} // end main_package

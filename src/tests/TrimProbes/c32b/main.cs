namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Point {
    public nint X, Y;
}

partial struct Named /*[]nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object newStructˢ = (@string)"new struct:"u8;
private static readonly object newNamedSliceˢ = (@string)"new named slice:"u8;
private static readonly object newIntˢ = (@string)"new int:"u8;
private static readonly object zeroPointIsNilˢ = (@string)"zero *Point is nil:"u8;
private static readonly object newPointBoxPointeeFixedˢ = (@string)"new *Point (box pointee, fixed depth) elem nil:"u8;
private static readonly object newErrorBuiltinsTableˢ = (@string)"new error (builtins table) elem nil:"u8;
private static readonly object newPointFallbackElemNilˢ = (@string)"new **Point (fallback) elem nil:"u8;
private static readonly object newFmtStringerFallbackˢ = (@string)"new fmt.Stringer (fallback) elem nil:"u8;

internal static void Main() {
    var p = reflect.New(reflect.TypeOf(new Point(nil)));
    p.Elem().Set(reflect.ValueOf(new Point(1, 2)));
    fmt.Println(newStructˢ, p.Interface()._<ж<Point>>().Value);
    var n = reflect.New(reflect.TypeOf(new Named(new nint[]{}.slice())));
    n.Elem().Set(reflect.ValueOf(new Named(new nint[]{3, 4}.slice())));
    fmt.Println(newNamedSliceˢ, n.Interface()._<ж<Named>>().ValueSlot);
    var i = reflect.New(reflect.TypeOf((nint)(0)));
    i.Elem().SetInt(5);
    fmt.Println(newIntˢ, i.Interface()._<ж<nint>>().Value);
    fmt.Println(zeroPointIsNilˢ, reflect.Zero(reflect.TypeOf(Ꮡ(new Point(nil)))).IsNil());
    var pp = reflect.New(reflect.TypeOf(Ꮡ(new Point(nil))));
    fmt.Println(newPointBoxPointeeFixedˢ, pp.Elem().IsNil());
    var e = reflect.New(reflect.TypeOf(((ж<error>)nil)).Elem());
    fmt.Println(newErrorBuiltinsTableˢ, e.Elem().IsNil());
    var ppp = reflect.New(reflect.TypeOf(((ж<ж<Point>>)nil)));
    fmt.Println(newPointFallbackElemNilˢ, ppp.Elem().IsNil());
    var st = reflect.New(reflect.TypeOf(((ж<fmt.Stringer>)nil)).Elem());
    fmt.Println(newFmtStringerFallbackˢ, st.Elem().IsNil());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;

partial class main_package {

partial struct Point {
    public nint X, Y;
    public @string Name;
}

partial struct Named /*[]nint*/;

partial struct MyBytes /*[]byte*/;

partial struct B1 /*num:byte*/;

partial struct Tagged1 {
    public nint A; /*`json:"a"`*/
    public @string S;
    public slice<nint> L;
    public ж<nint> P;
}

partial struct Tagged2 {
    public nint A; /*`xml:"a"`*/
    public @string S;
    public slice<nint> L;
    public ж<nint> P;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zeroStructˢ = (@string)"zero struct:"u8;
private static readonly object zeroSliceNilˢ = (@string)"zero slice nil:"u8;
private static readonly object zeroMapNilˢ = (@string)"zero map nil:"u8;
private static readonly object zero23Intˢ = (@string)"zero [2][3]int:"u8;
private static readonly object makeNamedSliceˢ = (@string)"make named slice:"u8;
private static readonly object makeMapˢ = (@string)"make map:"u8;
private static readonly object makeChanˢ = (@string)"make chan:"u8;
private static readonly object setbytesNamedˢ = (@string)"setbytes named:"u8;
private static readonly object setbytesNamedElemˢ = (@string)"setbytes named elem:"u8;
private static readonly @string changedˢ = "changed"u8;
private static readonly object convertAfterGcˢ = (@string)"convert after GC:"u8;

internal static void Main() {
    fmt.Println(zeroStructˢ, reflect.Zero(reflect.TypeOf(new Point(nil))).Interface());
    fmt.Println(zeroSliceNilˢ, reflect.Zero(reflect.TypeOf(new nint[]{}.slice())).IsNil());
    fmt.Println(zeroMapNilˢ, reflect.Zero(reflect.TypeOf(new map<@string, nint>{})).IsNil());
    array<array<nint>> arr = new(2, () => new(3));
    var z = reflect.Zero(reflect.TypeOf(arr)).Interface()._<array<array<nint>>>();
    fmt.Println(zero23Intˢ, z, len(z[1]));
    var s = reflect.MakeSlice(reflect.TypeOf(new Named(new nint[]{}.slice())), 2, 4);
    s.Index(1).SetInt(9);
    fmt.Println(makeNamedSliceˢ, s.Interface(), s.Len(), s.Cap(), s.Type().String());
    var m = reflect.MakeMap(reflect.TypeOf(new map<@string, nint>{}));
    m.SetMapIndex(reflect.ValueOf((@string)"k"u8), reflect.ValueOf((nint)(3)));
    fmt.Println(makeMapˢ, m.Interface());
    var c = reflect.MakeChan(reflect.TypeOf(new channel<nint>(0)), 1);
    c.Send(reflect.ValueOf((nint)(7)));
    var (v, ok) = c.Recv();
    fmt.Println(makeChanˢ, v.Int(), ok);
    ref var mb = ref heap<MyBytes>(out var Ꮡmb);
    mb = new MyBytes(new byte[]{1, 2, 3}.slice());
    reflect.ValueOf(Ꮡmb).Elem().SetBytes(new byte[]{9, 8}.slice());
    fmt.Println(setbytesNamedˢ, mb);
    ref var b1 = ref heap<slice<B1>>(out var Ꮡb1);
    b1 = new B1[]{1, 2}.slice();
    reflect.ValueOf(Ꮡb1).Elem().SetBytes(new byte[]{5, 6, 7}.slice());
    fmt.Println(setbytesNamedElemˢ, b1);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 42;
    var t1 = new Tagged1(A: 1, S: "s"u8, L: new nint[]{1, 2}.slice(), P: Ꮡn);
    var t2 = reflect.ValueOf(t1).Convert(reflect.TypeOf(new Tagged2(nil))).Interface()._<Tagged2>();
    t1.S = changedˢ;
    runtime.GC();
    fmt.Println(convertAfterGcˢ, t2.A, t2.S, t2.L, t2.P.Value);
}

} // end main_package

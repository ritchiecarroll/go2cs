namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct T {
    public nint A;
    public @string S;
}

partial struct Inner {
    public nint X;
}

partial struct Outer {
    public partial ref Inner Inner { get; }
    public nint Y;
}

partial struct Base {
    public nint A;
}

partial struct Def /*Base*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object addrWriteThroughˢ = (@string)"1 Addr write-through:"u8;
private static readonly object fmtStructˢ = (@string)"2 fmt &struct:"u8;
private static readonly object setIntReadBackˢ = (@string)"3 SetInt read back:"u8;
private static readonly object identityˢ = (@string)"4 identity:"u8;
private static readonly object promotedˢ = (@string)"5 promoted:"u8;
private static readonly object definedOverStructˢ = (@string)"6 defined over struct:"u8;

internal static void Main() {
    ref var s = ref heap<T>(out var Ꮡs);
    s = new T(1, "x"u8);
    var p = reflect.ValueOf(Ꮡs).Elem().Field(0).Addr().Interface()._<ж<nint>>();
    p.Value = 41;
    fmt.Println(addrWriteThroughˢ, s.A);
    fmt.Println(fmtStructˢ, Ꮡ(new T(2, "y"u8)));
    reflect.ValueOf(Ꮡs).Elem().Field(0).SetInt(42);
    fmt.Println(setIntReadBackˢ, s.A);
    var a1 = reflect.ValueOf(Ꮡs).Elem().Field(0).Addr().Interface()._<ж<nint>>();
    var a2 = reflect.ValueOf(Ꮡs).Elem().Field(0).Addr().Interface()._<ж<nint>>();
    fmt.Println(identityˢ, a1 == a2, a1 == Ꮡs.of(T.ᏑA));
    ref var o = ref heap(new Outer(), out var Ꮡo);
    reflect.ValueOf(Ꮡo).Elem().FieldByName("X"u8).SetInt(7);
    var x = reflect.ValueOf(Ꮡo).Elem().FieldByName("X"u8).Addr().Interface()._<ж<nint>>();
    var @in = reflect.ValueOf(Ꮡo).Elem().Field(0).Addr().Interface()._<ж<Inner>>();
    fmt.Println(promotedˢ, o.X, x.Value, @in == Ꮡo.of(Outer.ᏑInner));
    ref var d = ref heap(new Def(), out var Ꮡd);
    reflect.ValueOf(Ꮡd).Elem().Field(0).SetInt(9);
    fmt.Println(definedOverStructˢ, d.A);
}

} // end main_package

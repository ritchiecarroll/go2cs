namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct inner {
    internal @string a;
}

partial struct Q {
    public nint A;
    internal nint b;
    internal int32 c;
}

partial struct P {
    public nint Public;
    internal nint @private;
    internal inner s;
    internal ж<nint> ptr;
}

internal static reflectꓸValue field(reflectꓸValue v, nint i) {
    var f = v.Type().Field(i);
    return reflect.NewAt(f.Type, (@unsafe.Pointer)((uintptr)(@unsafe.Pointer)v.UnsafeAddr() + f.Offset)).Elem();
}

internal static void Main() {
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 7;
    ref var x = ref heap<P>(out var Ꮡx);
    x = new P(1, 2, new inner("x"u8), Ꮡn);
    var v = reflect.ValueOf(Ꮡx).Elem();
    for (nint i = 0; i < v.NumField(); i++) {
        var ve = field(v, i);
        fmt.Println(v.Type().Field(i).Name, ve.Kind(), ve.CanSet());
    }
    fmt.Println(field(v, 0).Interface(), field(v, 1).Interface(), field(v, 2).Interface(), field(v, 3).Interface()._<ж<nint>>().Value);
    field(v, 1).SetInt(20);
    field(v, 2).Set(reflect.ValueOf(new inner("z"u8)));
    fmt.Println(x.@private, x.s.a);
    ref var y = ref heap<P>(out var Ꮡy);
    y = new P(1, 3, new inner("y"u8), Ꮡn);
    var w = reflect.ValueOf(Ꮡy).Elem();
    fmt.Println(AreEqual(field(v, 1).Interface(), field(w, 1).Interface()));
    fmt.Println(reflect.DeepEqual(field(v, 2).Interface(), field(w, 2).Interface()));
    ref var q = ref heap<Q>(out var Ꮡq);
    q = new Q(4, 5, 6);
    var u = reflect.ValueOf(Ꮡq).Elem();
    fmt.Println(field(u, 0).Interface(), field(u, 1).Interface(), field(u, 2).Interface());
    field(u, 1).SetInt(50);
    fmt.Println(q.b);
    var p = reflect.NewAt(reflect.TypeOf((nint)(0)), @unsafe.Pointer.FromPinnedBox(Ꮡn));
    p.Elem().SetInt(9);
    fmt.Println(n);
}

} // end main_package

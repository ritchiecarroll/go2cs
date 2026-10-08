namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Nested {
    public nint X;
}

partial struct Holder {
    public ж<Nested> Nested;
    public Action Fn;
    public map<@string, nint> M;
}

internal static @string take(any v) {
    return fmt.Sprintf("%#v"u8, v);
}

internal static void Main() {
    var h = new Holder(nil);
    var hv = reflect.ValueOf(h);
    for (nint i = 0; i < hv.NumField(); i++) {
        var f = hv.Field(i);
        @string name = hv.Type().Field(i).Name;
        ref var dst = ref heap<any>(out var Ꮡdst);
        reflect.ValueOf(Ꮡdst).Elem().Set(f);
        var m = new map<@string, any>{};
        reflect.ValueOf(m).SetMapIndex(reflect.ValueOf((@string)"k"u8), f);
        var s = reflect.Append(reflect.ValueOf(new any[]{}.slice()), f).Interface()._<slice<any>>();
        var @out = reflect.ValueOf(take).Call(new reflectꓸValue[]{f}.slice())[0].Interface();
        var ch = new channel<any>(1);
        reflect.ValueOf(ch).Send(f);
        var got = ᐸꟷ(ch);
        fmt.Printf("%s: set=%T map=%T append=%T call=%v send=%T\n"u8, name, dst, m["k"u8], s[0], @out, got);
    }
}

} // end main_package

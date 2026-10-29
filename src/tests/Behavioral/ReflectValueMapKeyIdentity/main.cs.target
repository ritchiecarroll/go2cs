namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;

partial class main_package {

partial struct pair {
    public @string A;
    public nint B;
}

internal static void Main() {
    var m = new map<@string, any>{["vstring"] = (@string)"foo"u8, ["Vint"] = (nint)42, ["vbool"] = true};
    var dataVal = reflect.ValueOf(m);
    var keys = new map<reflectꓸValue, EmptyStruct>();
    foreach (var (_, k) in dataVal.MapKeys()) {
        keys[k] = new EmptyStruct();
    }
    fmt.Println("distinct keys:", len(keys));

    nint matched = 0;
    foreach (var (k, _) in keys) {
        foreach (var (_, field) in slice<@string>(["Vstring", "Vint", "Vbool"])) {
            if (strings.EqualFold(k.Interface()._<@string>(), field)) {
                matched++;
            }
        }
    }
    fmt.Println("matched fields:", matched);

    @string a = strings.Repeat("a", 2);
    @string b = strings.Repeat("b", 2);
    fmt.Println("different strings equal:", reflect.ValueOf(a) == reflect.ValueOf(b));
    ref var s1 = ref heap<pair>(out var Ꮡs1);
    s1 = new pair(a, 1);
    ref var s2 = ref heap<pair>(out var Ꮡs2);
    s2 = new pair(b, 2);
    fmt.Println("different structs equal:", reflect.ValueOf(s1) == reflect.ValueOf(s2));

    var v = reflect.ValueOf(a);
    var w = v;
    fmt.Println("copy equal:", v == w);

    var p = Ꮡs1;
    fmt.Println("same pointer equal:", reflect.ValueOf(p.OrTypedNil()) == reflect.ValueOf(p.OrTypedNil()));
    var q = Ꮡs2;
    fmt.Println("different pointers equal:", reflect.ValueOf(p.OrTypedNil()) == reflect.ValueOf(q.OrTypedNil()));
    fmt.Println("same map equal:", reflect.ValueOf(m) == reflect.ValueOf(m));
    var other = new map<@string, any>{};
    fmt.Println("different maps equal:", reflect.ValueOf(m) == reflect.ValueOf(other));

    fmt.Println("zero equal:", new reflectꓸValue() == new reflectꓸValue());
}

} // end main_package

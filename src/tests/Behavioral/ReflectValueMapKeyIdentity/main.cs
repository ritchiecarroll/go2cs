namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;

partial class main_package {

[GoType] partial struct pair {
    public @string A;
    public nint B;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object distinctKeysˢ = (@string)"distinct keys:"u8;
private static readonly object matchedFieldsˢ = (@string)"matched fields:"u8;
private static readonly object differentStringsEqualˢ = (@string)"different strings equal:"u8;
private static readonly object differentStructsEqualˢ = (@string)"different structs equal:"u8;
private static readonly object copyEqualˢ = (@string)"copy equal:"u8;
private static readonly object samePointerEqualˢ = (@string)"same pointer equal:"u8;
private static readonly object differentPointersEqualˢ = (@string)"different pointers equal:"u8;
private static readonly object sameMapEqualˢ = (@string)"same map equal:"u8;
private static readonly object differentMapsEqualˢ = (@string)"different maps equal:"u8;
private static readonly object zeroEqualˢ = (@string)"zero equal:"u8;

internal static void Main() {
    var m = new map<@string, any>{["vstring"u8] = (@string)"foo"u8, ["Vint"u8] = (nint)(42), ["vbool"u8] = true};
    var dataVal = reflect.ValueOf(m);
    var keys = new map<reflectꓸValue, EmptyStruct>();
    foreach (var (_, k) in dataVal.MapKeys()) {
        keys[k] = new EmptyStruct();
    }
    fmt.Println(distinctKeysˢ, len(keys));
    nint matched = 0;
    foreach (var (k, _) in keys) {
        foreach (var (_, field) in new @string[]{"Vstring"u8, "Vint"u8, "Vbool"u8}.slice()) {
            if (strings.EqualFold(k.Interface()._<@string>(), field)) {
                matched++;
            }
        }
    }
    fmt.Println(matchedFieldsˢ, matched);
    @string a = strings.Repeat("a"u8, 2);
    @string b = strings.Repeat("b"u8, 2);
    fmt.Println(differentStringsEqualˢ, reflect.ValueOf(a) == reflect.ValueOf(b));
    ref var s1 = ref heap<pair>(out var Ꮡs1);
    s1 = new pair(a, 1);
    ref var s2 = ref heap<pair>(out var Ꮡs2);
    s2 = new pair(b, 2);
    fmt.Println(differentStructsEqualˢ, reflect.ValueOf(s1) == reflect.ValueOf(s2));
    var v = reflect.ValueOf(a);
    var w = v;
    fmt.Println(copyEqualˢ, v == w);
    var p = Ꮡs1;
    fmt.Println(samePointerEqualˢ, reflect.ValueOf(p.OrTypedNil()) == reflect.ValueOf(p.OrTypedNil()));
    var q = Ꮡs2;
    fmt.Println(differentPointersEqualˢ, reflect.ValueOf(p.OrTypedNil()) == reflect.ValueOf(q.OrTypedNil()));
    fmt.Println(sameMapEqualˢ, reflect.ValueOf(m) == reflect.ValueOf(m));
    var other = new map<@string, any>{};
    fmt.Println(differentMapsEqualˢ, reflect.ValueOf(m) == reflect.ValueOf(other));
    fmt.Println(zeroEqualˢ, new reflectꓸValue(nil) == new reflectꓸValue(nil));
}

} // end main_package

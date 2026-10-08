namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using sort = sort_package;

partial class main_package {

partial struct T {
    internal nint n;
}

partial struct holder {
    public ж<T> P;
}

internal static void show(@string label, map<any, any> m) {
    slice<@string> keys = default!;
    foreach (var (k, v) in m) {
        keys = append(keys, fmt.Sprintf("%T(%v)=%v"u8, k, k, v));
    }
    sort.Strings(keys);
    fmt.Println(label, len(m), keys);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilKeyˢ = (@string)"nil-key"u8;
private static readonly @string afterNilKeyˢ = "after nil key:"u8;
private static readonly object mapIndexNilˢ = (@string)"MapIndex(nil):"u8;
private static readonly object mNilˢ = (@string)"m[nil]:"u8;
private static readonly object typedNilKeyˢ = (@string)"typed-nil-key"u8;
private static readonly @string afterTypedNilKeyˢ = "after typed-nil key:"u8;
private static readonly object mTNilˢ = (@string)"m[(*T)(nil)]:"u8;
private static readonly object mNilStillˢ = (@string)"m[nil] still:"u8;
private static readonly object mapIndexTNilˢ = (@string)"MapIndex((*T)(nil)):"u8;
private static readonly object mapIndexNilStillˢ = (@string)"MapIndex(nil) still:"u8;
private static readonly object againˢ = (@string)"again"u8;
private static readonly object mNilAfterOverwriteˢ = (@string)"m[nil] after overwrite:"u8;
private static readonly @string afterDeleteNilKeyˢ = "after delete nil key:"u8;
private static readonly object mapKeysˢ = (@string)"MapKeys:"u8;
private static readonly object keyKindˢ = (@string)" key kind:"u8;
private static readonly object elemˢ = (@string)"elem:"u8;
private static readonly object elemNilˢ = (@string)"elem nil:"u8;

internal static void Main() {
    var m = new map<any, any>{};
    var v = reflect.ValueOf(m);
    var keyType = v.Type().Key();
    v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf(nilKeyˢ));
    show(afterNilKeyˢ, m);
    fmt.Println(mapIndexNilˢ, v.MapIndex(reflect.Zero(keyType)));
    fmt.Println(mNilˢ, m[default!]);
    var h = new holder(nil);
    var pk = reflect.ValueOf(h).Field(0);
    v.SetMapIndex(pk, reflect.ValueOf(typedNilKeyˢ));
    show(afterTypedNilKeyˢ, m);
    fmt.Println(mTNilˢ, m[((ж<T>)nil)]);
    fmt.Println(mNilStillˢ, m[default!]);
    fmt.Println(mapIndexTNilˢ, v.MapIndex(pk));
    fmt.Println(mapIndexNilStillˢ, v.MapIndex(reflect.Zero(keyType)));
    v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf(againˢ));
    fmt.Println(mNilAfterOverwriteˢ, m[default!]);
    v.SetMapIndex(reflect.Zero(keyType), new reflectꓸValue(nil));
    show(afterDeleteNilKeyˢ, m);
    var keys = v.MapKeys();
    fmt.Println(mapKeysˢ, len(keys));
    foreach (var (_, k) in keys) {
        fmt.Println(keyKindˢ, k.Kind(), elemˢ, k.Elem().Type(), elemNilˢ, k.Elem().IsNil());
    }
}

} // end main_package

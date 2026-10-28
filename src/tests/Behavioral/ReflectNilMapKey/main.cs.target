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
    slice<@string> keys = default;
    foreach (var (k, v) in m) {
        keys = append(keys, fmt.Sprintf("%T(%v)=%v"u8, k, k, v));
    }
    sort.Strings(keys);
    fmt.Println(label, len(m), keys);
}

internal static void Main() {
    var m = new map<any, any>{};
    var v = reflect.ValueOf(m);
    var keyType = v.Type().Key();

    v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf("nil-key"));
    show("after nil key:", m);
    fmt.Println("MapIndex(nil):", v.MapIndex(reflect.Zero(keyType)));
    fmt.Println("m[nil]:", m[default]);

    var h = new holder();
    var pk = reflect.ValueOf(h).Field(0);
    v.SetMapIndex(pk, reflect.ValueOf("typed-nil-key"));
    show("after typed-nil key:", m);
    fmt.Println("m[(*T)(nil)]:", m[((ж<T>)nil)]);
    fmt.Println("m[nil] still:", m[default]);
    fmt.Println("MapIndex((*T)(nil)):", v.MapIndex(pk));
    fmt.Println("MapIndex(nil) still:", v.MapIndex(reflect.Zero(keyType)));

    v.SetMapIndex(reflect.Zero(keyType), reflect.ValueOf("again"));
    fmt.Println("m[nil] after overwrite:", m[default]);
    v.SetMapIndex(reflect.Zero(keyType), new reflectꓸValue());
    show("after delete nil key:", m);

    var keys = v.MapKeys();
    fmt.Println("MapKeys:", len(keys));
    foreach (var (_, k) in keys) {
        fmt.Println(" key kind:", k.Kind(), "elem:", k.Elem().Type(), "elem nil:", k.Elem().IsNil());
    }
}

} // end main_package

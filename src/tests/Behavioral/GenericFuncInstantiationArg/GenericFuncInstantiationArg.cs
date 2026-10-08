namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static T pick<T>(slice<T> a) {
    return a[0];
}

internal static map<K, V> pair<K, V>(K k, V v) {
    return new map<K, V>{[k] = v};
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object showˢ = (@string)"show:"u8;

internal static void show(any f) {
    fmt.Println(showˢ, reflect.TypeOf(f).String());
}

internal static @string apply(Func<any, @string> f, any v) {
    return f(v);
}

internal static @string describe<T>(T v) {
    return fmt.Sprintf("%v/%T"u8, v, v);
}

internal static any returned() {
    return pick<@string>;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlˢ = (@string)"control:"u8;

internal static void Main() {
    var pf = pick<@string>;
    fmt.Println(controlˢ, reflect.TypeOf((pf).OrTypedNilFunc()).String(), pf(new @string[]{"a"u8, "b"u8, "c"u8}.slice()));
    fmt.Println(reflect.TypeOf(pick<@string>).String());
    fmt.Println(reflect.TypeOf(pair<@string, nint>).String());
    show(pick<nint>);
    show(pair<nint, bool>);
    fmt.Printf("%T\n"u8, pick<float64>);
    fmt.Printf("%T\n"u8, pair<@string, @string>);
    fmt.Println(reflect.TypeOf(returned()).String());
    fmt.Println(apply(describe<any>, (nint)(42)));
    fmt.Println(apply(describe<any>, (@string)"go"u8));
}

} // end main_package

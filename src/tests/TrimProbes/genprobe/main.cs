namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Pair<K, V> {
    public K Key;
    public V Val;
}

partial struct Box<T> {
    internal T inner;
    internal nint n;
}

partial struct Plain {
    internal nint a;
    internal @string s;
}

internal static void Main() {
    var p = new Pair<@string, nint>("a"u8, 1);
    fmt.Printf("%v %+v\n"u8, p, p);
    var b = new Box<Plain>(new Plain(2, "x"u8), 3);
    fmt.Printf("%+v\n"u8, b);
    var t = reflect.TypeOf(p);
    fmt.Println(t.NumField(), t.Field(0).Name, t.Field(1).Type);
    fmt.Println(reflect.DeepEqual(b, new Box<Plain>(new Plain(2, "x"u8), 3)));
    fmt.Printf("%v\n"u8, new Plain(7, "plain"u8));
}

} // end main_package

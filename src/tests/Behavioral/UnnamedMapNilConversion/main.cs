namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct myMap /*map[@string, nint]*/;

partial struct mySlice /*[]byte*/;

internal static void Main() {
    fmt.Println(reflect.TypeOf(((map<@string, nint>)default!)));
    fmt.Println(reflect.TypeOf(((map<nint, byte>)default!)));
    fmt.Println(reflect.TypeOf(((map<@string, slice<Header>>)default!)));
    fmt.Println(reflect.TypeOf(((map<Key, Header>)default!)));

    var nilMap = ((map<@string, nint>)default!);
    fmt.Println("len:", len(nilMap), "isNil:", nilMap == default!);
    fmt.Println("read:", nilMap["absent"]);

    fmt.Println(reflect.TypeOf(((myMap)default!)));
    fmt.Println(reflect.TypeOf(((mySlice)default!)));
    fmt.Println(reflect.TypeOf(slice<byte>(default!)));
    fmt.Println(reflect.TypeOf((channel<nint>)(default!)));
    fmt.Println(reflect.TypeOf(((ж<nint>)nil)));

    var populated = new map<@string, nint>{["a"] = 1};
    fmt.Println("copy len:", len(((map<@string, nint>)populated)));
}

partial struct Key {
    public @string K;
}

partial struct Header {
    public @string N;
}

} // end main_package

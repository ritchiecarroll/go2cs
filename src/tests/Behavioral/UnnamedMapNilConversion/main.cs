namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct myMap /*map[@string, nint]*/;

partial struct mySlice /*[]byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object isNilˢ = (@string)"isNil:"u8;
private static readonly object readˢ = (@string)"read:"u8;
private static readonly @string absentˢ = "absent"u8;
private static readonly object copyLenˢ = (@string)"copy len:"u8;

internal static void Main() {
    fmt.Println(reflect.TypeOf(((map<@string, nint>)default!)));
    fmt.Println(reflect.TypeOf(((map<nint, byte>)default!)));
    fmt.Println(reflect.TypeOf(((map<@string, slice<Header>>)default!)));
    fmt.Println(reflect.TypeOf(((map<Key, Header>)default!)));
    var nilMap = ((map<@string, nint>)default!);
    fmt.Println(lenˢ, len(nilMap), isNilˢ, nilMap == default!);
    fmt.Println(readˢ, nilMap[absentˢ]);
    fmt.Println(reflect.TypeOf(((myMap)default!)));
    fmt.Println(reflect.TypeOf(((mySlice)default!)));
    fmt.Println(reflect.TypeOf(slice<byte>(default!)));
    fmt.Println(reflect.TypeOf((channel<nint>)(default!)));
    fmt.Println(reflect.TypeOf(((ж<nint>)nil)));
    var populated = new map<@string, nint>{["a"u8] = 1};
    fmt.Println(copyLenˢ, len(((map<@string, nint>)populated)));
}

partial struct Key {
    public @string K;
}

partial struct Header {
    public @string N;
}

} // end main_package

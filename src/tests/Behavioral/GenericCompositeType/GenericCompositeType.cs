namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Option<T> {
    internal T value;
    internal bool valid;
}

public static Option<T> NewOption<T>(T value) {
    return new Option<T>(
        value: value,
        valid: true
    );
}

public static Option<T> NewEmptyOption<T>() {
    T zero = GoZero<T>();
    return new Option<T>(
        value: zero,
        valid: false
    );
}

internal static void Main() {
    var optionPtr = Ꮡ(new Option<@string>(
        value: "hello"u8,
        valid: true
    ));
    fmt.Printf("Option value: %s, valid: %t\n"u8, (~optionPtr).value, (~optionPtr).valid);
    var options = new Option<nint>[]{
        NewOption<nint>(42),
        NewEmptyOption<nint>(),
        NewOption<nint>(100)
    }.slice();
    fmt.Printf("Options count: %d\n"u8, len(options));
    var cache = new map<@string, Option<float64>>{
        ["pi"u8] = NewOption<float64>(3.14159D),
        ["e"u8] = NewOption<float64>(2.71828D),
        ["phi"u8] = NewEmptyOption<float64>()
    };
    fmt.Printf("Cache size: %d\n"u8, len(cache));
    var resultChan = new channel<Option<bool>>(1);
    resultChan.ᐸꟷ(NewOption<bool>(true));
    var result = ᐸꟷ(resultChan);
    fmt.Printf("Channel result valid: %t\n"u8, result.valid);
}

} // end main_package

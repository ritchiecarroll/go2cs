namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Box<T> {
    public T Value;
}

public static any GetValue(nint index) {
    var values = new any[]{
        new Box<nint>(Value: 42),
        new Box<@string>(Value: "hello"u8),
        new Box<float64>(Value: 3.14D)
    }.slice();
    if (index >= 0 && index < len(values)) {
        return values[index];
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notAStringBoxˢ = (@string)"Not a string box"u8;
private static readonly object unknownBoxTypeˢ = (@string)"Unknown box type"u8;

internal static void Main() {
    var value = GetValue(0);
    {
        var (intBox, ok) = value._<Box<nint>>(ᐧ); if (ok) {
            fmt.Printf("Found an int box with value: %d\n"u8, intBox.Value);
        }
    }
    {
        var (strBox, ok) = value._<Box<@string>>(ᐧ); if (ok){
            fmt.Printf("Found a string box with value: %s\n"u8, strBox.Value);
        } else {
            fmt.Println(notAStringBoxˢ);
        }
    }
    var switchᴛ1 = GetValue(1);
    switch (switchᴛ1.type()) {
    case Box<nint> v: {
        fmt.Printf("Int box: %d\n"u8, v.Value);
        break;
    }
    case Box<@string> v: {
        fmt.Printf("String box: %s\n"u8, v.Value);
        break;
    }
    case Box<float64> v: {
        fmt.Printf("Float box: %f\n"u8, v.Value);
        break;
    }
    default: {
        var v = switchᴛ1;
        fmt.Println(unknownBoxTypeˢ);
        break;
    }}
}

} // end main_package

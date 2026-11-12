namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Box<T> {
    public T Value;
}

public static any GetValue(nint index) {
    var values = slice<any>([
        new Box<nint>(Value: 42),
        new Box<@string>(Value: "hello"),
        new Box<float64>(Value: 3.14D)
    ]);

    if (index >= 0 && index < len(values)) {
        return values[index];
    }
    return default;
}

internal static void main() {
    var value = GetValue(0);

    if (value._<Box<nint>>(out var intBox)) {
        fmt.Printf("Found an int box with value: %d\n"u8, intBox.Value);
    }

    if (value._<Box<@string>>(out var strBox)) {
        fmt.Printf("Found a string box with value: %s\n"u8, strBox.Value);
    } else {
        fmt.Println("Not a string box");
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
        fmt.Println("Unknown box type");
        break;
    }}
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ValueType {
    internal @string unit;
}

internal static @string describe(this ValueType v) {
    return v.unit;
}

partial interface message {
    @string describe();
}

internal static void show(message m) {
    fmt.Println(m.describe());
}

internal static void Main() {
    message m = new ValueType(unit: "cpu in nanoseconds"u8);
    show(m);
    ref var v = ref heap<ValueType>(out var Ꮡv);
    v = new ValueType(unit: "heap in bytes"u8);
    message mp = new ValueTypeжmessage(Ꮡv);
    show(mp);
    fmt.Println(mp.describe());
    show(new ValueType(unit: "v1.4"u8));
}

} // end main_package

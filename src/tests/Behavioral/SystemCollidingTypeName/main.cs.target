namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct ValueType {
    internal @string unit;
}

} partial class mainᴛValueType {

internal static @string describe(this ValueType v) { return v.unit; }

} partial class main_package {

partial interface message {
    @string describe();
}

internal static void show(message m) { fmt.Println(m.describe()); }

internal static void main() {
    message m = new ValueType(unit: "cpu in nanoseconds");
    show(m);

    ref var v = ref heap<ValueType>(new ValueType(unit: "heap in bytes"), out var Ꮡv);
    message mp = new ValueTypeжmessage(Ꮡv);
    show(mp);
    fmt.Println(mp.describe());

    show(new ValueType(unit: "v1.4"));
}

} // end main_package

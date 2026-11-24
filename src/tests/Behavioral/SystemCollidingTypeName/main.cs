namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ValueType {
    internal @string unit;
}

internal static @string describe(this ValueType v) { return v.unit; }

partial interface message {
    @string describe();
}

internal static void show(message m) { fmt.Println(m.describe()); }

partial struct Int32 {
    internal int32 v;
}

public static ж<int32> Add(this ж<Int32> Ꮡx, int32 d) {
    ref var x = ref Ꮡx.DerefOrNull();

    x.v += d;
    return Ꮡx.Ꮡv;
}

internal static void main() {
    message m = new ValueType(unit: "cpu in nanoseconds");
    show(m);

    ref var v = ref heap<ValueType>(new ValueType(unit: "heap in bytes"), out var Ꮡv);
    message mp = new ValueTypeжmessage(Ꮡv);
    show(mp);
    fmt.Println(mp.describe());

    show(new ValueType(unit: "v1.4"));

    ref var counter = ref heap(new Int32(), out var Ꮡcounter);
    var p = Ꮡcounter.Add(5);
    p.Value += 1;
    fmt.Println(counter.v);
    fmt.Println(Ꮡcounter.Add(36).Value);
}

} // end main_package

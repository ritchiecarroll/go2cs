namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct @float {
    internal nint n, fracBits;
}

internal static void normalize(this ref @float f) { f.fracBits++; }

internal static @string String(this @float f) { return fmt.Sprintf("float(%d,%d)"u8, f.n, f.fracBits); }

internal static @float newFloat(nint n, nint fracBits) {
    var f = new @float(n: n, fracBits: fracBits);
    f.normalize();
    return f;
}

internal partial struct half_float /*num:float32*/;

internal static any half(float64 v) {
    return ((half_float)(float32)v) / 2F;
}

internal static void main() {
    var f = newFloat(3, 4);
    fmt.Println(f.n, f.fracBits);

    var p = Ꮡ(new @float(1, 2));
    p.normalize();
    var fs = slice<@float>([new(5, 6), new(n: 7)]);
    var m = new map<@string, @float>{["a"] = new(8, 9)};

    any x = f;
    var (g, ok) = x._<@float>(ᐧ);
    @float z = default;
    fmt.Println(p.Value, fs, m["a"], g, ok, z);
    fmt.Printf("%v %T\n"u8, half(5D), half(5D));
}

} // end main_package

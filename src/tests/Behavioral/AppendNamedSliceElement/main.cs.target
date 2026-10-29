namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Inner /*[]nint*/;

partial struct Outer /*[]Inner*/;

internal static Outer build() {
    Outer o = default;
    var s = slice<nint>([1, 2]);
    o = append(o, (Inner)(s));
    o = append(o, (Inner)(slice<nint>([3, 4, 5])));
    Inner inner = slice<nint>([6]);
    o = append(o, inner);
    return o;
}

internal static void Main() {
    var o = build();
    fmt.Println(len(o));
    foreach (var (_, inner) in o) {
        fmt.Println(inner);
    }
}

} // end main_package

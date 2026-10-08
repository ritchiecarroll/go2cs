namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Inner /*[]nint*/;

partial struct Outer /*[]Inner*/;

internal static Outer build() {
    Outer o = default!;
    var s = new nint[]{1, 2}.slice();
    o = append(o, (Inner)(s));
    o = append(o, (Inner)(new nint[]{3, 4, 5}.slice()));
    Inner inner = new nint[]{6}.slice();
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

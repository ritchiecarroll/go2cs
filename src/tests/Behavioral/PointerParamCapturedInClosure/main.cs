namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct config {
    internal nint size;
    internal nint tag;
}

internal static Func<nint, nint> scaler(ж<config> Ꮡc) {
    return (nint x) => x * Ꮡc.Value.size + Ꮡc.Value.tag;
}

internal static Action mutate(ж<config> Ꮡc) {
    return () => {
        Ꮡc.Value.size++;
    };
}

internal static void Main() {
    var c = Ꮡ(new config(size: 10, tag: 3));
    var f = scaler(c);
    fmt.Println(f(5));
    var inc = mutate(c);
    inc();
    inc();
    fmt.Println((~c).size);
    fmt.Println(f(2));
}

} // end main_package

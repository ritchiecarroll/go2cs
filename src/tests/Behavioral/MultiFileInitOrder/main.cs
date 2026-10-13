namespace go;

using fmt = fmt_package;

partial class main_package {

[GoInit] internal static void initΔ4() {
    order = append(order, "main#1"u8);
}

internal static void Main() {
    fmt.Println(len(order));

    foreach (var (_, name) in order) {
        fmt.Println(name);
    }
}

} // end main_package

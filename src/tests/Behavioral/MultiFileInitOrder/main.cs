namespace go;

using fmt = fmt_package;

partial class main_package {

[GoInit] internal static void initΔ4() {
    order = append(order, (@string)"main#1");
}

internal static void main() {
    fmt.Println(len(order));

    foreach (var (_, name) in range(order)) {
        fmt.Println(name);
    }
}

} // end main_package

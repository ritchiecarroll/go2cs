namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    nint size = 5;
    slice<nint> s = default;
    foreach (var i in range(size)) {
        s = append(s, i);
    }
    fmt.Println(s);
}

} // end main_package

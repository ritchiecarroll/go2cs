namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal nint mark;
    internal nint other;
}

internal static void Main() {
    var a = new holder(mark: 1, other: 2);
    var b = new holder(mark: 1, other: 2);
    var c = new holder(mark: 1, other: 3);
    fmt.Println(a == b);
    fmt.Println(a == c);
    fmt.Println(b == c);
}

} // end main_package

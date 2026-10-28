namespace go;

using fmt = fmt_package;

partial class main_package {

internal static bool /*_*/ valid(bool flag) {
    if (flag) {
        return true;
    }
    return default;
}

internal static (nint, @string label) pair(@string tag) {
    @string label = default;

    label = "empty:"u8 + tag;
    if (tag == "set"u8) {
        return (7, "set");
    }
    return (default, label);
}

internal static void Main() {
    fmt.Println(valid(true), valid(false));
    var (n, label) = pair("base");
    fmt.Println(n, label);
    (n, label) = pair("set");
    fmt.Println(n, label);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

public partial interface describer {
    @string Describe();
}

partial struct Label {
    internal @string text;
}

} partial class mainᴛLabel {

public static @string Describe(this Label l) {
    return "label:"u8 + l.text;
}

} partial class main_package {

public static @string Show(describer d) {
    return d.Describe();
}

internal static void main() {
    fmt.Println(Show(new Label(text: "A")));
    fmt.Println(Show(new Label(text: "B")));
}

} // end main_package

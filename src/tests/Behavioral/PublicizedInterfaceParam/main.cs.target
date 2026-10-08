namespace go;

using fmt = fmt_package;

partial class main_package {

public partial interface describer {
    @string Describe();
}

partial struct Label {
    internal @string text;
}

public static @string Describe(this Label l) {
    return "label:"u8 + l.text;
}

public static @string Show(describer d) {
    return d.Describe();
}

internal static void Main() {
    fmt.Println(Show(new Label(text: "A"u8)));
    fmt.Println(Show(new Label(text: "B"u8)));
}

} // end main_package

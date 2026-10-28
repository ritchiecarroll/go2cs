namespace go;

using fmt = fmt_package;

partial class main_package {

public static UntypedInt Width => 8;

internal static @string classify(uintptr n) {
    switch (n) {
    case var ᴛ when ᴛ == Width: {
        return "width";
    }
    default: {
        return "other";
    }}

}

internal static @string matchVar(uintptr p, uintptr a, uintptr b) {
    switch (p) {
    case var ᴛ when ᴛ == a: {
        return "A";
    }
    case var ᴛ when ᴛ == b: {
        return "B";
    }
    default: {
        return "?";
    }}

}

internal static void Main() {
    fmt.Println(classify(8));
    fmt.Println(classify(3));
    fmt.Println(matchVar(1, 1, 2));
    fmt.Println(matchVar(2, 1, 2));
    fmt.Println(matchVar(9, 1, 2));
}

} // end main_package

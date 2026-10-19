namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct version /*@string*/;

internal static bool isValid(this version v) {
    return v != ""u8;
}

internal static @string tag(this version v) {
    return ((@string)v) + "!"u8;
}

internal static version asVersion(@string s) {
    return ((version)s);
}

internal static void mutate(ref @string s) {
    s = s + "-mutated"u8;
}

internal static void Main() {
    version fileVersion = asVersion("go1.21");
    if (fileVersion.isValid()) {
        fmt.Println(fileVersion.tag());
    }

    @string cause = "";
    mutate(ref cause);
    fmt.Println("cause:" + cause);

    @string label = "x";
    for (nint i = 0; i < 2; i++) {
        label = label + "y"u8;
    }
    fmt.Println(label);
}

} // end main_package

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string go121ˢ = "go1.21"u8;

internal static void Main() {
    version fileVersion = asVersion(go121ˢ);
    if (fileVersion.isValid()) {
        fmt.Println(fileVersion.tag());
    }
    @string cause = ""u8;
    mutate(ref cause);
    fmt.Println("cause:" + cause);
    @string label = "x"u8;
    for (nint i = 0; i < 2; i++) {
        label = label + "y"u8;
    }
    fmt.Println(label);
}

} // end main_package

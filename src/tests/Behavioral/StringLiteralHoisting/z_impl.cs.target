namespace go;

partial class main_package {

internal static @string describe() {
    return "derived through a later file";
}

internal static @string reuseShared() {
    return "shared across files";
}

[GoInit] internal static void init() {
    if (reuseShared() != "shared across files"u8) {
        throw panic("init literal must not be hoisted");
    }
}

} // end main_package

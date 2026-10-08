namespace go;

partial class main_package {

internal static @string pkgLevelLiteral = "package level literal stays inline"u8;

internal static @string derivedAtInit;
internal static void initᴛderivedAtInit() { derivedAtInit = describe(); }

partial struct box {
    internal @string name;
    internal @string tag;
}

partial struct label /*@string*/;

} // end main_package

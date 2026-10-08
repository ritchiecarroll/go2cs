namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct version /*@string*/;

internal static @string tag(this version v) {
    return ((@string)v) + "!"u8;
}

internal static version bump(version v) {
    return v + "-next"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string go121ˢ = "go1.21"u8;

internal partial struct main_rec /*dyn*/ {
    internal version v;
}

internal static void Main() {
    version @base = ((version)(@string)go121ˢ);
    version @short = @base + "-rc"u8;
    version @explicit = @base + "-beta"u8;
    version viaCall = bump(@base);
    version acc = @base;
    acc += "-acc"u8;
    version joined = @short + ((version)(@string)"/x"u8);
    var typed = new version[]{@base + "-t1"u8, @base + "-t2"u8}.slice();
    var elided = new slice<version>[]{new version[]{@base + "-e1"u8}.slice()}.slice();
    var inMap = new map<@string, version>{["k"u8] = @base + "-m"u8};
    var inStruct = new main_rec(@base + "-s"u8);
    fmt.Println(@short, @explicit, viaCall, acc, joined);
    fmt.Println(typed, elided, inMap["k"u8], inStruct.v);
    fmt.Println(@short.tag(), joined.tag());
}

} // end main_package

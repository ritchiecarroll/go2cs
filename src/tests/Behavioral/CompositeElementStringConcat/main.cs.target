namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> take(slice<@string> s) {
    return s;
}

internal partial struct main_rec /*dyn*/ {
    internal @string a, b;
}

internal static void Main() {
    @string prefix = "go"u8;
    var typedSlice = new @string[]{prefix + "-a"u8, prefix + "-b"u8}.slice();
    var typedArray = new @string[]{prefix + "-c"u8, prefix + "-d"u8}.array();
    var elided = new slice<@string>[]{new @string[]{prefix + "-e"u8}.slice()}.slice();
    var parens = new @string[]{(prefix + "-f"u8)}.slice();
    var bare = new @string[]{"lit"u8, "x"u8 + "y"u8}.slice();
    var viaCall = take(new @string[]{prefix + "-g"u8}.slice());
    var inMap = new map<@string, @string>{["k"u8] = prefix + "-h"u8};
    var inStruct = new main_rec(prefix + "-i"u8, "plain"u8);
    fmt.Println(typedSlice, typedArray, elided, parens, bare, viaCall);
    fmt.Println(inMap["k"u8], inStruct.a, inStruct.b);
    fmt.Println(prefix + "-j");
}

} // end main_package

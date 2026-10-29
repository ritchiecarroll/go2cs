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
    @string prefix = "go";

    var typedSlice = slice<@string>([prefix + "-a"u8, prefix + "-b"u8]);

    var typedArray = new @string[]{prefix + "-c"u8, prefix + "-d"u8}.array();

    var elided = slice<slice<@string>>([slice<@string>([prefix + "-e"u8])]);

    var parens = slice<@string>([(prefix + "-f"u8)]);

    var bare = slice<@string>(["lit", "x"u8 + "y"u8]);

    var viaCall = take(slice<@string>([prefix + "-g"u8]));

    var inMap = new map<@string, @string>{["k"] = prefix + "-h"u8};
    var inStruct = new main_rec(prefix + "-i"u8, "plain");

    fmt.Println(typedSlice, typedArray, elided, parens, bare, viaCall);
    fmt.Println(inMap["k"], inStruct.a, inStruct.b);

    fmt.Println(prefix + "-j");
}

} // end main_package

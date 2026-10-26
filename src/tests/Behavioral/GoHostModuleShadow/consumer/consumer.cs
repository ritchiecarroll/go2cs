namespace go.GoHostModuleShadow;

using runtime = runtime_package;
using debug = global::go.runtime.debug_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using lib = global::go.go.shadowlib.@in.lib_package;
using global::go.go.shadowlib.@in;
using global::go.runtime;
using global::go.unicode;

partial class consumer_package {

public static @string Describe(@string s) {
    nint upper = 0;
    foreach (var (_, r) in s) {
        if (unicode.IsUpper(r)) {
            upper++;
        }
    }
    _ = debug.SetGCPercent(debug.SetGCPercent(100));
    return lib.Greeting(s) + " runes="u8 + itoa(utf8.RuneCountInString(s)) + " upper="u8 + itoa(upper) + " gomaxprocs>0="u8 + bool2s(runtime.GOMAXPROCS(0) > 0);
}

internal static @string itoa(nint n) {
    if (n == 0) {
        return "0";
    }
    @string digits = "";
    for (; n > 0; n /= 10) {
        digits = ((@string)(rune)((rune)'0' + n % 10)) + digits;
    }
    return digits;
}

internal static @string bool2s(bool b) {
    if (b) {
        return "true";
    }
    return "false";
}

} // end consumer_package

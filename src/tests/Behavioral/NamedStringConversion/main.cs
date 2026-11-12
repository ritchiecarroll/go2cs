namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct errorString /*@string*/;

} partial class mainᴛerrorString {

internal static @string Error(this errorString e) { return "err: "u8 + ((@string)e); }

} partial class main_package {

partial struct label /*@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string kaboomˢ = "kaboom"u8;
private static readonly @string tagˢ = "tag"u8;
private static readonly @string jsonOmitemptyˢ = "json,omitempty"u8;

internal static void main() {
    error e = ((errorString)(@string)kaboomˢ);
    fmt.Println(e.Error());

    label l = (label)(@string)tagˢ;
    fmt.Println(l, len(l));

    label st = (label)(@string)jsonOmitemptyˢ;
    fmt.Println(st[0], st[4]);
    label name = st[0..4];
    fmt.Println(name, name != ""u8, name == "json"u8);
}

} // end main_package

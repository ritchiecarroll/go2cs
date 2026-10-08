namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Values /*map[@string, slice<@string>]*/;

public static @string Get(this Values v, @string key) {
    {
        var vs = v[key]; if (len(vs) > 0) {
            return vs[0];
        }
    }
    return ""u8;
}

public static void Add(this Values v, @string key, @string value) {
    v[key] = append(v[key], value);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string colorˢ = "color"u8;
private static readonly @string redˢ = "red"u8;
private static readonly @string blueˢ = "blue"u8;

internal static void Main() {
    var v = new Values(new map<@string, slice<@string>>{});
    v.Add(colorˢ, redˢ);
    v.Add(colorˢ, blueˢ);
    fmt.Println(v.Get(colorˢ));
    fmt.Println(len(v[colorˢ]));
    fmt.Println(len(v));
}

} // end main_package

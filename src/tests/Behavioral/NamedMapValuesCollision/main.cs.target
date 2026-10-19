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
    return "";
}

public static void Add(this Values v, @string key, @string value) {
    v[key] = append(v[key], value);
}

internal static void Main() {
    var v = new Values(new map<@string, slice<@string>>{});
    v.Add("color", "red");
    v.Add("color", "blue");
    fmt.Println(v.Get("color"));
    fmt.Println(len(v["color"]));
    fmt.Println(len(v));
}

} // end main_package

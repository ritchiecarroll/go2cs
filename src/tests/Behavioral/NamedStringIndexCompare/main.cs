namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Kind /*@string*/;

partial struct S {
    public Kind K;
}

internal static Kind get() { return (@string)"x"; }

internal static void Main() {
    var vals = new map<@string, Kind>{["a"] = (@string)"x", ["b"] = (@string)"y"};
    var s = slice<Kind>([(@string)"x", (@string)"y"]);
    var a = new Kind[]{(@string)"x", (@string)"y"}.array();
    ref var k = ref heap(new Kind(), out var Ꮡk);
    k = (@string)"xy";

    fmt.Println(vals["a"] == "x", vals["a"] != "y", "x" == vals["a"], "y" != vals["b"]);
    fmt.Println(s[0] == "x", s[1] != "x", a[0] == "x", a[1] == "y", ("x") == (s[0]));
    fmt.Println(vals["a"] < "y", vals["b"] <= "x", vals["b"] > "x", vals["a"] >= "x", "a" < s[0]);
    fmt.Println(vals["missing"] == "", s[0] == "x" && a[1] == "y");

    fmt.Println(s[0] + "z"u8 == "xz", vals["a"] + "z"u8 != "xz", "xz" == a[0] + "z"u8, k[1..] + "!"u8 == "y!");

    fmt.Println(k[1..] == "y", k[..1] != "x", k[0..2] == "xy", "y" == k[1..], k[1..] > "a");

    ref var st = ref heap<S>(new S((@string)"x"), out var Ꮡst);
    var p = Ꮡst;
    var ps = new map<@string, ж<Kind>>{["k"] = Ꮡk};
    fmt.Println(k == "xy"u8, st.K == "x"u8, p.K == "x"u8, get() == "x"u8, ps["k"].Value == "xy"u8);
    fmt.Println(vals["a"] == ((Kind)(@string)"x"), ((@string)vals["a"]) == "x"u8, vals["a"] + "z"u8);
    var exprᴛ1 = vals["a"];
    switch (ᐧ) {
    case {} when exprᴛ1 == (Kind)("x"u8): {
        fmt.Println("switch x");
        break;
    }
    default: {
        fmt.Println("switch other");
        break;
    }}

    var plain = slice<@string>(["x", "y"]);
    var pm = new map<@string, @string>{["a"] = "x"};
    fmt.Println(plain[0] == "x", pm["a"] != "x", plain[1][0..1] == "y");
}

} // end main_package

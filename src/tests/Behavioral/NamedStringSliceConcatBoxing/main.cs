namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using encoding;

partial class main_package {

partial struct Token /*@string*/;

public static @string Kind(this Token t) { return "token:"u8 + ((@string)t); }

internal static any negate(any v) {
    switch (v.type()) {
    case json.Number vΔ1: {
        if (vΔ1[0] == '-') {
            return vΔ1[1..];
        }
        return "-"u8 + vΔ1;
    }}
    return v;
}

internal static @string describe(any v) {
    switch (v.type()) {
    case json.Number vΔ1: {
        return "number "u8 + vΔ1.String();
    }
    case Token vΔ1: {
        return vΔ1.Kind();
    }
    case @string vΔ1: {
        return "string "u8 + vΔ1;
    }}
    return fmt.Sprintf("other %T"u8, v);
}

internal static void show(@string label, any v) {
    fmt.Printf("%s: %T %v -> %s\n"u8, label, v, v, describe(v));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcdefˢ = "abcdef"u8;

internal static void Main() {
    var itemsᴷ = new stackAppend<any>();

    foreach (var (_, v) in range(slice<any>([(json.Number)(@string)"1", (json.Number)(@string)"-2.5"]))) {
        show("negate", negate(v));
    }

    Token t = (Token)(@string)abcdefˢ;
    show("t[1:]", t[1..]);
    show("t[:2]", t[..2]);
    show("t[1:3]", t[1..3]);
    show("t[:]", t[..]);
    show("t + \"!\"", t + "!"u8);
    show("\"<\" + t", "<"u8 + t);
    show("t + t", t + t);

    any boxed = t[2..];
    show("var", boxed);

    var items = slice<any>([t[3..], "x"u8 + t]);
    items = append(itemsᴷ, items, (any)(t[..1] + "y"u8));
    foreach (var (i, item) in range(items)) {
        show(fmt.Sprint("item ", i), item);
    }

    var m = new map<@string, any>{["k"] = t[4..]};
    show("map", m["k"]);

    fmt.Println(t[1..].Kind(), ("["u8 + t + "]"u8).Kind());

    @string s = "plain";
    show("s[1:]", s[1..]);
    show("s + t-string", s + ((@string)t));
}

} // end main_package

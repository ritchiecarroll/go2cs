namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal any inner;
}

partial struct pair {
    internal @string label;
    internal any value;
}

internal static void describe(@string prefix, any v) {
    switch (v.type()) {
    case @string s: {
        fmt.Println(prefix, "string:", s);
        break;
    }
    default: {
        var s = v;
        fmt.Println(prefix, "other:", s);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valˢ = (@string)"val"u8;

internal static void main() {
    var n = Ꮡ(new node(inner: (@string)"hi"));
    var p = new pair("tag", valˢ);
    var m = new map<@string, any>{["k"] = (@string)"mv"};
    var mk = new map<any, nint>{[(@string)"ky"] = 7};
    var s = slice<any>([(@string)"a", (@string)"b"]);
    var arr = array<any>([(@string)"x", (@string)"y"]);
    var el = slice<node>([new(inner: (@string)"e1")]);
    var ep = slice<pair>([new("e2", (@string)"e3")]);
    var pp = slice<ж<node>>([Ꮡ(new node(inner: (@string)"p1"))]);
    var pq = slice<ж<pair>>([Ꮡ(new pair("q1", (@string)"q2"))]);
    var sp = new array<any>(3){[1] = (@string)"sp"};

    describe("keyed", n.inner);
    fmt.Println("label:", p.label);
    describe("positional", p.value);
    describe("mapval", m["k"]);

    foreach (var (k, v) in mk) {
        describe("mapkey", k);
        fmt.Println("mapkeyval:", v);
    }

    describe("slice0", s[0]);
    describe("slice1", s[1]);
    describe("array0", arr[0]);
    describe("array1", arr[1]);
    describe("elided", el[0].inner);
    fmt.Println("elidedpos label:", ep[0].label);
    describe("elidedpos", ep[0].value);
    describe("ptrelided", pp[0].inner);
    fmt.Println("ptrelidedpos label:", pq[0].label);
    describe("ptrelidedpos", pq[0].value);
    describe("sparse0", sp[0]);
    describe("sparse1", sp[1]);
}

} // end main_package

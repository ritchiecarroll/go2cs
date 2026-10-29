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

internal static void Main() {
    var n = Ꮡ(new node(inner: (@string)"hi"u8));
    var p = new pair("tag", (@string)"val"u8);
    var m = new map<@string, any>{["k"] = (@string)"mv"u8};
    var mk = new map<any, nint>{[(@string)"ky"u8] = 7};
    var s = slice<any>([(@string)"a"u8, (@string)"b"u8]);
    var arr = new any[]{(@string)"x"u8, (@string)"y"u8}.array();
    var el = slice<node>([new(inner: (@string)"e1"u8)]);
    var ep = slice<pair>([new("e2", (@string)"e3"u8)]);
    var pp = slice<ж<node>>([Ꮡ(new node(inner: (@string)"p1"u8))]);
    var pq = slice<ж<pair>>([Ꮡ(new pair("q1"u8, (@string)"q2"u8))]);
    var sp = new array<any>(3){[1] = (@string)"sp"u8};

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

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("dyn")] internal partial struct main_Point {
    public nint X, Y;
}

[GoLocalName("Points")] [GoType("[]main_Point")] internal partial struct main_Points;

[GoLocalName("Tally")] [GoType("map[@string, nint]")] internal partial struct main_Tally;

[GoLocalName("Stream")] [GoType("chan nint")] internal partial struct main_Stream;

[GoLocalName("Triple")] [GoType("[3]nint")] internal partial struct main_Triple;

[GoType("dyn")] internal partial struct main_Node {
    public nint V;
}

[GoLocalName("NodePtr")] [GoType("ж<main_Node>")] internal partial class main_NodePtr;

[GoLocalName("recursiveSlice")] [GoType("[]main_recursiveSlice")] internal partial struct main_recursiveSlice;

[GoLocalName("recursiveMap")] [GoType("map[@string, main_recursiveMap]")] internal partial struct main_recursiveMap;

internal static void Main() {
    var pts = new main_Points(new main_Point[]{new(1, 2), new(3, 4), new(5, 6)}.slice());
    foreach (var (_, p) in pts) {
        fmt.Println(p.X, p.Y);
    }
    var tally = new main_Tally(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    fmt.Println(tally["a"u8] + tally["b"u8]);
    var stream = new main_Stream(1);
    stream.ᐸꟷ(7);
    fmt.Println(ᐸꟷ<nint>(stream));
    var triple = new main_Triple(new nint[]{10, 20, 30}.array());
    nint sum = 0;
    foreach (var (_, n) in triple.ΔRangeSnapshot()) {
        sum += n;
    }
    main_NodePtr np = Ꮡ(new main_Node(V: 9));
    fmt.Println(((ж<main_Node>)(np)).Value.V);
    var rs = new main_recursiveSlice(new main_recursiveSlice[]{new main_recursiveSlice(new main_recursiveSlice[]{default!}.slice()), default!}.slice());
    fmt.Println(len(rs), len(rs[0]));
    var rm = new main_recursiveMap(new map<@string, main_recursiveMap>{["a"u8] = new main_recursiveMap(new map<@string, main_recursiveMap>{["b"u8] = default!})});
    fmt.Println(len(rm), len(rm["a"u8]));
    fmt.Println(sum);
}

} // end main_package

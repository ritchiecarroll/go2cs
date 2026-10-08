namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint x;
    internal nint y;
}

internal static void Main() {
    ref var v = ref heap<node>(out var Ꮡv);
    v = new node(x: 5, y: 6);
    var node = Ꮡv;
    var px = node.of(main_package.node.Ꮡx);
    px.Value = 99;
    var py = node.of(main_package.node.Ꮡy);
    py.Value = 88;
    fmt.Println(v.x, v.y);
    fmt.Println((~node).x, (~node).y);
}

} // end main_package

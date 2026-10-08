namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct item {
    internal nint v;
}

partial struct Array {
    internal @string label;
}

partial struct queue /*[]ж<item>*/;

internal static void Main() {
    var q = new queue(new ж<item>[]{Ꮡ(new item(v: 1)), Ꮡ(new item(v: 2))}.slice());
    q = append(q, Ꮡ(new item(v: 3)));
    q[1].Value.v = 5;
    nint sum = 0;
    foreach (var (_, it) in q) {
        sum += it.Value.v;
    }
    fmt.Println(len(q), (~q[0]).v, (~q[1]).v, (~q[2]).v, sum);
}

} // end main_package

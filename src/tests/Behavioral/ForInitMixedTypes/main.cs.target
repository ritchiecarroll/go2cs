namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static void Main() {
    var third = Ꮡ(new node(val: 3));
    var second = Ꮡ(new node(val: 2, next: third));
    var first = Ꮡ(new node(val: 1, next: second));
    nint sum = 0;
    for ((nint i, var n) = (3, first); i > 0; (i, n) = (i - 1, n.Value.next)) {
        sum += n.Value.val;
    }
    fmt.Println(sum);
    var words = new @string[]{"a"u8, "b"u8, "c"u8}.slice();
    @string @out = ""u8;
    for ((nint i, @string s) = (0, ""); i < len(words); (i, s) = (i + 1, words[i])) {
        @out += s;
    }
    fmt.Println(@out);
    var total = 0.0D;
    for ((nint i, var f) = (0, 0.5D); i < 4; (i, f) = (i + 1, f * 2D)) {
        total += f;
    }
    fmt.Println(total);
}

} // end main_package

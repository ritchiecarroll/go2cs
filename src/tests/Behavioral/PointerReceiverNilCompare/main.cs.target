namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct box {
    internal nint val;
}

} partial class mainᴛbox {

internal static bool isNil(this ж<box> Ꮡb) { return Ꮡb == nil; }
internal static bool notNil(this ж<box> Ꮡb) { return Ꮡb != nil; }

internal static bool same(this ж<box> Ꮡb, ж<box> other) { return Ꮡb == other; }

} partial class main_package {

partial struct embedder {
    /*embed*/ internal box box;
    internal nint tag;
}

} partial class mainᴛembedder {

internal static bool isNil(this ж<embedder> Ꮡe) { return Ꮡe == nil; }

internal static @string checkGuard(this ж<embedder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (Ꮡe == nil) {
        return "nil";
    }
    return fmt.Sprintf("val=%d tag=%d"u8, e.val, e.tag);
}

} partial class mainᴛbox {

internal static @string describe(this ж<box> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil) {
        return "<nil>";
    }
    return fmt.Sprintf("box(%d)"u8, b.val);
}

} partial class main_package {

internal static void main() {
    var b = Ꮡ(new box());
    fmt.Println(b.isNil(), b.notNil());
    fmt.Println(b.same(b), b.same(Ꮡ(new box())));

    var e = Ꮡ(new embedder());
    e.val = 7;
    e.tag = 9;
    fmt.Println(e.isNil(), e.checkGuard());

    ж<box> np = default;
    fmt.Println(np.isNil(), np.notNil(), np.describe());
    fmt.Println(b.describe());

    ж<embedder> ne = default;
    fmt.Println(ne.isNil(), ne.checkGuard());
}

} // end main_package

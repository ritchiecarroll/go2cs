namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct thing {
    internal nint n;
}

internal static bool Equal(this ref thing t, any x) {
    var (other, ok) = x._<ж<thing>>(ᐧ);
    return ok && (~other).n == t.n;
}

internal static any Public(this ref thing t) {
    return t.n;
}

internal static @string Label(this ref thing t) {
    return fmt.Sprintf("thing(%d)"u8, t.n);
}


partial interface _ᴛ1 /*dyn*/ {
    bool Equal(any x);
}
internal static _ᴛ1 _ᴛ1ʗ = new thingж_ᴛ1(Ꮡ(new thing(nil)));


partial interface _ᴛ2 /*dyn*/ {
    any Public();
    bool Equal(any x);
}
internal static _ᴛ2 _ᴛ2ʗ = new thingж_ᴛ2(Ꮡ(new thing(nil)));


partial interface labelerᴛ1 /*dyn*/ {
    @string Label();
}
internal static labelerᴛ1 labeler = new thingжlabelerᴛ1(Ꮡ(new thing(n: 41)));


partial struct originᴛ1 /*dyn*/ {
    internal nint x, y;
}
internal static originᴛ1 origin = new originᴛ1(x: 3, y: 4);

internal static void Main() {
    var a = Ꮡ(new thing(n: 7));
    var b = Ꮡ(new thing(n: 7));
    var c = Ꮡ(new thing(n: 8));
    fmt.Println(a.Equal(b.OrTypedNil()), a.Equal(c.OrTypedNil()), a.Public());
    fmt.Println(labeler.Label());
    fmt.Println(origin.x, origin.y);
    _ᴛ1 eq = new thingж_ᴛ1(a);
    fmt.Println(eq.Equal(b.OrTypedNil()), eq.Equal(c.OrTypedNil()));
}

} // end main_package

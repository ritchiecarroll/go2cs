namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct inner {
    internal int64 a;
    internal int64 b;
}

partial struct wrapper /*inner*/;

partial struct box {
    internal wrapper w;
}

internal static void fill(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = Ꮡb.of(box.Ꮡw);
    c.Value.a = 10;
    c.Value.b = 20;
}

internal static int64 readBack(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = Ꮡb.of(box.Ꮡw);
    return (~c).a + (~c).b;
}

internal static partial void bump(ref int64 p) {
    p = p + 7;
}

partial struct @base {
    internal int64 id;
}

internal static int64 twice(this @base b) {
    return b.id * 2;
}

partial struct derived {
    internal partial ref @base @base { get; }
    internal int64 tag;
}

partial struct @file {
    internal int64 fd;
}

internal static int64 bump(this ref @file f) {
    f.fd += 2;
    return f.fd;
}

partial struct mixed {
    public nint Pub;
    internal nint sec;
}

internal static void Main() {
    var mx = new mixed(Pub: 3);
    mx.sec = 4;
    fmt.Println(mx.Pub + mx.sec);
    ref var b = ref heap(new box(), out var Ꮡb);
    fill(Ꮡb);
    fmt.Println(b.w.a, b.w.b);
    fmt.Println(readBack(Ꮡb));
    var c = Ꮡb.of(box.Ꮡw);
    bump(ref (c.of(wrapper.Ꮡa)).DerefOrNull());
    fmt.Println(b.w.a, readBack(Ꮡb));
    var s = new shadowed(nil);
    s.fn = 5;
    s.ctxt.fn = 7;
    s.tag = 9;
    fmt.Println(s.fn, s.ctxt.fn, s.tag);
    var d = new derived(nil);
    d.id = 5;
    d.tag = 1;
    fmt.Println(d.id, d.tag, d.twice());
    ref var e = ref heap<derived>(out var Ꮡe);
    e = new derived(@base: new @base(id: 21), tag: 2);
    var p = Ꮡe;
    bump(ref (p.of(derived.Ꮡid)).DerefOrNull());
    fmt.Println(e.id, e.tag, e.twice());
    shadowed z = new(nil);
    z.ctxt.fn = 7;
    z.tag = 3;
    fmt.Println(z.ctxt.fn, z.tag);
    var np = @new<shadowed>();
    np.Value.tag = 11;
    fmt.Println((~np).tag, (~np).ctxt.tag);
    var fl = Ꮡ(new @file(fd: 5));
    fmt.Println(fl.bump(), (~fl).fd);
}

partial struct ctxt {
    internal nint fn, tag;
}

partial struct shadowed {
    internal partial ref ctxt ctxt { get; }
    internal nint fn;
}

} // end main_package

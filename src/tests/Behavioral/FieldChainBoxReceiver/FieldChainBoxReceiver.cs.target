namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal int64 n;
}

internal static ж<counter> ptr(this ж<counter> Ꮡc) {
    return Ꮡc;
}

internal static void inc(this ж<counter> Ꮡc) {
    Ꮡc.ptr().Value.n++;
}

partial struct holder {
    internal counter c;
}

partial struct box {
    internal holder h;
}

internal static void viaParam(ж<box> Ꮡb) {
    Ꮡb.of(box.Ꮡh).of(holder.Ꮡc).inc();
}

partial struct wrapper {
    internal box b;
}

internal static ж<wrapper> self(this ж<wrapper> Ꮡw) {
    return Ꮡw;
}

internal static void bump(this ж<wrapper> Ꮡw) {
    _ = Ꮡw.self();
    Ꮡw.of(wrapper.Ꮡb).of(box.Ꮡh).of(holder.Ꮡc).inc();
}

partial struct mid {
    internal counter c;
}

partial struct deep {
    internal mid mid;
}

internal static void bumpDeep(this ж<deep> Ꮡd) {
    Ꮡd.of(deep.Ꮡmid).of(mid.Ꮡc).inc();
}

internal static void Main() {
    var b = Ꮡ(new box(nil));
    viaParam(b);
    viaParam(b);
    fmt.Println((~b).h.c.n);
    var w = Ꮡ(new wrapper(nil));
    w.bump();
    w.bump();
    w.bump();
    fmt.Println((~w).b.h.c.n);
    var d = Ꮡ(new deep(nil));
    d.bumpDeep();
    d.bumpDeep();
    d.bumpDeep();
    d.bumpDeep();
    fmt.Println((~d).mid.c.n);
}

} // end main_package

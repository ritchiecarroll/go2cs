namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal uint32 v;
}

partial struct outer {
    internal inner @in;
}

partial struct core {
    internal uint32 sema;
}

partial struct holder {
    internal core c;
}

partial struct wrapper {
    /*embed*/ internal ж<holder> holder;
}

internal static void Main() {
    var o = Ꮡ(new outer());
    fmt.Println("depth1:", o.of(outer.Ꮡin) == o.of(outer.Ꮡin));
    fmt.Println("depth2:", o.of(outer.Ꮡin).of(inner.Ꮡv) == o.of(outer.Ꮡin).of(inner.Ꮡv));

    var m = new map<ж<uint32>, @string>{};
    m[o.of(outer.Ꮡin).of(inner.Ꮡv)] = "first";
    m[o.of(outer.Ꮡin).of(inner.Ꮡv)] = "second";
    fmt.Println("map entries:", len(m), m[o.of(outer.Ꮡin).of(inner.Ꮡv)]);

    var h = Ꮡ(new holder());
    var w = Ꮡ(new wrapper(holder: h));
    fmt.Println("promoted struct:", w.of(wrapper.Ꮡc) == h.of(holder.Ꮡc));
    fmt.Println("promoted field:", w.of(wrapper.Ꮡc).of(core.Ꮡsema) == h.of(holder.Ꮡc).of(core.Ꮡsema));

    var pm = new map<ж<uint32>, nint>{};
    pm[w.of(wrapper.Ꮡc).of(core.Ꮡsema)] = 1;
    pm[h.of(holder.Ꮡc).of(core.Ꮡsema)] = 2;
    fmt.Println("promoted map:", len(pm), pm[w.of(wrapper.Ꮡc).of(core.Ꮡsema)]);

    (w.of(wrapper.Ꮡc).of(core.Ꮡsema)).Value = 7;
    fmt.Println("shared storage:", (~h).c.sema, (~w).c.sema);
}

} // end main_package

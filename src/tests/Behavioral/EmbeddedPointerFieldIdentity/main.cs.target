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
    fmt.Println("depth1:", o.Ꮡin == o.Ꮡin);
    fmt.Println("depth2:", o.Ꮡin.Ꮡv == o.Ꮡin.Ꮡv);

    var m = new map<ж<uint32>, @string>{};
    m[o.Ꮡin.Ꮡv] = "first";
    m[o.Ꮡin.Ꮡv] = "second";
    fmt.Println("map entries:", len(m), m[o.Ꮡin.Ꮡv]);

    var h = Ꮡ(new holder());
    var w = Ꮡ(new wrapper(holder: h));
    fmt.Println("promoted struct:", w.of(wrapper.Ꮡc) == h.Ꮡc);
    fmt.Println("promoted field:", w.of(wrapper.Ꮡc).Ꮡsema == h.Ꮡc.Ꮡsema);

    var pm = new map<ж<uint32>, nint>{};
    pm[w.of(wrapper.Ꮡc).Ꮡsema] = 1;
    pm[h.Ꮡc.Ꮡsema] = 2;
    fmt.Println("promoted map:", len(pm), pm[w.of(wrapper.Ꮡc).Ꮡsema]);

    (w.of(wrapper.Ꮡc).Ꮡsema).Value = 7;
    fmt.Println("shared storage:", h.c.sema, w.c.sema);
}

} // end main_package

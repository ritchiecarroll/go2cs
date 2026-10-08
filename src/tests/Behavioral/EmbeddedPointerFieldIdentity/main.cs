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
    internal partial ref ж<holder> holder { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object depth1ˢ = (@string)"depth1:"u8;
private static readonly object depth2ˢ = (@string)"depth2:"u8;
private static readonly @string firstˢ = "first"u8;
private static readonly @string secondˢ = "second"u8;
private static readonly object mapEntriesˢ = (@string)"map entries:"u8;
private static readonly object promotedStructˢ = (@string)"promoted struct:"u8;
private static readonly object promotedFieldˢ = (@string)"promoted field:"u8;
private static readonly object promotedMapˢ = (@string)"promoted map:"u8;
private static readonly object sharedStorageˢ = (@string)"shared storage:"u8;

internal static void Main() {
    var o = Ꮡ(new outer(nil));
    fmt.Println(depth1ˢ, o.of(outer.Ꮡin) == o.of(outer.Ꮡin));
    fmt.Println(depth2ˢ, o.of(outer.Ꮡin).of(inner.Ꮡv) == o.of(outer.Ꮡin).of(inner.Ꮡv));
    var m = new map<ж<uint32>, @string>{};
    m[o.of(outer.Ꮡin).of(inner.Ꮡv)] = firstˢ;
    m[o.of(outer.Ꮡin).of(inner.Ꮡv)] = secondˢ;
    fmt.Println(mapEntriesˢ, len(m), m[o.of(outer.Ꮡin).of(inner.Ꮡv)]);
    var h = Ꮡ(new holder(nil));
    var w = Ꮡ(new wrapper(holder: h));
    fmt.Println(promotedStructˢ, w.of(wrapper.Ꮡc) == h.of(holder.Ꮡc));
    fmt.Println(promotedFieldˢ, w.of(wrapper.Ꮡc).of(core.Ꮡsema) == h.of(holder.Ꮡc).of(core.Ꮡsema));
    var pm = new map<ж<uint32>, nint>{};
    pm[w.of(wrapper.Ꮡc).of(core.Ꮡsema)] = 1;
    pm[h.of(holder.Ꮡc).of(core.Ꮡsema)] = 2;
    fmt.Println(promotedMapˢ, len(pm), pm[w.of(wrapper.Ꮡc).of(core.Ꮡsema)]);
    (w.of(wrapper.Ꮡc).of(core.Ꮡsema)).Value = 7;
    fmt.Println(sharedStorageˢ, (~h).c.sema, (~w).c.sema);
}

} // end main_package

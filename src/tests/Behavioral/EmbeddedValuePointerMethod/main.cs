namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct inner {
    internal nint n;
}

internal static void bump(this ref inner i, nint d) {
    i.n += d;
}

internal static void reset0(this ref inner i) {
    i.n = 0;
}

internal static nint total(this ref inner i) {
    return i.n;
}

partial struct outer {
    internal nint tag;
    internal partial ref inner inner { get; }
}

internal static partial void viaParam(ж<outer> Ꮡo) {
    Ꮡo.of(outer.Ꮡinner).bump(100);
}

partial struct flags /*num:uint8*/;

internal static void set(this ref flags f, flags bit) {
    f |= (flags)(bit);
}

internal static void clear(this ref flags f, flags bit) {
    f &= unchecked((flags)~(flags)(bit));
}

partial struct chunk {
    internal uint16 inUse;
    internal partial ref flags flags { get; }
}

internal static void alloc(this ref chunk c, uint16 n) {
    c.inUse += n;
    if (c.inUse >= 4) {
        c.flags.set(1);
    }
}

internal static void free(this ref chunk c, uint16 n) {
    c.inUse -= n;
    c.flags.clear(1);
}

internal static void bump(this ref chunk c, ж<chunk> Ꮡother) {
    {
        var cΔ1 = Ꮡother;
        cΔ1.of(chunk.Ꮡflags).set(2);
    }
}

internal static void Main() {
    var o = Ꮡ(new outer(tag: 7));
    o.of(outer.Ꮡinner).bump(5);
    o.of(outer.Ꮡinner).bump(3);
    viaParam(o);
    fmt.Println(o.of(outer.Ꮡinner).total());
    fmt.Println((~o).n);
    fmt.Println((~o).inner.n);
    fmt.Println((~o).tag);
    o.of(outer.Ꮡinner).reset0();
    fmt.Println((~o).n);
    var c = new chunk(nil);
    c.alloc(5);
    fmt.Println(c.inUse, (uint8)c.flags);
    c.free(2);
    fmt.Println(c.inUse, (uint8)c.flags);
    var d = new chunk(nil);
    ref var e = ref heap<chunk>(out var Ꮡe);
    e = new chunk(nil);
    d.bump(Ꮡe);
    fmt.Println((uint8)d.flags, (uint8)e.flags);
}

} // end main_package

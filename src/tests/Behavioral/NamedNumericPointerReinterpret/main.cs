namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial uint64 load64(ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    return p;
}

internal static partial uint32 load32(ж<uint32> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    return p;
}

partial struct lfstack /*num:uint64*/;

partial struct sweepClass /*num:uint32*/;

internal static uint64 peek(this ж<lfstack> Ꮡhead) {
    return load64(Ꮡhead.Reinterpret<lfstack, uint64>());
}

internal static uint32 peek(this ж<sweepClass> Ꮡsc) {
    return load32(Ꮡsc.Reinterpret<sweepClass, uint32>());
}

internal static uint64 peekVia(ж<lfstack> Ꮡp) {
    return load64(Ꮡp.Reinterpret<lfstack, uint64>());
}

partial struct hexval /*num:uint64*/;

internal static partial uint64 describe(hexval v) {
    return (uint64)v;
}

internal static partial void storeInt(ж<nint> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
}

partial struct gobber /*num:nint*/;

internal static void set(this ж<gobber> Ꮡg, nint v) {
    storeInt(Ꮡg.Reinterpret<gobber, nint>(), v);
}

partial struct coord {
    public nint X, Y;
}

partial struct point /*coord*/;

internal static partial void bump(ж<coord> Ꮡc) {
    var p = Ꮡc.Reinterpret<coord, point>();
    (p.Value.X, p.Value.Y) = (3, 4);
}

partial struct namedString /*@string*/;

internal static partial void setStr(ref namedString s, @string v) {
    s = ((namedString)v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string beforeˢ = "before"u8;
private static readonly @string afterˢ = "after"u8;

internal static void Main() {
    ref var a = ref heap(new lfstack(), out var Ꮡa);
    a = 0xDEADBEEF01UL;
    ref var b = ref heap(new sweepClass(), out var Ꮡb);
    b = 1234;
    fmt.Println(Ꮡa.peek(), Ꮡb.peek());
    fmt.Println(peekVia(Ꮡa));
    ref var c = ref heap(new lfstack(), out var Ꮡc);
    c = 42;
    fmt.Println(Ꮡc.peek());
    var h = ((hexval)(uint64)a);
    fmt.Println(describe(h), (uint64)((hexval)(uint64)c));
    ref var g = ref heap(new gobber(), out var Ꮡg);
    Ꮡg.set(23);
    fmt.Println(g);
    ref var xy = ref heap<coord>(out var Ꮡxy);
    xy = new coord(1, 2);
    bump(Ꮡxy);
    fmt.Println(xy.X, xy.Y);
    ref var s = ref heap<@string>(out var Ꮡs);
    s = beforeˢ;
    setStr(ref (Ꮡs.Reinterpret<@string, namedString>()).DerefOrNull(), afterˢ);
    fmt.Println(s);
    ref var d = ref heap(new lfstack(), out var Ꮡd);
    d = 7;
    storeInt64(ref (Ꮡd.Reinterpret<lfstack, uint64>()).DerefOrNull(), 99);
    fmt.Println(Ꮡd.peek(), d);
}

internal static partial void storeInt64(ref uint64 p, uint64 v) {
    p = v;
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct M /*map[nint, nint]*/;

partial class P /*ж<M>*/;

partial class PM /*ж<map<@string, nint>>*/;

partial class PS /*ж<slice<nint>>*/;

partial class PP /*ж<ж<nint>>*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedMapˢ = (@string)"named map:"u8;

internal static void namedMap() {
    ref var m = ref heap<M>(out var Ꮡm);
    m = new M(new map<nint, nint>{[1] = 1, [2] = 2});
    var p = new P(Ꮡm);
    (p.ValueSlot)[3] = 3;
    fmt.Println(namedMapˢ, len(p.ValueSlot), (p.ValueSlot)[3], len(m));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unnamedMapˢ = (@string)"unnamed map:"u8;

internal static void unnamedMap() {
    ref var m = ref heap<map<@string, nint>>(out var Ꮡm);
    m = new map<@string, nint>{["a"u8] = 1};
    var p = new PM(Ꮡm);
    (p.ValueSlot)["b"u8] = 2;
    fmt.Println(unnamedMapˢ, len(p.ValueSlot), (p.ValueSlot)["b"u8], len(m));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sliceˢ = (@string)"slice:"u8;

internal static void Δslice() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    s = new nint[]{1, 2, 3}.slice();
    var p = new PS(Ꮡs);
    p.ValueSlot = append(p.ValueSlot, (nint)(4));
    fmt.Println(sliceˢ, len(p.ValueSlot), (p.ValueSlot)[3], len(s));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pointerNilˢ = (@string)"pointer nil:"u8;
private static readonly object pointerSetˢ = (@string)"pointer set:"u8;

internal static void pointer() {
    ref var ip = ref heap<ж<nint>>(out var Ꮡip);
    var pp = new PP(Ꮡip);
    fmt.Println(pointerNilˢ, pp.ValueSlot == nil);
    ref var v = ref heap<nint>(out var Ꮡv);
    v = 7;
    pp.ValueSlot = Ꮡv;
    fmt.Println(pointerSetˢ, (pp.ValueSlot).Value, ip.Value);
}

internal static void Main() {
    namedMap();
    unnamedMap();
    Δslice();
    pointer();
}

} // end main_package

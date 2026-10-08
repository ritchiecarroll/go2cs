namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal ж<nint> p;
    internal slice<nint> s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pp1Pp2ˢ = (@string)"pp1==pp2"u8;
private static readonly object pp1Nilˢ = (@string)"pp1==nil"u8;
private static readonly object pp1Nilˢ2 = (@string)"*pp1==nil"u8;
private static readonly object pp1Pp2ˢ2 = (@string)"*pp1==*pp2"u8;
private static readonly object aliasPp1ˢ = (@string)"alias==pp1"u8;
private static readonly object p1Setˢ = (@string)"p1 set"u8;
private static readonly object keysˢ = (@string)"keys"u8;
private static readonly object beforeˢ = (@string)"before"u8;
private static readonly object afterˢ = (@string)"after"u8;
private static readonly object fieldNilˢ = (@string)"field nil"u8;
private static readonly object fieldKeyˢ = (@string)"field key"u8;
private static readonly object fieldSetˢ = (@string)"field set"u8;

internal static void Main() {
    ref var p1 = ref heap<ж<nint>>(out var Ꮡp1);
    p1 = ((ж<nint>)nil);
    ref var p2 = ref heap<ж<nint>>(out var Ꮡp2);
    p2 = ((ж<nint>)nil);
    var pp1 = Ꮡp1;
    var pp2 = Ꮡp2;
    fmt.Println(pp1Pp2ˢ, pp1 == pp2);
    fmt.Println(pp1Nilˢ, pp1 == nil);
    fmt.Println(pp1Nilˢ2, pp1.ValueSlot == nil);
    fmt.Println(pp1Pp2ˢ2, pp1.ValueSlot == pp2.ValueSlot);
    var alias = pp1;
    fmt.Println(aliasPp1ˢ, alias == pp1);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 42;
    alias.ValueSlot = Ꮡn;
    fmt.Println(p1Setˢ, pp1.ValueSlot != nil, (pp1.ValueSlot).Value);
    ref var v = ref heap<nint>(out var Ꮡv);
    v = 5;
    ref var c = ref heap<ж<nint>>(out var Ꮡc);
    c = Ꮡv;
    ref var d = ref heap<ж<nint>>(out var Ꮡd);
    d = Ꮡv;
    fmt.Println((@string)"c==d"u8, c == d, (@string)"&c==&d"u8, Ꮡc == Ꮡd);
    var keys = new map<ж<ж<nint>>, @string>{[Ꮡc] = "c"u8, [Ꮡd] = "d"u8};
    fmt.Println(keysˢ, len(keys), keys[Ꮡc], keys[Ꮡd]);
    ref var late = ref heap<ж<nint>>(out var Ꮡlate);
    var lp = Ꮡlate;
    var stable = new map<ж<ж<nint>>, @string>{[lp] = "stable"u8};
    fmt.Println(beforeˢ, stable[lp]);
    late = Ꮡv;
    fmt.Println(afterˢ, stable[lp], len(stable), (lp.ValueSlot).Value);
    ref var h = ref heap(new holder(), out var Ꮡh);
    var fp = Ꮡh.of(holder.Ꮡp);
    var fs = Ꮡh.of(holder.Ꮡs);
    fmt.Println(fieldNilˢ, fp.ValueSlot == nil, fs.ValueSlot == default!, len(fs.ValueSlot));
    var fields = new map<ж<ж<nint>>, @string>{[fp] = "h.p"u8};
    fmt.Println(fieldKeyˢ, fields[fp]);
    h.p = Ꮡv;
    fmt.Println(fieldSetˢ, fields[fp], (fp.ValueSlot).Value, fp.ValueSlot == c);
}

} // end main_package

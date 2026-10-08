namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct frame {
    public @string Name;
    public bool Inlined;
}

internal static @string label(this frame f) {
    return f.Name;
}

internal static void bump(this ref frame f) {
    f.Name += "!"u8;
}

partial interface namer {
    @string label();
}

partial struct holder {
    internal frame f;
    internal namer i;
}

internal static nint frameCalls;

internal static frame makeFrame() {
    frameCalls++;
    return new frame(Name: "made"u8);
}

internal static ж<frame> makePtr() {
    frameCalls++;
    return Ꮡ(new frame(Name: "madeptr"u8));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object typedˢ = (@string)"typed   "u8;
private static readonly object boxedˢ = (@string)"boxed   "u8;
private static readonly object distinctˢ = (@string)"distinct"u8;
private static readonly object argumentˢ = (@string)"argument"u8;
private static readonly object assignˢ = (@string)"assign  "u8;
private static readonly object chainVˢ = (@string)"chainV  "u8;
private static readonly object chainPˢ = (@string)"chainP  "u8;
private static readonly object chainIˢ = (@string)"chainI  "u8;
private static readonly object callOnceˢ = (@string)"callOnce"u8;
private static readonly @string s10ˢ = "S10"u8;
private static readonly object idxVˢ = (@string)"idxV    "u8;
private static readonly object idxPˢ = (@string)"idxP    "u8;
private static readonly object argPˢ = (@string)"argP    "u8;
private static readonly object idxMapVˢ = (@string)"idxMapV "u8;
private static readonly object ptrThruˢ = (@string)"ptrThru "u8;
private static readonly @string p15ˢ = "P15"u8;
private static readonly object chainPtrˢ = (@string)"chainPtr"u8;
private static readonly @string n16ˢ = "N16"u8;
private static readonly object chainIfPˢ = (@string)"chainIfP"u8;
private static readonly object callPtrˢ = (@string)"callPtr "u8;

internal static void Main() {
    ref var a = ref heap<frame>(out var Ꮡa);
    a = new frame(Name: "a"u8);
        var aʗ1 = a;
    var typed = new Func<@string>[]{
        () => aʗ1.label(),
        () => Ꮡa.Value.Name
    }.slice();
    a.Name = "A"u8;
    fmt.Println(typedˢ, typed[0](), typed[1]());
    ref var b = ref heap<frame>(out var Ꮡb);
    b = new frame(Name: "b"u8, Inlined: true);
        var bʗ1 = b;
    var boxed = new any[]{
        () => bʗ1.label(),
        bool () => Ꮡb.Value.Inlined
    }.slice();
    b.Name = "B"u8;
    fmt.Println(boxedˢ, boxed[0]._<Func<@string>>()(), boxed[1]._<Func<bool>>()());
    var c = new frame(Name: "p"u8);
    
    var cʗ1 = c;
    var first = () => cʗ1.label();
    c.Name = "q"u8;
    
    var cʗ2 = c;
    var second = () => cʗ2.label();
    c.Name = "r"u8;
    fmt.Println(distinctˢ, first(), second(), c.label());
    var d = new frame(Name: "d"u8);
    @string call(Func<@string> f) => f();
    var dʗ1 = d;
    @string got = call(() => dʗ1.label());
    d.Name = "D"u8;
    var dʗ2 = d;
    fmt.Println(argumentˢ, got, call(() => dʗ2.label()));
    ref var e = ref heap<frame>(out var Ꮡe);
    e = new frame(Name: "e"u8);
    @string watch() => Ꮡe.Value.Name;
    var eʗ1 = e;
    
    var bound = () => eʗ1.label();
    e.Name = "E"u8;
    fmt.Println(assignˢ, bound(), watch());
    var h6 = new holder(f: new frame(Name: "f6"u8));
    var recvʗ1 = h6.f;
    
    var h6ʗ1 = h6;
    var chainV = () => recvʗ1.label();
    h6.f.Name = "F6"u8;
    fmt.Println(chainVˢ, chainV());
    ref var h7 = ref heap<holder>(out var Ꮡh7);
    h7 = new holder(f: new frame(Name: "f7"u8));
    var chainP = Ꮡh7.of(holder.Ꮡf).bump;
    chainP();
    fmt.Println(chainPˢ, h7.f.Name);
    var h8 = new holder(i: new frame(Name: "i8"u8));
    
    var h8ʗ1 = h8;
    var chainI = h8ʗ1.i.label;
    h8.i = new frame(Name: "I8"u8);
    fmt.Println(chainIˢ, chainI());
    frameCalls = 0;
    var recvʗ2 = makeFrame();
    
    var callRecv = () => recvʗ2.label();
    _ = callRecv();
    _ = callRecv();
    fmt.Println(callOnceˢ, frameCalls);
    var s10 = new slice<frame>(1, 1);
    s10[0] = new frame(Name: "s10"u8);
    var recvʗ3 = s10[0];
    
    var s10ʗ1 = s10;
    var idxV = () => recvʗ3.label();
    s10 = append(s10, new frame(Name: "grown"u8));
    s10[0].Name = s10ˢ;
    fmt.Println(idxVˢ, idxV());
    var s11 = new slice<frame>(1, 1);
    s11[0] = new frame(Name: "s11"u8);
    var idxP = Ꮡ(s11, 0).bump;
    s11 = append(s11, new frame(Name: "grown"u8));
    idxP();
    fmt.Println(idxPˢ, s11[0].Name);
    ref var p12 = ref heap<frame>(out var Ꮡp12);
    p12 = new frame(Name: "p12"u8);
    Action hold(Action f) => f;
    var argP = hold(Ꮡp12.bump);
    p12 = new frame(Name: "P12"u8);
    argP();
    fmt.Println(argPˢ, p12.Name);
    var m13 = new map<@string, frame>{["k"u8] = new(Name: "m13"u8)};
    var recvʗ4 = m13["k"u8];
    
    var m13ʗ1 = m13;
    var idxMapV = () => recvʗ4.label();
    m13["k"u8] = new frame(Name: "M13"u8);
    fmt.Println(idxMapVˢ, idxMapV());
    ref var c14 = ref heap<frame>(out var Ꮡc14);
    c14 = new frame(Name: "c14"u8);
    var thru = Ꮡc14.bump;
    thru();
    thru();
    fmt.Println(ptrThruˢ, c14.Name);
    var p15 = Ꮡ(new holder(f: new frame(Name: "p15"u8)));
    var recvʗ5 = (~p15).f;
    
    var p15ʗ1 = p15;
    var chainPtrBase = () => recvʗ5.label();
    p15.Value.f.Name = p15ˢ;
    fmt.Println(chainPtrˢ, chainPtrBase());
    namer n16 = new frameжnamer(Ꮡ(new frame(Name: "n16"u8)));
    var h16 = new holder(i: n16);
    
    var h16ʗ1 = h16;
    var chainIfacePtr = h16ʗ1.i.label;
    n16._<ж<frame>>().Value.Name = n16ˢ;
    fmt.Println(chainIfPˢ, chainIfacePtr());
    frameCalls = 0;
    var recvʗ6 = makePtr();
    
    var callPtr = () => recvʗ6.bump();
    callPtr();
    callPtr();
    fmt.Println(callPtrˢ, frameCalls);
}

} // end main_package

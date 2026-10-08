namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct frame {
    public @string Name;
}

internal static @string label(this frame f) {
    return f.Name;
}

internal static @string tag(this frame f, @string suffix) {
    return f.Name + suffix;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string touchedˢ = "touched"u8;

internal static @string touch(this frame f) {
    f.Name = touchedˢ;
    return f.Name;
}

partial struct holder {
    internal ж<frame> p;
}

internal static nint ptrCalls;

internal static ж<frame> makePtr() {
    ptrCalls++;
    return Ꮡ(new frame(Name: "made"u8));
}

internal static Func<@string> viaParam(ж<frame> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var recvʗ1 = p;
    return () => recvʗ1.label();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fieldˢ = (@string)"field   "u8;
private static readonly object repointˢ = (@string)"repoint "u8;
private static readonly object identˢ = (@string)"ident   "u8;
private static readonly object argumentˢ = (@string)"argument"u8;
private static readonly object callOnceˢ = (@string)"callOnce"u8;
private static readonly object paramsˢ = (@string)"params  "u8;
private static readonly object copyˢ = (@string)"copy    "u8;
private static readonly object paramˢ = (@string)"param   "u8;
private static readonly object elemˢ = (@string)"elem    "u8;

internal static void Main() {
    var h1 = new holder(p: Ꮡ(new frame(Name: "a"u8)));
    var recvʗ1 = ~h1.p;
    
    var h1ʗ1 = h1;
    var fieldV = () => recvʗ1.label();
    h1.p.Value.Name = "A"u8;
    fmt.Println(fieldˢ, fieldV(), (~h1.p).label());
    var h2 = new holder(p: Ꮡ(new frame(Name: "b"u8)));
    var recvʗ2 = ~h2.p;
    
    var h2ʗ1 = h2;
    var repoint = () => recvʗ2.label();
    h2.p = Ꮡ(new frame(Name: "B"u8));
    fmt.Println(repointˢ, repoint(), (~h2.p).label());
    var p3 = Ꮡ(new frame(Name: "c"u8));
    var recvʗ3 = ~p3;
    
    var p3ʗ1 = p3;
    var identV = () => recvʗ3.label();
    p3.Value.Name = "C"u8;
    fmt.Println(identˢ, identV());
    var h4 = new holder(p: Ꮡ(new frame(Name: "d"u8)));
    @string call(Func<@string> f) => f();
    var recvʗ4 = ~h4.p;
    @string got = call(() => recvʗ4.label());
    h4.p.Value.Name = "D"u8;
    var recvʗ5 = ~h4.p;
    fmt.Println(argumentˢ, got, call(() => recvʗ5.label()));
    ptrCalls = 0;
    var recvʗ6 = ~makePtr();
    
    var callV = () => recvʗ6.label();
    _ = callV();
    _ = callV();
    fmt.Println(callOnceˢ, ptrCalls, callV());
    var h6 = new holder(p: Ꮡ(new frame(Name: "e"u8)));
    var recvʗ7 = ~h6.p;
    
    var h6ʗ1 = h6;
    var tag = (@string p1) => recvʗ7.tag(p1);
    h6.p.Value.Name = "E"u8;
    fmt.Println(paramsˢ, tag("!"u8));
    var h7 = new holder(p: Ꮡ(new frame(Name: "f"u8)));
    var recvʗ8 = ~h7.p;
    
    var h7ʗ1 = h7;
    var touch = () => recvʗ8.touch();
    fmt.Println(copyˢ, touch(), (~h7.p).Name);
    var p8 = Ꮡ(new frame(Name: "g"u8));
    var paramV = viaParam(p8);
    p8.Value.Name = "G"u8;
    fmt.Println(paramˢ, paramV());
    var s9 = new ж<frame>[]{Ꮡ(new frame(Name: "h"u8))}.slice();
    var recvʗ9 = ~s9[0];
    
    var s9ʗ1 = s9;
    var elemV = () => recvʗ9.label();
    s9[0].Value.Name = "H"u8;
    fmt.Println(elemˢ, elemV());
}

} // end main_package

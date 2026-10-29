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

internal static @string touch(this frame f) {
    f.Name = "touched";
    return f.Name;
}

partial struct holder {
    internal ж<frame> p;
}

internal static nint ptrCalls;

internal static ж<frame> makePtr() {
    ptrCalls++;
    return Ꮡ(new frame(Name: "made"));
}

internal static Func<@string> viaParam(ж<frame> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var recvʗ1 = p;
    return () => recvʗ1.label();
}

internal static void Main() {
    var h1 = new holder(p: Ꮡ(new frame(Name: "a")));
    var recvʗ1 = ~h1.p;
    
    var h1ʗ1 = h1;
    var fieldV = () => recvʗ1.label();
    h1.p.Value.Name = "A";

    fmt.Println("field   ", fieldV(), (~h1.p).label());

    var h2 = new holder(p: Ꮡ(new frame(Name: "b")));
    var recvʗ2 = ~h2.p;
    
    var h2ʗ1 = h2;
    var repoint = () => recvʗ2.label();
    h2.p = Ꮡ(new frame(Name: "B"));

    fmt.Println("repoint ", repoint(), (~h2.p).label());

    var p3 = Ꮡ(new frame(Name: "c"));
    var recvʗ3 = ~p3;
    
    var p3ʗ1 = p3;
    var identV = () => recvʗ3.label();
    p3.Value.Name = "C";

    fmt.Println("ident   ", identV());

    var h4 = new holder(p: Ꮡ(new frame(Name: "d")));
    @string call(Func<@string> f) => f();
    var recvʗ4 = ~h4.p;
    @string got = call(() => recvʗ4.label());
    h4.p.Value.Name = "D";

    var recvʗ5 = ~h4.p;
    fmt.Println("argument", got, call(() => recvʗ5.label()));

    ptrCalls = 0;
    var recvʗ6 = ~makePtr();
    
    var callV = () => recvʗ6.label();
    _ = callV();
    _ = callV();

    fmt.Println("callOnce", ptrCalls, callV());

    var h6 = new holder(p: Ꮡ(new frame(Name: "e")));
    var recvʗ7 = ~h6.p;
    
    var h6ʗ1 = h6;
    var tag = (@string p1) => recvʗ7.tag(p1);
    h6.p.Value.Name = "E";

    fmt.Println("params  ", tag("!"));

    var h7 = new holder(p: Ꮡ(new frame(Name: "f")));
    var recvʗ8 = ~h7.p;
    
    var h7ʗ1 = h7;
    var touch = () => recvʗ8.touch();

    fmt.Println("copy    ", touch(), (~h7.p).Name);

    var p8 = Ꮡ(new frame(Name: "g"));
    var paramV = viaParam(p8);
    p8.Value.Name = "G";

    fmt.Println("param   ", paramV());

    var s9 = slice<ж<frame>>([Ꮡ(new frame(Name: "h"u8))]);
    var recvʗ9 = ~s9[0];
    
    var s9ʗ1 = s9;
    var elemV = () => recvʗ9.label();
    s9[0].Value.Name = "H";

    fmt.Println("elem    ", elemV());
}

} // end main_package

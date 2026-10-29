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
    return new frame(Name: "made");
}

internal static ж<frame> makePtr() {
    frameCalls++;
    return Ꮡ(new frame(Name: "madeptr"));
}

internal static void Main() {
    var s10ᴷ = new stackAppend<frame>();
    var s11ᴷ = new stackAppend<frame>();

    ref var a = ref heap<frame>(new frame(Name: "a"), out var Ꮡa);

        var aʗ1 = a;
    var typed = new Func<@string>[]{
        () => aʗ1.label(),
        () => Ꮡa.Value.Name
    }.slice();

    a.Name = "A";

    fmt.Println("typed   ", typed[0](), typed[1]());

    ref var b = ref heap<frame>(new frame(Name: "b", Inlined: true), out var Ꮡb);

        var bʗ1 = b;
    var boxed = new any[]{
        () => bʗ1.label(),
        bool () => Ꮡb.Value.Inlined
    }.slice();

    b.Name = "B";

    fmt.Println("boxed   ", boxed[0]._<Func<@string>>()(), boxed[1]._<Func<bool>>()());

    var c = new frame(Name: "p");
    
    var cʗ1 = c;
    var first = () => cʗ1.label();
    c.Name = "q";
    
    var cʗ2 = c;
    var second = () => cʗ2.label();
    c.Name = "r";

    fmt.Println("distinct", first(), second(), c.label());

    var d = new frame(Name: "d");
    @string call(Func<@string> f) => f();
    var dʗ1 = d;
    @string got = call(() => dʗ1.label());
    d.Name = "D";

    var dʗ2 = d;
    fmt.Println("argument", got, call(() => dʗ2.label()));

    ref var e = ref heap<frame>(new frame(Name: "e"), out var Ꮡe);
    @string watch() => Ꮡe.Value.Name;
    var eʗ1 = e;
    
    var bound = () => eʗ1.label();
    e.Name = "E";

    fmt.Println("assign  ", bound(), watch());

    var h6 = new holder(f: new frame(Name: "f6"));
    var recvʗ1 = h6.f;
    
    var h6ʗ1 = h6;
    var chainV = () => recvʗ1.label();
    h6.f.Name = "F6";

    fmt.Println("chainV  ", chainV());

    ref var h7 = ref heap<holder>(new holder(f: new frame(Name: "f7")), out var Ꮡh7);
    var chainP = Ꮡh7.of(holder.Ꮡf).bump;
    chainP();

    fmt.Println("chainP  ", h7.f.Name);

    var h8 = new holder(i: new frame(Name: "i8"));
    
    var h8ʗ1 = h8;
    var chainI = h8ʗ1.i.label;
    h8.i = new frame(Name: "I8");

    fmt.Println("chainI  ", chainI());

    frameCalls = 0;
    var recvʗ2 = makeFrame();
    
    var callRecv = () => recvʗ2.label();
    _ = callRecv();
    _ = callRecv();

    fmt.Println("callOnce", frameCalls);

    var s10 = new slice<frame>(1, 1);
    s10[0] = new frame(Name: "s10");
    var recvʗ3 = s10[0];
    
    var s10ʗ1 = s10;
    var idxV = () => recvʗ3.label();
    s10 = append(s10ᴷ, s10, new frame(Name: "grown"));
    s10[0].Name = "S10";

    fmt.Println("idxV    ", idxV());

    var s11 = new slice<frame>(1, 1);
    s11[0] = new frame(Name: "s11");
    var idxP = Ꮡ(s11, 0).bump;
    s11 = append(s11ᴷ, s11, new frame(Name: "grown"));
    idxP();

    fmt.Println("idxP    ", s11[0].Name);

    ref var p12 = ref heap<frame>(new frame(Name: "p12"), out var Ꮡp12);
    Action hold(Action f) => f;
    var argP = hold(Ꮡp12.bump);
    p12 = new frame(Name: "P12");
    argP();

    fmt.Println("argP    ", p12.Name);

    var m13 = new map<@string, frame>{["k"] = new(Name: "m13")};
    var recvʗ4 = m13["k"];
    
    var m13ʗ1 = m13;
    var idxMapV = () => recvʗ4.label();
    m13["k"] = new frame(Name: "M13");

    fmt.Println("idxMapV ", idxMapV());

    ref var c14 = ref heap<frame>(new frame(Name: "c14"), out var Ꮡc14);
    var thru = Ꮡc14.bump;
    thru();
    thru();

    fmt.Println("ptrThru ", c14.Name);

    var p15 = Ꮡ(new holder(f: new frame(Name: "p15")));
    var recvʗ5 = p15.f;
    
    var p15ʗ1 = p15;
    var chainPtrBase = () => recvʗ5.label();
    p15.f.Name = "P15";

    fmt.Println("chainPtr", chainPtrBase());

    namer n16 = new frameжnamer(Ꮡ(new frame(Name: "n16")));
    var h16 = new holder(i: n16);
    
    var h16ʗ1 = h16;
    var chainIfacePtr = h16ʗ1.i.label;
    n16._<namer, ж<frame>>().Name = "N16";

    fmt.Println("chainIfP", chainIfacePtr());

    frameCalls = 0;
    var recvʗ6 = makePtr();
    
    var callPtr = () => recvʗ6.bump();
    callPtr();
    callPtr();

    fmt.Println("callPtr ", frameCalls);
}

} // end main_package

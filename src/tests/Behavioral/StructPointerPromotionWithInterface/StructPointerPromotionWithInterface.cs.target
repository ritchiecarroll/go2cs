namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial interface Abser {
    float64 Abs();
}

partial struct MyError {
    public time.Time When;
    public @string What;
}

partial struct MyCustomError {
    public @string Message;
    /*embed*/ public Abser Abser;
    /*embed*/ public ж<MyError> MyError;
}

public static float64 Time(this ref MyCustomError myErr) {
    return 0.0D;
}

public static float64 Time(this MyError myErr) {
    return (float64)myErr.When.Unix();
}

partial struct core {
    internal nint pings;
}

internal static @string Ping(this ref core c) {
    c.pings++;
    return "pong";
}

partial struct Station {
    /*embed*/ internal core core;
    internal @string id;
}

partial struct noop {
}

partial struct link {
    /*embed*/ internal noop noop;
    /*embed*/ public ж<Station> Station;
}

partial interface Pinger {
    @string Ping();
}

partial struct Device {
    internal @string name;
    internal nint hits;
}

public static ж<nint> Tag(this ж<Device> Ꮡd) {
    return Ꮡd.of(Device.Ꮡhits);
}

public static @string Describe(this ref Device d) {
    return d.name;
}

partial struct meta {
    internal @string label;
    internal nint count;
}

internal static @string Stamp(this ref meta m) {
    m.count++;
    return m.label;
}

internal static nint Hits(this ref meta m) {
    return m.count;
}

partial struct kindBase {
    /*embed*/ internal meta meta;
}

partial struct counterKind {
    /*embed*/ internal kindBase kindBase;
}

partial interface stamper {
    @string Stamp();
    nint Hits();
}

partial interface Describer {
    @string Describe();
    ж<nint> Tag();
}

partial struct rig {
    internal Device dev;
}

internal static nint probeRig(rig rʗp) {
    ref var r = ref heap(rʗp, out var Ꮡr);

    return Ꮡr.of(rig.Ꮡdev).Tag().Value;
}

partial struct deviceHandle {
    /*embed*/ public ж<Device> Device;
}

partial struct leftSide {
    internal @string tag;
}

internal static @string Ping(this leftSide l) {
    return "L";
}

partial struct rightSide {
    internal @string tag;
}

internal static @string Ping(this rightSide r) {
    return "R";
}

partial struct pair {
    /*embed*/ internal leftSide leftSide;
    /*embed*/ internal rightSide rightSide;
}

partial struct Inner {
    public @string Value;
}

partial struct Middle {
    /*embed*/ public ж<Inner> Inner;
}

partial struct Outer {
    internal ж<ж<Inner>> ptr;
}

internal static void Main() {
    ref var e = ref heap<MyError>(out var Ꮡe);
    e = new MyError(time.Now(), "Hello");
    var a = new MyCustomError("New One", default!, Ꮡe);

    a.Message = "New";
    a.What = "World";

    fmt.Println("MyError What =", e.What);
    fmt.Println("MyCustomError What =", a.What);
    fmt.Println("MyCustomError method =", a.Time());

    ref var inner = ref heap<ж<Inner>>(out var Ꮡinner);
    inner = Ꮡ(new Inner(Value: "hello"));
    var innerPtr = Ꮡinner;

    var middle = new Middle(Inner: inner);
    fmt.Println(middle.Value);

    var outer = new Outer(ptr: innerPtr);
    fmt.Println((~(outer.ptr.ValueSlot)).Value);

    var dev = Ꮡ(new Device(name: "sensor", hits: 3));
    Describer dsc = new deviceHandle(Device: dev);
    fmt.Println(dsc.Describe());
    var p = dsc.Tag();
    p.Value = 7;
    fmt.Println((~dev).hits);

    var pw = new pair(new leftSide(tag: "a"), new rightSide(tag: "b"));
    fmt.Println(pw.leftSide.tag, pw.rightSide.Ping());

    fmt.Println(probeRig(new rig(dev: new Device(name: "r", hits: 42))));

    var ck = Ꮡ(new counterKind(nil));
    ck.Value.label = "k9";
    stamper st = new counterKindжstamper(ck);
    fmt.Println(st.Stamp(), st.Stamp(), st.Hits());

    var stn = Ꮡ(new Station(id: "s1"));
    Pinger pg = new link(Station: stn);
    fmt.Println(pg.Ping(), pg.Ping(), (~stn).pings);
}

} // end main_package

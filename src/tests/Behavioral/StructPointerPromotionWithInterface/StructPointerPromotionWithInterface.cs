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
    public partial ref ж<MyError> MyError { get; }
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pongˢ = "pong"u8;

internal static @string Ping(this ref core c) {
    c.pings++;
    return pongˢ;
}

partial struct Station {
    internal partial ref core core { get; }
    internal @string id;
}

partial struct noop {
}

partial struct link {
    internal partial ref noop noop { get; }
    public partial ref ж<Station> Station { get; }
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
    internal partial ref meta meta { get; }
}

partial struct counterKind {
    internal partial ref kindBase kindBase { get; }
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
    public partial ref ж<Device> Device { get; }
}

partial struct leftSide {
    internal @string tag;
}

internal static @string Ping(this leftSide l) {
    return "L"u8;
}

partial struct rightSide {
    internal @string tag;
}

internal static @string Ping(this rightSide r) {
    return "R"u8;
}

partial struct pair {
    internal partial ref leftSide leftSide { get; }
    internal partial ref rightSide rightSide { get; }
}

partial struct Inner {
    public @string Value;
}

partial struct Middle {
    public partial ref ж<Inner> Inner { get; }
}

partial struct Outer {
    internal ж<ж<Inner>> ptr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string newˢ = "New"u8;
private static readonly @string worldˢ = "World"u8;
private static readonly object myErrorWhatˢ = (@string)"MyError What ="u8;
private static readonly object myCustomErrorWhatˢ = (@string)"MyCustomError What ="u8;
private static readonly object myCustomErrorMethodˢ = (@string)"MyCustomError method ="u8;

internal static void Main() {
    ref var e = ref heap<MyError>(out var Ꮡe);
    e = new MyError(time.Now(), "Hello"u8);
    var a = new MyCustomError("New One"u8, default!, Ꮡe);
    a.Message = newˢ;
    a.What = worldˢ;
    fmt.Println(myErrorWhatˢ, e.What);
    fmt.Println(myCustomErrorWhatˢ, a.What);
    fmt.Println(myCustomErrorMethodˢ, a.Time());
    ref var inner = ref heap<ж<Inner>>(out var Ꮡinner);
    inner = Ꮡ(new Inner(Value: "hello"u8));
    var innerPtr = Ꮡinner;
    var middle = new Middle(Inner: inner);
    fmt.Println(middle.Value);
    var outer = new Outer(ptr: innerPtr);
    fmt.Println((~(outer.ptr.ValueSlot)).Value);
    var dev = Ꮡ(new Device(name: "sensor"u8, hits: 3));
    Describer dsc = new deviceHandle(Device: dev);
    fmt.Println(dsc.Describe());
    var p = dsc.Tag();
    p.Value = 7;
    fmt.Println((~dev).hits);
    var pw = new pair(new leftSide(tag: "a"u8), new rightSide(tag: "b"u8));
    fmt.Println(pw.leftSide.tag, pw.rightSide.Ping());
    fmt.Println(probeRig(new rig(dev: new Device(name: "r"u8, hits: 42))));
    var ck = Ꮡ(new counterKind(nil));
    ck.Value.label = "k9"u8;
    stamper st = new counterKindжstamper(ck);
    fmt.Println(st.Stamp(), st.Stamp(), st.Hits());
    var stn = Ꮡ(new Station(id: "s1"u8));
    Pinger pg = new link(Station: stn);
    fmt.Println(pg.Ping(), pg.Ping(), (~stn).pings);
}

} // end main_package

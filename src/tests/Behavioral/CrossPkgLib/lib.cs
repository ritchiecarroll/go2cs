global using Temperature = go.CrossPkgLib_package.Celsius;
global using Token = object;

namespace go;

partial class CrossPkgLib_package {

public static UntypedInt Precision => 2;

public static UntypedInt Sep => /* ':' */ 58;

partial struct Celsius /*num:float64*/;

public static Celsius Boiling() { return 100D; }

public static Temperature Freezing() { return 0D; }

internal static Celsius Add⁔(/*this*/ Celsius c, Celsius d) { return c + d; }

partial struct Sensor {
    public @string Name;
    public Celsius Temp;
}

internal static bool Hot⁔(/*this*/ Sensor s) { return s.Temp > 50D; }

internal static @string Label⁔(/*this*/ Sensor s) { return s.Name; }

internal static void Calibrate⁔(/*this*/ ref Sensor s, Celsius d) { s.Temp += d; }

partial interface Labeled {
    @string Label();
}

internal static Labeled _ᴛ1ʗ = new Sensor();

public static @string Describe(Labeled l) { return l.Label(); }

public static Labeled LabeledOf(ж<Sensor> s) { return new SensorжLabeled(s); }

partial struct Meter {
    internal nint count;
}

internal static nint Bump⁔(/*this*/ ref Meter m) {
    m.count++;
    return m.count;
}

public static ж<Meter> NewMeter() { return Ꮡ(new Meter()); }

partial interface Reporter {
    @string Report();
}

internal static @string Report⁔(/*this*/ ref Meter m) { return "count"; }

partial struct Alarm {
    public @string Msg;
}

internal static @string Error⁔(/*this*/ ref Alarm a) { return a.Msg; }

public static error AsErr(ж<Alarm> a) { return new Alarmжerror(a); }

public static Reporter AsReporter(ж<Meter> m) { return new MeterжReporter(m); }

partial struct Cache<T> {
    public nint Hits;
}

internal static nint Bump⁔<T>(/*this*/ ref Cache<T> c) {
    c.Hits++;
    return c.Hits;
}

public static slice<T> Wrap<T>(T v) { return [v]; }

public static V Pair<K, V>(K k, V v) { return v; }

public delegate bool Sift(nint _);

internal static bool Sift⁔(/*this*/ Sensor s, Sift f) { return f((nint)s.Temp); }

partial struct Node {
    public nint ID;
}

public delegate (ж<Node>, error) Resolver(map<@string, ж<Node>> imports, @string path);

public static (ж<Node>, error) Resolve(Resolver r, @string path) { return r(default, path); }

partial struct Probe {
    public nint Hits;
}

internal static nint Sample⁔(/*this*/ ref Probe p) {
    p.Hits++;
    return p.Hits;
}

partial interface Sampler {
    nint Sample();
}

partial interface Sealed {
    @string Label();
    @string Seal();
}

partial interface Rated {
    @string Label();
    nint Rating();
}

partial struct Status {
    public nint Code;
}

internal static nint Status⁔(/*this*/ Sensor s) { return (nint)s.Temp; }

partial struct Grade /*num:nint*/;

internal static nint Grade⁔(/*this*/ Sensor s) { return 1; }

internal static nint Token⁔(/*this*/ Sensor s) { return (nint)s.Temp; }

public static Token AsToken(nint v) { return v; }

public partial struct snapshot {
    public nint At;
}

public static readonly ж<snapshot> ᏑLatest = new StaticBox<snapshot>(new snapshot(At: 42));
public static ref snapshot Latest => ref ((StaticBox<snapshot>)ᏑLatest).ValueSlot;

public static snapshot Peek() { return Latest; }

partial struct Ticks /*num:uintptr*/;

partial struct Device {
    /*embed*/ public Sensor Sensor;
    public nint Serial;
}

partial struct Marker {
    public @string ΔMarker;
}

internal static @string Marker⁔(/*this*/ Sensor s) { return s.Name; }

public static Marker MakeMarker(@string s) { return new Marker(ΔMarker: s); }

partial interface Emitter {
    @string Emit();
    void emitNode();
    @string nodeTag();
}

public static @string DescribeEmitter(Emitter e) { return e.Emit() + "/"u8 + e.nodeTag(); }

partial struct Leaf {
    public @string Text;
}

internal static @string Emit⁔(/*this*/ ref Leaf l) { return l.Text; }

internal static void emitNode⁔(/*this*/ ref Leaf l) { }

internal static @string nodeTag⁔(/*this*/ ref Leaf l) { return "lf"; }

public static ж<Leaf> NewLeaf(@string text) { return Ꮡ(new Leaf(Text: text)); }

partial struct EmitBase {
    public @string Label;
}

internal static @string Emit⁔(/*this*/ ref EmitBase e) { return e.Label; }

partial struct Branch {
    /*embed*/ public EmitBase EmitBase;
    public nint Kind;
}

internal static void emitNode⁔(/*this*/ ref Branch b) { }

internal static @string nodeTag⁔(/*this*/ ref Branch b) { return "brn"; }

public static ж<Branch> NewBranch(@string label, nint kind) {
    return Ꮡ(new Branch(EmitBase: new EmitBase(Label: label), Kind: kind));
}

partial struct Verdict /*num:nint*/;

internal static nint Score⁔(/*this*/ Verdict v) { return (nint)v * 10; }

partial interface Scored {
    nint Score();
}

} // end CrossPkgLib_package

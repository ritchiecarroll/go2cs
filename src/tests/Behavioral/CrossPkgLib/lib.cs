global using Temperature = go.CrossPkgLib_package.Celsius;
global using ΔToken = object;

namespace go;

partial class CrossPkgLib_package {

public static UntypedInt Precision => 2;

public static UntypedInt Sep => /* ':' */ 58;

partial struct Celsius /*num:float64*/;

public static Celsius Boiling() {
    return 100D;
}

public static Temperature Freezing() {
    return 0D;
}

public static Celsius Add(this Celsius c, Celsius d) {
    return c + d;
}

partial struct Sensor {
    public @string Name;
    public Celsius Temp;
}

public static bool Hot(this Sensor s) {
    return s.Temp > 50D;
}

public static @string Label(this Sensor s) {
    return s.Name;
}

public static void Calibrate(this ref Sensor s, Celsius d) {
    s.Temp += d;
}

partial interface Labeled {
    @string Label();
}

internal static Labeled _ᴛ1ʗ = new Sensor(nil);

public static @string Describe(Labeled l) {
    return l.Label();
}

public static Labeled LabeledOf(ж<Sensor> Ꮡs) {
    return new SensorжLabeled(Ꮡs);
}

partial struct Meter {
    internal nint count;
}

public static nint Bump(this ref Meter m) {
    m.count++;
    return m.count;
}

public static ж<Meter> NewMeter() {
    return Ꮡ(new Meter(nil));
}

partial interface Reporter {
    @string Report();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string countˢ = "count"u8;

public static @string Report(this ref Meter m) {
    return countˢ;
}

partial struct Alarm {
    public @string Msg;
}

public static @string Error(this ref Alarm a) {
    return a.Msg;
}

public static error AsErr(ж<Alarm> Ꮡa) {
    return new Alarmжerror(Ꮡa);
}

public static Reporter AsReporter(ж<Meter> Ꮡm) {
    return new MeterжReporter(Ꮡm);
}

partial struct Cache<T> {
    public nint Hits;
}

public static nint Bump<T>(this ref Cache<T> c) {
    c.Hits++;
    return c.Hits;
}

public static slice<T> Wrap<T>(T v) {
    return new T[]{v}.slice();
}

public static V Pair<K, V>(K k, V v) {
    return v;
}

// type ΔSift is a methodless func type — rendered inline as its base delegate

public static bool Sift(this Sensor s, Func<nint, bool> f) {
    return f((nint)(float64)s.Temp);
}

partial struct Node {
    public nint ID;
}

// type Resolver is a methodless func type — rendered inline as its base delegate

public static (ж<Node>, error) Resolve(Func<map<@string, ж<Node>>, @string, (ж<Node>, error)> r, @string path) {
    return r(default!, path);
}

partial struct Probe {
    public nint Hits;
}

public static nint Sample(this ref Probe p) {
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

partial struct ΔStatus {
    public nint Code;
}

public static nint Status(this Sensor s) {
    return (nint)(float64)s.Temp;
}

partial struct ΔGrade /*num:nint*/;

public static nint Grade(this Sensor s) {
    return 1;
}

public static nint Token(this Sensor s) {
    return (nint)(float64)s.Temp;
}

public static ΔToken AsToken(nint v) {
    return v;
}

public partial struct snapshot {
    public nint At;
}

public static snapshot Latest = new snapshot(At: 42);

public static snapshot Peek() {
    return Latest;
}

partial struct Ticks /*num:uintptr*/;

partial struct Device {
    public partial ref Sensor Sensor { get; }
    public nint Serial;
}

partial struct ΔMarker {
    public @string ΔΔMarker;
}

public static @string Marker(this Sensor s) {
    return s.Name;
}

public static ΔMarker MakeMarker(@string s) {
    return new ΔMarker(ΔΔMarker: s);
}

partial interface Emitter {
    @string Emit();
    void emitNode();
    @string nodeTag();
}

public static @string DescribeEmitter(Emitter e) {
    return e.Emit() + "/"u8 + e.nodeTag();
}

partial struct Leaf {
    public @string Text;
}

public static @string Emit(this ref Leaf l) {
    return l.Text;
}

internal static void emitNode(this ref Leaf l) {
}

internal static @string nodeTag(this ref Leaf l) {
    return "lf"u8;
}

public static ж<Leaf> NewLeaf(@string text) {
    return Ꮡ(new Leaf(Text: text));
}

partial struct EmitBase {
    public @string Label;
}

public static @string Emit(this ref EmitBase e) {
    return e.Label;
}

partial struct Branch {
    public partial ref EmitBase EmitBase { get; }
    public nint Kind;
}

internal static void emitNode(this ref Branch b) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string brnˢ = "brn"u8;

internal static @string nodeTag(this ref Branch b) {
    return brnˢ;
}

public static ж<Branch> NewBranch(@string label, nint kind) {
    return Ꮡ(new Branch(EmitBase: new EmitBase(Label: label), Kind: kind));
}

partial struct Verdict /*num:nint*/;

public static nint Score(this Verdict v) {
    return (nint)v * 10;
}

partial interface Scored {
    nint Score();
}

} // end CrossPkgLib_package

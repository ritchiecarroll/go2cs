global using Tagged = go.CrossPkgLib_package.Labeled;
global using tick = go.CrossPkgLib_package.Ticks;

namespace go;

using fmt = fmt_package;
using CrossPkgLib = CrossPkgLib_package;

partial class main_package {

public static Func<CrossPkgLibꓸStatus, nint> CheckFunc = (CrossPkgLibꓸStatus st) => st.Code * 2;

internal static (CrossPkgLibꓸStatus, nint) gauge(CrossPkgLibꓸStatus st) {
    return (new CrossPkgLibꓸStatus(Code: st.Code + 1), st.Code);
}

partial struct meterBox {
    internal CrossPkgLibꓸStatus st;
    internal nint sat;
}

internal static ж<CrossPkgLibꓸStatus> statusPtr(ж<CrossPkgLibꓸStatus> Ꮡst) {
    ref var st = ref Ꮡst.DerefOrNull();

    st.Code++;
    return Ꮡst;
}

partial struct Holder<T> {
    internal T item;
}

internal static ж<Holder<ж<CrossPkgLib.Sensor>>> sensorHolder = Ꮡ(new Holder<ж<CrossPkgLib.Sensor>>(nil));

partial struct sensorBox {
    public partial ref Holder<ж<CrossPkgLib_package.Sensor>> Holder { get; }
    internal @string tag;
}

partial struct ledger {
    internal ж<CrossPkgLibꓸStatus> cur;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notedˢ = (@string)"noted"u8;

internal static void note(nint n) {
    fmt.Println(notedˢ, n);
}

partial struct badge {
    internal @string name;
}

internal static @string Label(this badge b) {
    return "badge:"u8 + b.name;
}

partial interface namedLabel :
    CrossPkgLib.Labeled
{
    nint Rank();
}

partial struct emblem {
    internal @string name;
    internal nint rank;
}

internal static @string Label(this emblem e) {
    return e.name;
}

internal static nint Rank(this emblem e) {
    return e.rank;
}

partial interface stamped :
    CrossPkgLib.Labeled
{
    @string Stamp();
}

partial interface Labeled {
    @string Label();
}

partial struct seal {
    internal @string name;
}

internal static @string Label(this seal s) {
    return s.name;
}

internal static @string Stamp(this seal s) {
    return "ok:"u8 + s.name;
}

partial struct dial {
    internal @string n;
}

internal static @string Label(this ref dial d) {
    return "dial:"u8 + d.n;
}

partial interface certificate :
    CrossPkgLib.Rated,
    CrossPkgLib.Sealed
{
    @string Label();
    nint Serial();
}

partial struct cert {
    internal nint id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string certˢ = "cert"u8;

internal static @string Label(this cert c) {
    return certˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string waxˢ = "wax"u8;

internal static @string Seal(this cert c) {
    return waxˢ;
}

internal static nint Rating(this cert c) {
    return 9;
}

internal static nint Serial(this cert c) {
    return c.id;
}

partial struct holder<T> {
    public partial ref ж<CrossPkgLib_package.Cache<T>> Cache { get; }
    internal @string name;
}

partial struct relay {
    internal @string tag;
}

internal static @string Report(this ref relay r) {
    return "relay:"u8 + r.tag;
}

internal static (ж<relay>, error) makeRelay() {
    return (Ꮡ(new relay(tag: "live"u8)), default!);
}

internal static (CrossPkgLib.Reporter, error) getReporter() {
    var (ᴛ1, ᴛ2) = makeRelay();
    return (new relayжReporter(ᴛ1), ᴛ2);
}

internal static CrossPkgLib.Emitter leafEmitter = new CrossPkgLib.LeafжEmitter(CrossPkgLib.NewLeaf("leaf"u8));

internal static CrossPkgLib.Emitter branchEmitter = new CrossPkgLib.BranchжEmitter(CrossPkgLib.NewBranch("branch"u8, 3));

internal static Func<@string, (@string, slice<byte>, error)> makeScanner(@string @base) {
    return (@string @file) => {
        @string name = default!;
        slice<byte> data = default!;
        error err = default!;
        name = @file;
        (data, err) = readWith(@base, @file);
        return (name, data, err);
    };
}

internal static (slice<byte>, error) readWith(@string @base, @string @file) {
    return (slice<byte>(@base + @file), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object promotedLabelˢ = (@string)"promoted label:"u8;
private static readonly object coarseˢ = (@string)"coarse"u8;
private static readonly object fineˢ = (@string)"fine"u8;
private static readonly object unknownˢ = (@string)"unknown"u8;
private static readonly @string tagˢ = "tag"u8;
private static readonly object tokenˢ = (@string)"token:"u8;
private static readonly @string abcdeˢ = "abcde"u8;
private static readonly object leafˢ = (@string)"leaf:"u8;
private static readonly object branchˢ = (@string)"branch:"u8;
private static readonly object describedLeafˢ = (@string)"described leaf:"u8;
private static readonly object describedBranchˢ = (@string)"described branch:"u8;
private static readonly @string helloˢ = "hello"u8;
private static readonly object verdictScoreˢ = (@string)"verdict score:"u8;
private static readonly object talliesScoreˢ = (@string)"tallies score:"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        defer(note, (nint)(CrossPkgLib.Precision), ref ᒐ);
        var b = CrossPkgLib.Boiling();
        var r = b.Add(10D);
        fmt.Println((float64)b);
        fmt.Println((float64)r);
        CrossPkgLibꓸTemperature t = CrossPkgLib.Freezing();
        t = t.Add(32D);
        fmt.Println((float64)t);
        var s = new CrossPkgLib.Sensor(Name: "kitchen"u8, Temp: CrossPkgLib.Boiling());
        fmt.Println(s.Name, (float64)s.Temp, s.Hot());
        CrossPkgLib.Labeled l = s;
        fmt.Println(l.Label());
        fmt.Println(CrossPkgLib.Describe(s));
        ref var s2 = ref heap<CrossPkgLib.Sensor>(out var Ꮡs2);
        s2 = new CrossPkgLib.Sensor(Name: "attic"u8, Temp: 20D);
        var p = new probe(Sensor: Ꮡs2, id: 7);
        fmt.Println(p.Name, (float64)p.Temp, p.id);
        p.Temp = 75D;
        fmt.Println((float64)s2.Temp, s2.Hot());
        p.Sensor.Value.Calibrate(5D);
        fmt.Println((float64)s2.Temp);
        var g = new tagged(Sensor: new CrossPkgLib.Sensor(Name: "cellar"u8, Temp: 5D), n: 3);
        fmt.Println(g.Name, (float64)g.Temp, g.n);
        g.Temp = 60D;
        fmt.Println((float64)g.Temp, g.Sensor.Hot());
        Labeled lb2 = g;
        fmt.Println(promotedLabelˢ, lb2.Label());
        var c = new counter(Meter: CrossPkgLib.NewMeter());
        fmt.Println(c.Meter.Value.Bump());
        ΔMeter m = c;
        fmt.Println(m.Bump());
        fmt.Println(c.Meter.Value.Bump());
        fmt.Println(g.Meter());
        var rd = ((reading)CrossPkgLib.Boiling());
        var cback = ((CrossPkgLib.Celsius)rd);
        fmt.Println((float64)cback, cback == CrossPkgLib.Boiling());
        var (st1, err1) = stampOrErr(false);
        var (st2, err2) = stampOrErr(true);
        fmt.Println(st1 == st2, err1 != default!, st2 == bigStamp, err2 == default!);
        var rg = new rig(Device: new CrossPkgLib.Device(Sensor: new CrossPkgLib.Sensor(Name: "deep"u8, Temp: 55D), Serial: 9), id: 1);
        fmt.Println((float64)rg.Temp, rg.Serial, rg.id);
        rg.Temp = 66D;
        fmt.Println((float64)rg.Device.Sensor.Temp);
        fmt.Println(rg.Device.Sensor.Hot());
        rg.Device.Sensor.Calibrate(3D);
        fmt.Println((float64)rg.Device.Sensor.Temp);
        var exprᴛ1 = CrossPkgLib.Precision;
        if (exprᴛ1 == 1) {
            fmt.Println(coarseˢ);
        }
        else if (exprᴛ1 == 2) {
            fmt.Println(fineˢ);
        }
        else { /* default: */
            fmt.Println(unknownˢ);
        }

        fmt.Println("a" + ((@string)(rune)CrossPkgLib.Sep) + "b");
        fmt.Println(CheckFunc(new CrossPkgLibꓸStatus(Code: 21)), new CrossPkgLib.Sensor(Temp: 9D).Status());
        var (g1, c1) = gauge(new CrossPkgLibꓸStatus(Code: 5));
        fmt.Println(g1.Code, c1);
        var mb = new meterBox(st: new CrossPkgLibꓸStatus(Code: 3), sat: 1);
        fmt.Println(mb.st.Code + mb.sat);
        fmt.Println(CrossPkgLib.Latest.At, CrossPkgLib.Peek().At);
        var sptr = Ꮡ(new CrossPkgLibꓸStatus(Code: 10));
        var led = new ledger(cur: statusPtr(sptr));
        fmt.Println((~led.cur).Code);
        CrossPkgLibꓸGrade gv = ((CrossPkgLibꓸGrade)7);
        CrossPkgLibꓸStatus stv = default!;
        stv.Code = (nint)gv;
        fmt.Println(stv.Code);
        var ccel = ((CrossPkgLib.Celsius)(float64)((localCelsius)2.5D));
        fmt.Println((float64)ccel);
        CrossPkgLib.Labeled l1 = new badge(name: "a"u8);
        Tagged l2 = new badge(name: "b"u8);
        fmt.Println(l1.Label(), l2.Label());
        CrossPkgLib.Reporter rep = default!;
        var mtr = CrossPkgLib.NewMeter();
        rep = new CrossPkgLib.MeterжReporter(mtr);
        fmt.Println(rep.Report());
        error boom = new CrossPkgLib.Alarmжerror(Ꮡ(new CrossPkgLib.Alarm(Msg: "boom"u8)));
        fmt.Println(boom.Error());
        var tk = ((tick)(uintptr)((nint)(3 | (nint)gv)));
        fmt.Println((uint64)(uintptr)tk);
        namedLabel nl = new emblem(name: "gold"u8, rank: 1);
        fmt.Println(CrossPkgLib.Describe(nl), nl.Rank());
        stamped st = new seal(name: "notary"u8);
        CrossPkgLib.Labeled lb = new seal(name: "base"u8);
        fmt.Println(st.Label(), st.Stamp(), CrossPkgLib.Describe(st), lb.Label());
        Labeled wide = new seal(name: "w"u8);
        fmt.Println(AreEqual(((CrossPkgLib.Labeled)wide), lb));
        var sp2 = Ꮡ(new CrossPkgLib.Sensor(Name: "porch"u8, Temp: 40D));
        fmt.Println(((CrossPkgLib.Labeled)new CrossPkgLib.SensorжLabeled(sp2)).Label(), CrossPkgLib.LabeledOf(sp2).Label());
        Labeled localLb = new CrossPkgLib_SensorжLabeled(sp2);
        fmt.Println(localLb.Label());
        CrossPkgLib.Labeled fgl = new dialжLabeled(Ꮡ(new dial(n: "psi"u8)));
        fmt.Println(fgl.Label(), CrossPkgLib.Describe(fgl));
        var (rp, rerr) = getReporter();
        fmt.Println(rp.Report(), rerr == default!);
        certificate ct = new cert(id: 42);
        fmt.Println(ct.Label(), ct.Seal(), ct.Rating(), ct.Serial());
        CrossPkgLib.Sealed sd = ct;
        CrossPkgLib.Rated rt = ct;
        fmt.Println(sd.Label(), rt.Rating());
        var pr = Ꮡ(new CrossPkgLib.Probe(nil));
        CrossPkgLib.Sampler sam = new CrossPkgLib.ProbeжSampler(pr);
        fmt.Println(sam.Sample(), sam.Sample(), (~pr).Hits);
        var h = Ꮡ(new holder<nint>(Cache: Ꮡ(new CrossPkgLib.Cache<nint>(nil)), name: "h"u8));
        fmt.Println(h.Value.Cache.Value.Bump(), h.Value.Cache.Value.Bump(), (~h).name);
        var mk = CrossPkgLib.MakeMarker(tagˢ);
        fmt.Println(mk.ΔΔMarker);
        CrossPkgLibꓸToken tok = CrossPkgLib.AsToken(42);
        fmt.Println(tokenˢ, tok);
        var wrapped = CrossPkgLib.Wrap<nint>(5);
        fmt.Println(len(wrapped), wrapped[0]);
        fmt.Println(CrossPkgLib.Pair<@string, nint>("k"u8, 8));
        Func<nint, bool> isHot = (nint tΔ1) => tΔ1 > 50;
        fmt.Println(isHot(60), isHot(40));
        var (nd, nerr) = CrossPkgLib.Resolve(new Func<map<@string, ж<CrossPkgLib.Node>>, @string, (ж<CrossPkgLib.Node>, error)>(simpleResolve), abcdeˢ);
        fmt.Println((~nd).ID, nerr == default!);
        sensorHolder.Value.item = Ꮡ(new CrossPkgLib.Sensor(Name: "garage"u8, Temp: 30D));
        fmt.Println((~(~sensorHolder).item).Name);
        var sbx = new sensorBox(tag: "b"u8);
        sbx.Holder.item = Ꮡ(new CrossPkgLib.Sensor(Name: "shed"u8, Temp: 40D));
        fmt.Println((~sbx.Holder.item).Name, sbx.tag);
        fmt.Println(leafˢ, leafEmitter.Emit());
        fmt.Println(branchˢ, branchEmitter.Emit());
        fmt.Println(describedLeafˢ, CrossPkgLib.DescribeEmitter(leafEmitter));
        fmt.Println(describedBranchˢ, CrossPkgLib.DescribeEmitter(branchEmitter));
        var scan = makeScanner("p:"u8);
        var (scanName, scanData, scanErr) = scan(helloˢ);
        fmt.Println(scanName, ((@string)scanData), scanErr == default!);
        var bbuf = slice<byte>("key"u8);
        bbuf = append(bbuf, (byte)(CrossPkgLib.Sep));
        var rbuf = new rune[]{(rune)'a'}.slice();
        rbuf = append(rbuf, (rune)(CrossPkgLib.Precision));
        fmt.Println(((@string)bbuf), len(rbuf), rbuf[1]);
        var sc = ((CrossPkgLib.Scored)((CrossPkgLib.Verdict)4));
        fmt.Println(verdictScoreˢ, sc.Score());
        sc = new talliesжScored(Ꮡ(new tallies(pts: 7)));
        fmt.Println(talliesScoreˢ, sc.Score());
        var cal = ((Action<ж<CrossPkgLib.Sensor>, CrossPkgLib.Celsius>)(CrossPkgLib.Calibrate));
        var mx = Ꮡ(new CrossPkgLib.Sensor(Name: "mx"u8, Temp: 10D));
        cal(mx, 4D);
        fmt.Println((float64)(~mx).Temp);
        var hot = ((Func<CrossPkgLib.Sensor, bool>)(CrossPkgLib.Hot));
        fmt.Println(hot(mx.Value), hot(new CrossPkgLib.Sensor(Temp: 60D)));
        var madd = ((Func<CrossPkgLib.Celsius, CrossPkgLib.Celsius, CrossPkgLib.Celsius>)(CrossPkgLib.Add));
        fmt.Println((float64)madd(2D, 3D));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct localCelsius /*num:float64*/;

partial struct reading /*global::go.CrossPkgLib_package.Celsius*/;

partial struct stamp /*global::go.CrossPkgLib_package.Ticks*/;

internal static stamp bigStamp => unchecked((stamp)(CrossPkgLib.Ticks)0x80000001);

internal static (stamp, error) stampOrErr(bool ok) {
    if (!ok) {
        return ((stamp)(CrossPkgLib.Ticks)(0), fmt.Errorf("no stamp"u8));
    }
    return ((stamp)(CrossPkgLib.Ticks)(bigStamp), default!);
}

internal static (ж<CrossPkgLib.Node>, error) simpleResolve(map<@string, ж<CrossPkgLib.Node>> imports, @string path) {
    return (Ꮡ(new CrossPkgLib.Node(ID: len(path))), default!);
}

partial struct probe {
    public partial ref ж<CrossPkgLib_package.Sensor> Sensor { get; }
    internal nint id;
}

partial struct tagged {
    public partial ref CrossPkgLib_package.Sensor Sensor { get; }
    internal nint n;
}

partial interface ΔMeter {
    nint Bump();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string taggedMeterˢ = "tagged-meter"u8;

internal static @string Meter(this tagged t) {
    return taggedMeterˢ;
}

partial struct counter {
    public partial ref ж<CrossPkgLib_package.Meter> Meter { get; }
}

partial struct rig {
    public partial ref CrossPkgLib_package.Device Device { get; }
    internal nint id;
}

partial struct tallies {
    internal nint pts;
}

internal static nint Score(this ref tallies t) {
    return t.pts;
}

} // end main_package

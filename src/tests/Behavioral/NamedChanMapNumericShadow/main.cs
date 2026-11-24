namespace go;

using fmt = fmt_package;
using slices = slices_package;

partial class main_package {

partial struct Pipe /*chan nint*/;

internal static nint Capacity⁔(/*this*/ Pipe p) { return -1; }
internal static nint Length⁔(/*this*/ Pipe p) { return -2; }
internal static @string IsClosed⁔(/*this*/ Pipe p) { return "never closed"; }
internal static @string IsUnbuffered⁔(/*this*/ Pipe p) { return "who knows"; }
internal static nint Receive⁔(/*this*/ Pipe p) { return 77; }
internal static @string Receiving⁔(/*this*/ Pipe p) { return "receiving"; }
internal static @string Sending⁔(/*this*/ Pipe p, nint v) { return fmt.Sprint("sending ", v); }
internal static bool Received⁔(/*this*/ Pipe p, nint v) { return v > 0; }
internal static @string Send⁔(/*this*/ Pipe p, nint v) { return fmt.Sprint("send ", v); }
internal static bool Sent⁔(/*this*/ Pipe p, nint v) { return v < 0; }
internal static Pipe Make⁔(/*this*/ Pipe p, nint n) { return new Pipe(n); }
internal static nint GetEnumerator⁔(/*this*/ Pipe p) { return 3; }

partial struct Counts /*map[@string, nint]*/;

internal static nint Length⁔(/*this*/ Counts c) { return 100 + len(c); }
internal static @string IsNil⁔(/*this*/ Counts c) { return "not nil"; }
internal static nint Count⁔(/*this*/ Counts c) { return -len(c); }
internal static nint Make⁔(/*this*/ Counts c) { return 1; }
internal static @string GetEnumerator⁔(/*this*/ Counts c) { return "map enumerator"; }
internal static nint Add⁔(/*this*/ Counts c, @string k) { return len(k); }
internal static bool Remove⁔(/*this*/ Counts c, @string k) { return k == ""u8; }

partial struct Level /*num:nint*/;

internal static nint CompareTo⁔(/*this*/ Level l, Level o) { return 42; }

partial struct Gauge /*num:float64*/;

internal static float64 Value⁔(/*this*/ Gauge g) { return (float64)g * 2D; }

partial struct Name /*@string*/;

internal static nint Length⁔(/*this*/ Name n) { return 500 + len(n); }
internal static nint CompareTo⁔(/*this*/ Name n, Name o) { return 43; }
internal static @string GetEnumerator⁔(/*this*/ Name n) { return "name enumerator"; }

partial struct Wire /*chan nint*/;

public static nint Capacities(this Wire w) { return 9000; }
public static nint Lengths(this Wire w) { return 9001; }

partial struct Tally /*map[@string, nint]*/;

internal static nint Counts⁔(/*this*/ Tally t) { return 9002; }

partial struct Rank /*num:nint*/;

public static nint CompareTwo(this Rank r, Rank o) { return 9003; }

partial struct Label /*@string*/;

public static nint Len(this Label l) { return 9004; }

partial class IsNull /*ж<nint>*/;
partial struct Capacity /*chan @string*/;
partial struct Count /*map[nint, bool]*/;
partial struct CompareTo /*num:nint*/;
partial struct Length /*@string*/;

partial struct Receive /*chan nint*/;
partial struct Send /*chan nint*/;

partial struct point {
    public nint X, Y;
}

/*[3]*/ partial class Value /*ж<array<nint>>*/;
partial class ValueSlot /*ж<slice<nint>>*/;
/*[2]*/ partial class IsNilPointer /*ж<array<nint>>*/;
partial class NilInstance /*ж<@string>*/;
partial class of /*ж<point>*/;
/*[3]*/ partial class at /*ж<array<nint>>*/;

partial class strPtr /*ж<@string>*/;

internal static void bump(Value p) {
    p.Value‿[2]++;
    p.Value‿.Assign(array<nint>([p.Value‿[0], p.Value‿[1], p.Value‿[2] * 10]));
}

internal static Value through(Value p) { return p; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bobˢ = "bob"u8;
private static readonly @string heyˢ = "hey"u8;
private static readonly @string fourˢ = "four"u8;

internal static void main() {
    var p = new Pipe(2);
    fmt.Println("Capacity/Length:", p.Capacity(), p.Length());
    fmt.Println("IsClosed/IsUnbuffered:", p.IsClosed(), p.IsUnbuffered());
    fmt.Println("Receive/Receiving/Sending/Received:", p.Receive(), p.Receiving(), p.Sending(4), p.Received(3));
    fmt.Println("Send/Sent:", p.Send(5), p.Sent(-1));
    var m = p.Make(5);
    fmt.Println("Make:", cap(m), "GetEnumerator:", p.GetEnumerator());
    
    var pʗ1 = p;
    var pc = () => pʗ1.Capacity();
    
    var pʗ2 = pʗ1;
    var pl = () => pʗ2.Length();
    
    var pʗ3 = pʗ2;
    var pic = () => pʗ3.IsClosed();
    fmt.Println("channel method values:", pc(), pl(), pic());

    p.ᐸꟷ(1);
    var pᴛ1 = p;
    switch (trySelect(ᐸꟷ<nint>(pᴛ1, ꓸꓸꓸ))) {
    case 0 when pᴛ1.ꟷᐳ(out var v): {
        fmt.Println("selected receive:", v);
        break;
    }
    default: {
        fmt.Println("no receive");
        break;
    }}
    p.ᐸꟷ(2);
    var pᴛ2 = p;
    switch (trySelect(ᐸꟷ<nint>(pᴛ2, ꓸꓸꓸ))) {
    case 0 when pᴛ2.ꟷᐳ(out var v, out var ok): {
        fmt.Println("selected receive ok:", v, ok);
        break;
    }
    default: {
        fmt.Println("no receive ok");
        break;
    }}
    var pᴛ3 = p.ᐸꟷ(9, ꓸꓸꓸ);
    switch (trySelect(pᴛ3)) {
    case 0: {
        fmt.Println("selected send");
        break;
    }
    default: {
        fmt.Println("no send");
        break;
    }}
    fmt.Println("builtin len/cap:", len(p), cap(p), ᐸꟷ<nint>(p));

    var q = new Pipe(3);
    q.ᐸꟷ(10);
    q.ᐸꟷ(20);
    q.ᐸꟷ(30);
    close<nint>(q);
    foreach (var v in q) {
        fmt.Println("range pipe:", v);
    }

    var c = new Counts(new map<@string, nint>{["a"] = 1, ["b"] = 2});
    fmt.Println("Counts:", c.Length(), c.IsNil(), c.Count(), c.Make(), c.GetEnumerator(), len(c));
    fmt.Println("Counts Add/Remove:", c.Add("xyz"), c.Remove("a"), len(c), c["a"]);
    
    var cʗ1 = c;
    var cl = () => cʗ1.Length();
    
    var cʗ2 = cʗ1;
    var cn = () => cʗ2.IsNil();
    fmt.Println("map method values:", cl(), cn());
    nint sum = 0;
    foreach (var (kΔ1, v) in c) {
        sum += len(kΔ1) + v;
    }
    fmt.Println("range counts:", sum);
    c["c"] = 3;
    delete(c, "a"u8);
    fmt.Println("map writes:", len(c), c["c"], c["a"]);

    Level a = 1;
    Level b = 2;
    fmt.Println("CompareTo:", a.CompareTo(b), a < b, min(b, a), max(a, b));
    
    var lv = (Level p1) => a.CompareTo(p1);
    fmt.Println("numeric method value:", lv(b));
    var levels = slice<Level>([3, 1, 2]);
    slices.Sort<slice<Level>, Level>(levels);
    fmt.Println("sorted levels:", levels);

    var g = (Gauge)1.5D;
    
    var gv = () => g.Value();
    fmt.Println("Gauge Value:", g.Value(), gv(), g + 1D);

    Name n = (Name)(@string)bobˢ;
    Name o = (Name)(@string)"al";
    fmt.Println("Name:", n.Length(), n.CompareTo(o), n.GetEnumerator(), len(n), n < o, min(n, o), n + o);
    
    var nl = () => n.Length();
    fmt.Println("string method value:", nl());
    foreach (var (i, r) in n) {
        fmt.Println("range name:", i, (@string)r);
    }
    var names = slice<Name>([(@string)"c", (@string)"a", (@string)"b"]);
    slices.Sort<slice<Name>, Name>(names);
    fmt.Println("sorted names:", names, n[1..]);

    var w = new Wire(4);
    w.ᐸꟷ(1);
    fmt.Println("near-miss channel:", w.Capacities(), w.Lengths(), len(w), cap(w), ᐸꟷ<nint>(w));
    var t = new Tally(new map<@string, nint>{["x"] = 1});
    fmt.Println("near-miss map:", t.Counts(), len(t), t["x"]);
    Rank r1 = (Rank)5;
    Rank r2 = (Rank)4;
    fmt.Println("near-miss numeric:", r1.CompareTwo(r2), min(r1, r2), r1 > r2);
    Label lb = (Label)(@string)heyˢ;
    fmt.Println("near-miss string:", lb.Len(), len(lb), lb[1..]);
    foreach (var (i, r) in lb) {
        fmt.Println("near-miss range:", i, (@string)r);
    }

    ref var x = ref heap<nint>(5, out var Ꮡx);
    IsNull @in = Ꮡx;
    @in.Value = 6;
    fmt.Println("type IsNull:", @in.Value, @in != nil, x);
    var cc = new Capacity(1);
    cc.ᐸꟷ("c");
    fmt.Println("type Capacity:", len(cc), ᐸꟷ<@string>(cc));
    var k = new Count(new map<nint, bool>{[1] = true});
    fmt.Println("type Count:", len(k), k[1]);
    CompareTo ct = 4;
    fmt.Println("type CompareTo:", ct < 5, min(ct, (CompareTo)2));
    Length ln = fourˢ;
    fmt.Println("type Length:", len(ln), ln < "z"u8, ln[1..]);
    var rc = new Receive(1);
    rc.ᐸꟷ(5);
    var rcᴛ1 = rc;
    switch (trySelect(ᐸꟷ<nint>(rcᴛ1, ꓸꓸꓸ))) {
    case 0 when rcᴛ1.ꟷᐳ(out var v): {
        fmt.Println("type Receive selected:", v);
        break;
    }
    default: {
        fmt.Println("type Receive: none");
        break;
    }}
    rc.ᐸꟷ(6);
    var sc = new Send(1);
    sc.ᐸꟷ(ᐸꟷ<nint>(rc));
    fmt.Println("type Receive/Send:", len(rc), ᐸꟷ<nint>(sc));

    ref var v1 = ref heap<array<nint>>(array<nint>([7, 8, 9]), out var Ꮡv1);
    Value pv = Ꮡv1;
    (pv.Value‿)[0] += 1;
    pv.Value‿[1] = 80;
    bump(pv);
    nint sum3 = 0;
    foreach (var (i, e) in range(pv.Value‿)) {
        sum3 += i * e;
    }
    fmt.Println("type Value:", pv.Value‿, v1, pv.Value‿[..2], 3, sum3, pv != nil);
    fmt.Println("type Value len/cap through a call:", len(through(pv).Value‿), cap(through(pv).Value‿));
    ref var sl = ref heap<slice<nint>>(slice<nint>([1, 2]), out var Ꮡsl);
    ValueSlot ps = Ꮡsl;
    ps.ΔValueSlot = append(ps.ΔValueSlot, (nint)3);
    fmt.Println("type ValueSlot:", len(ps.ΔValueSlot), sl);
    ref var arr = ref heap<array<nint>>(array<nint>([4, 5]), out var Ꮡarr);
    IsNilPointer pn = Ꮡarr;
    fmt.Println("type IsNilPointer:", pn.Value[1], 2, pn != nil);
    NilInstance ni = default;
    any boxed = ((NilInstance)nil);
    var (_, isNilInstance) = boxed._<NilInstance>(ᐧ);
    any boxedSp = ((strPtr)nil);
    var (_, isStrPtr) = boxedSp._<strPtr>(ᐧ);
    ref var s5 = ref heap<@string>("five", out var Ꮡs5);
    ni = Ꮡs5;
    fmt.Println("typed nil:", boxed, boxed != nil, isNilInstance, boxedSp != nil, isStrPtr, ni != nil, ni.Value);
    ref var pt = ref heap<point>(new point(1, 2), out var Ꮡpt);
    of po = Ꮡpt;
    po.X = 10;
    var px = po.Δof(point.ᏑY);
    px.Value = 20;
    fmt.Println("type of:", po.X, po.Y, pt);
    ref var three = ref heap<array<nint>>(array<nint>([7, 8, 9]), out var Ꮡthree);
    at pa = Ꮡthree;
    pa.Value[0] = 70;
    var pe = pa.Δat<nint>(2);
    pe.Value = 90;
    fmt.Println("type at:", pa.Value[0], pa.Value[1], three, pa.Value[1..], len(pa.Value[..2]));
}

} // end main_package

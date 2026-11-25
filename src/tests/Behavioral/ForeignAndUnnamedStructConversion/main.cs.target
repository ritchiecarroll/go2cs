namespace go;

using fmt = fmt_package;
using lib = ForeignAndUnnamedStructConversion.lib_package;
using other = ForeignAndUnnamedStructConversion.other_package;
using ForeignAndUnnamedStructConversion;
using ꓸꓸꓸanonBox_V = variadic<main_package.anonBox_V>;
using ꓸꓸꓸlibꓸPitch = variadic<ForeignAndUnnamedStructConversion.lib_package.Pitch>;

partial class main_package {

public partial struct hertz {
    public nint Hz;
}

internal static nint half(this hertz h) { return h.Hz / 2; }

partial struct Hertz {
    public nint Hz;
}

partial struct boxed /*hertz*/;

partial struct note {
    public @string Name;
    public lib.Pitch Pitch;
    public slice<nint> Marks;
    public ж<lib.Note> Next;
    public float64 Weight;
}

internal static nint show(hertz h) { return h.Hz; }

internal static hertz made() { return (hertz)lib.Middle(); }

internal static void foreignToLocal() {
    ref var p = ref heap<lib.Pitch>(new lib.Pitch(Hz: 440), out var Ꮡp);

    ref var h = ref heap<hertz>((hertz)p, out var Ꮡh);
    h.Hz++;
    fmt.Println("foreign to local:", h.Hz, p.Hz, h.half());
    var H = (Hertz)p;
    H.Hz += 2;
    fmt.Println("foreign to exported local:", H.Hz, p.Hz);

    var back = (lib.Pitch)h;
    fmt.Println("local to foreign:", back.Hz, back.Double());

    var o = new other.Pitch(Hz: 7);
    fmt.Println("a second package's Pitch:", ((hertz)o).Hz, ((other.Pitch)h).Hz);

    var b = (boxed)p;
    fmt.Println("a type defined over the local:", b.Hz, ((lib.Pitch)b).Hz);

    fmt.Println("argument, result, call operand:", show((hertz)p), made().Hz, ((hertz)lib.Middle()).half());

    var n = new lib.Note(Name: "A", Pitch: p, Marks: [1, 2], Next: Ꮡ(new lib.Note(Name: "B")), Weight: 1.5D);
    var m = (note)n;
    m.Marks[0] = 9;
    m.Next.Name = "C";
    m.Name = "a";
    fmt.Println("several fields:", m.Name, n.Name, m.Pitch.Hz, n.Marks[0], n.Next.Name, m.Weight);
    var n2 = (lib.Note)m;
    fmt.Println("several fields back:", n2.Name, len(n2.Marks), n2.Next.Name);

    var ph = Ꮡp.Reinterpret<lib.Pitch, hertz>();
    ph.Hz = 100;
    fmt.Println("pointer, foreign to local:", p.Hz, ph.half());
    var pp = Ꮡh.Reinterpret<hertz, lib.Pitch>();
    pp.Hz = 200;
    fmt.Println("pointer, local to foreign:", h.Hz, pp.Double());
}

internal partial struct convLocal_x /*dyn*/ {
    public nint Hz;
}

internal static nint convLocal(hertz h) {
    var x = (convLocal_x)h;
    x.Hz++;
    return x.Hz + h.Hz;
}

internal static nint convForeign(lib.Pitch p) {
    var x = (convLocal_x)p;
    x.Hz++;
    return x.Hz + p.Hz;
}

internal static nint convOperand(lib.Pitch p) { return ((convLocal_x)p).Hz; }

internal static nint takeAnon(convLocal_x a) { return a.Hz; }

partial struct anonBox_V /*dyn*/ {
    public nint Hz;
}

partial struct anonBox {
    public anonBox_V V;
}

internal static nint varLocal(hertz h) {
    anonBox_V x = h;
    return x.Hz;
}

internal static nint varForeign(lib.Pitch p) {
    anonBox_V x = p;
    return x.Hz;
}

internal static nint assignLocal(hertz h) {
    anonBox_V x = default;
    x = h;
    return x.Hz;
}
internal static nint assignForeign(lib.Pitch p) {
    anonBox_V x = default;
    x = p;
    return x.Hz;
}
internal static nint argLocal(hertz h) { return takeAnon(h); }
internal static nint argForeign(lib.Pitch p) { return takeAnon(p); }
internal static anonBox_V retLocal(hertz h) { return h; }
internal static anonBox_V retForeign(lib.Pitch p) { return p; }

internal static nint fieldLocal(hertz h) {
    var b = new anonBox(V: h);
    return b.V.Hz;
}

internal static nint fieldForeign(lib.Pitch p) {
    var b = new anonBox(V: p);
    return b.V.Hz;
}

internal static nint elemLocal(hertz h) {
    var s = slice<anonBox_V>([h]);
    return s[0].Hz;
}

internal static nint elemForeign(lib.Pitch p) {
    var s = slice<anonBox_V>([p]);
    return s[0].Hz;
}

internal static nint mapLocal(hertz h) {
    var m = new map<@string, anonBox_V>{["k"] = h};
    return m["k"].Hz;
}

internal static nint mapForeign(lib.Pitch p) {
    var m = new map<@string, anonBox_V>{["k"] = p};
    return m["k"].Hz;
}

internal static nint sendLocal(hertz h) {
    var c = new channel<anonBox_V>(1);
    c.ᐸꟷ(h);
    return (ᐸꟷ(c)).Hz;
}
internal static nint sendForeign(lib.Pitch p) {
    var c = new channel<anonBox_V>(1);
    c.ᐸꟷ(p);
    return (ᐸꟷ(c)).Hz;
}

internal static nint takeLocal(hertz h) { return h.Hz; }
internal static nint takeForeign(lib.Pitch p) { return p.Double(); }

partial struct localBox {
    public hertz V;
}
partial struct foreignBox {
    public lib.Pitch V;
}

internal static nint backConvLocal(anonBox_V a) { return ((hertz)a).half(); }
internal static nint backConvForeign(anonBox_V a) { return ((lib.Pitch)a).Double(); }

internal static nint backVarLocal(anonBox_V a) {
    hertz x = a;
    return x.half();
}

internal static nint backVarForeign(anonBox_V a) {
    lib.Pitch x = a;
    return x.Double();
}

internal static nint backAssignLocal(anonBox_V a) {
    hertz x = default;
    x = a;
    return x.half();
}
internal static nint backAssignForeign(anonBox_V a) {
    lib.Pitch x = default;
    x = a;
    return x.Double();
}
internal static nint backArgLocal(anonBox_V a) { return takeLocal(a); }
internal static nint backArgForeign(anonBox_V a) { return takeForeign(a); }
internal static hertz backRetLocal(anonBox_V a) { return a; }
internal static lib.Pitch backRetForeign(anonBox_V a) { return a; }

internal static nint backFieldLocal(anonBox_V a) {
    var b = new localBox(V: a);
    return b.V.half();
}

internal static nint backFieldForeign(anonBox_V a) {
    var b = new foreignBox(V: a);
    return b.V.Double();
}

internal static nint backElemLocal(anonBox_V a) {
    var s = slice<hertz>([a]);
    return s[0].half();
}

internal static nint backElemForeign(anonBox_V a) {
    var s = slice<lib.Pitch>([a]);
    return s[0].Double();
}

internal static nint backMapLocal(anonBox_V a) {
    var m = new map<@string, hertz>{["k"] = a};
    return m["k"].half();
}

internal static nint backMapForeign(anonBox_V a) {
    var m = new map<@string, lib.Pitch>{["k"] = a};
    return m["k"].Double();
}
internal static nint backSendLocal(anonBox_V a) {
    var c = new channel<hertz>(1);
    c.ᐸꟷ(a);
    return (ᐸꟷ(c)).half();
}
internal static nint backSendForeign(anonBox_V a) {
    var c = new channel<lib.Pitch>(1);
    c.ᐸꟷ(a);
    return (ᐸꟷ(c)).Double();
}

internal static (bool, bool) sameLocal(anonBox_V a, hertz h) { return ((hertz)a == h, h != (hertz)a); }
internal static (bool, bool) sameForeign(anonBox_V a, lib.Pitch p) { return ((lib.Pitch)a == p, p != (lib.Pitch)a); }

internal static hertz pkgHertz = new hertz(Hz: 3);
internal static anonBox_V pkgAnon = pkgHertz;

partial struct taker {
}

internal static nint anon(this taker _, anonBox_V a) { return a.Hz; }
internal static nint named(this taker _, lib.Pitch p) { return p.Double(); }

internal static nint takeAnons(params ꓸꓸꓸanonBox_V xsʗp) {
    var xs = xsʗp.sslice();

    nint n = 0;
    foreach (var (_, x) in range(xs)) {
        n += x.Hz;
    }
    return n;
}

internal static nint setFieldLocal(hertz h) {
    anonBox b = default;
    b.V = h;
    return b.V.Hz;
}

internal static nint setElemForeign(lib.Pitch p) {
    var s = new slice<anonBox_V>(1);
    s[0] = p;
    return s[0].Hz;
}
internal static nint setMapLocal(hertz h) {
    var m = new map<@string, anonBox_V>{};
    m["k"] = h;
    return m["k"].Hz;
}
internal static nint setDerefForeign(lib.Pitch p) {
    ref var x = ref heap(new anonBox_V(), out var Ꮡx);
    var q = Ꮡx;
    q.Value = p;
    return x.Hz;
}
internal static nint variadicLocal(hertz h) { return takeAnons(h, h); }
internal static nint methodForeign(lib.Pitch p) { return new taker().anon(p); }

internal static nint funcValueLocal(hertz h) {
    var f = takeAnon;
    return f(h);
}

internal static nint /*n*/ deferForeign(lib.Pitch p) {
    nint n = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("deferForeign.func1")] (anonBox_V a) => {
            n = a.Hz;
        }, p, ref ᒐ);
        n = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return n;
}
internal static partial nint goLocal(hertz h) {
    var c = new channel<nint>(0);
    var cʗ1 = c;
    goǃ([GoFunc("goLocal.func1")] (anonBox_V a) => {
        cʗ1.ᐸꟷ(a.Hz);
    }, h);
    return ᐸꟷ(c);
}
internal static (anonBox_V, error) twoForeign(lib.Pitch p) { return (p, default); }
internal static nint literalLocal(hertz h) {
    var hʗ1 = h;
    [GoFunc("literalLocal.func1")] anonBox_V f() => hʗ1;
    return f().Hz;
}

internal static nint positionalForeign(lib.Pitch p) {
    var b = new anonBox(p);
    return b.V.Hz;
}

internal static nint arrayLocal(hertz h) {
    var s = array<anonBox_V>([h, h]);
    return s[1].Hz;
}

internal static nint keyForeign(lib.Pitch p) {
    var m = new map<anonBox_V, nint>{[p] = 3};
    return m[new anonBox_V(Hz: p.Hz)];
}
internal static nint indexLocal(hertz h) {
    var m = new map<anonBox_V, nint>{[new(Hz: h.Hz)] = 4};
    return m[h];
}
internal static nint deleteForeign(lib.Pitch p) {
    var m = new map<anonBox_V, nint>{[new(Hz: p.Hz)] = 4, [new(Hz: 1)] = 5};
    delete(m, p);
    return len(m);
}
internal static nint appendLocal(hertz h) {
    var sᴷ = new stackAppend<anonBox_V>();

    slice<anonBox_V> s = default;
    s = append(sᴷ, s, h, h);
    return len(s) + s[1].Hz;
}
internal static nint selectForeign(lib.Pitch p) {
    var c = new channel<anonBox_V>(1);
    var cᴛ1 = c.ᐸꟷ(p, ꓸꓸꓸ);
    switch (trySelect(cᴛ1)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    return (ᐸꟷ(c)).Hz;
}
internal static nint receiveLocal(hertz h) {
    var c = new channel<hertz>(1);
    c.ᐸꟷ(h);
    anonBox_V x = default;
    x = ᐸꟷ(c);
    return x.Hz;
}
internal static nint receiveOkForeign(lib.Pitch p) {
    var c = new channel<lib.Pitch>(1);
    c.ᐸꟷ(p);
    anonBox_V x = default;
    bool ok = default;
    (x, ok) = ᐸꟷ(c, ꟷ);
    if (!ok) {
        return -1;
    }
    return x.Hz;
}
internal static nint mapOkLocal(hertz h) {
    var m = new map<@string, hertz>{["k"] = h};
    anonBox_V x = default;
    bool ok = default;
    (x, ok) = m["k", ꟷ];
    if (!ok) {
        return -1;
    }
    return x.Hz;
}
internal static (lib.Pitch, bool) pitchAndTrue(lib.Pitch p) { return (p, true); }
internal static nint resultsForeign(lib.Pitch p) {
    anonBox_V x = default;
    bool ok = default;
    (x, ok) = pitchAndTrue(p);
    if (!ok) {
        return -1;
    }
    return x.Hz;
}
internal static nint rangeLocal(hertz h) {
    anonBox_V x = default;
    foreach (var (_, vᴛ1) in range(slice<hertz>([h, new(Hz: h.Hz + 1)]))) {
        x = vᴛ1;

    }
    return x.Hz;
}
internal static nint assertForeign(lib.Pitch p) {
    any i = p;
    anonBox_V x = default;
    x = i._<lib.Pitch>();
    return x.Hz;
}

internal static anonBox_V pkgUnnamed = new anonBox_V(Hz: 4);
internal static lib.Pitch pkgPitch = pkgUnnamed;

internal static nint takePitches(params ꓸꓸꓸlibꓸPitch xsʗp) {
    var xs = xsʗp.sslice();

    nint n = 0;
    foreach (var (_, x) in range(xs)) {
        n += x.Hz;
    }
    return n;
}

internal static nint local(this taker _, hertz h) { return h.half(); }

internal static nint backSetFieldForeign(anonBox_V a) {
    foreignBox b = default;
    b.V = a;
    return b.V.Double();
}

internal static nint backSetElemLocal(anonBox_V a) {
    var s = new slice<hertz>(1);
    s[0] = a;
    return s[0].half();
}
internal static nint backSetMapForeign(anonBox_V a) {
    var m = new map<@string, lib.Pitch>{};
    m["k"] = a;
    return m["k"].Double();
}
internal static nint backSetDerefLocal(anonBox_V a) {
    ref var x = ref heap(new hertz(), out var Ꮡx);
    var q = Ꮡx;
    q.Value = a;
    return x.half();
}
internal static nint backVariadicForeign(anonBox_V a) { return takePitches(a, a); }
internal static nint backMethodLocal(anonBox_V a) { return new taker().local(a); }

internal static nint backFuncValueForeign(anonBox_V a) {
    var f = takeForeign;
    return f(a);
}

internal static nint /*n*/ backDeferLocal(anonBox_V a) {
    nint n = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("backDeferLocal.func1")] (hertz h) => {
            n = h.half();
        }, a, ref ᒐ);
        n = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return n;
}
internal static partial nint backGoForeign(anonBox_V a) {
    var c = new channel<nint>(0);
    var cʗ1 = c;
    goǃ([GoFunc("backGoForeign.func1")] (lib.Pitch p) => {
        cʗ1.ᐸꟷ(p.Double());
    }, a);
    return ᐸꟷ(c);
}
internal static (hertz, error) backTwoLocal(anonBox_V a) { return (a, default); }
internal static nint backLiteralForeign(anonBox_V a) {
    var aʗ1 = a;
    [GoFunc("backLiteralForeign.func1")] lib.Pitch f() => aʗ1;
    return f().Double();
}

internal static nint backPositionalLocal(anonBox_V a) {
    var b = new localBox(a);
    return b.V.half();
}

internal static nint backArrayForeign(anonBox_V a) {
    var s = array<lib.Pitch>([a, a]);
    return s[1].Double();
}

internal static nint backKeyLocal(anonBox_V a) {
    var m = new map<hertz, nint>{[a] = 3};
    return m[new hertz(Hz: a.Hz)];
}
internal static nint backIndexForeign(anonBox_V a) {
    var m = new map<lib.Pitch, nint>{[new(Hz: a.Hz)] = 4};
    return m[a];
}
internal static nint backDeleteLocal(anonBox_V a) {
    var m = new map<hertz, nint>{[new(Hz: a.Hz)] = 4, [new(Hz: 1)] = 5};
    delete(m, a);
    return len(m);
}
internal static nint backAppendForeign(anonBox_V a) {
    var sᴷ = new stackAppend<lib.Pitch>();

    slice<lib.Pitch> s = default;
    s = append(sᴷ, s, (lib.Pitch)(a), (lib.Pitch)(a));
    return len(s) + s[1].Double();
}
internal static nint backSelectLocal(anonBox_V a) {
    var c = new channel<hertz>(1);
    var cᴛ1 = c.ᐸꟷ(a, ꓸꓸꓸ);
    switch (trySelect(cᴛ1)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    return (ᐸꟷ(c)).half();
}
internal static nint backReceiveForeign(anonBox_V a) {
    var c = new channel<anonBox_V>(1);
    c.ᐸꟷ(a);
    lib.Pitch x = default;
    x = ᐸꟷ(c);
    return x.Double();
}
internal static nint backReceiveOkLocal(anonBox_V a) {
    var c = new channel<anonBox_V>(1);
    c.ᐸꟷ(a);
    hertz x = default;
    bool ok = default;
    (x, ok) = ᐸꟷ(c, ꟷ);
    if (!ok) {
        return -1;
    }
    return x.half();
}
internal static nint backMapOkForeign(anonBox_V a) {
    var m = new map<@string, anonBox_V>{["k"] = a};
    lib.Pitch x = default;
    bool ok = default;
    (x, ok) = m["k", ꟷ];
    if (!ok) {
        return -1;
    }
    return x.Double();
}
internal static (anonBox_V, bool) unnamedAndTrue(anonBox_V a) { return (a, true); }
internal static nint backResultsLocal(anonBox_V a) {
    hertz x = default;
    bool ok = default;
    (x, ok) = unnamedAndTrue(a);
    if (!ok) {
        return -1;
    }
    return x.half();
}
internal static nint backRangeForeign(anonBox_V a) {
    lib.Pitch x = default;
    foreach (var (_, vᴛ1) in range(slice<anonBox_V>([a, new(Hz: a.Hz + 1)]))) {
        x = vᴛ1;

    }
    return x.Double();
}
internal static nint backAssertLocal(anonBox_V a) {
    any i = a;
    hertz x = default;
    x = i._<anonBox_V>();
    return x.half();
}

internal static nint caseLocal(anonBox_V a, hertz h) {
    switch (h) {
    case var ᴛ when ᴛ == (hertz)a: {
        return 1;
    }}

    return 0;
}
internal static nint caseTagForeign(anonBox_V a, lib.Pitch p) {
    switch (a) {
    case var ᴛ when (lib.Pitch)ᴛ == p: {
        return 1;
    }}

    return 0;
}

internal static void everyOtherPlace() {
    var h = new hertz(Hz: 220);
    var p = new lib.Pitch(Hz: 440);
    var a = new anonBox_V(Hz: 8);

    fmt.Println("package variable:", pkgAnon.Hz, pkgPitch.Double());
    fmt.Println("assign to field, element, map entry, through pointer:", setFieldLocal(h), setElemForeign(p), setMapLocal(h), setDerefForeign(p));
    fmt.Println("variadic, method, function value:", variadicLocal(h), methodForeign(p), funcValueLocal(h));
    fmt.Println("deferred and go-statement argument:", deferForeign(p), goLocal(h));
    var (two, err) = twoForeign(p);
    fmt.Println("one of two results, a literal's result:", two.Hz, err, literalLocal(h));
    fmt.Println("positional field, array element:", positionalForeign(p), arrayLocal(h));
    fmt.Println("map key, index, delete, append:", keyForeign(p), indexLocal(h), deleteForeign(p), appendLocal(h));
    fmt.Println("select send, receive, comma-ok receive and map read:", selectForeign(p), receiveLocal(h), receiveOkForeign(p), mapOkLocal(h));
    fmt.Println("two results, range, assertion:", resultsForeign(p), rangeLocal(h), assertForeign(p));

    fmt.Println("named: assign to field, element, map entry, through pointer:", backSetFieldForeign(a), backSetElemLocal(a), backSetMapForeign(a), backSetDerefLocal(a));
    fmt.Println("named: variadic, method, function value:", backVariadicForeign(a), backMethodLocal(a), backFuncValueForeign(a));
    fmt.Println("named: deferred and go-statement argument:", backDeferLocal(a), backGoForeign(a));
    (var twoBack, err) = backTwoLocal(a);
    fmt.Println("named: one of two results, a literal's result:", twoBack.half(), err, backLiteralForeign(a));
    fmt.Println("named: positional field, array element:", backPositionalLocal(a), backArrayForeign(a));
    fmt.Println("named: map key, index, delete, append:", backKeyLocal(a), backIndexForeign(a), backDeleteLocal(a), backAppendForeign(a));
    fmt.Println("named: select send, receive, comma-ok receive and map read:", backSelectLocal(a), backReceiveForeign(a), backReceiveOkLocal(a), backMapOkForeign(a));
    fmt.Println("named: two results, range, assertion:", backResultsLocal(a), backRangeForeign(a), backAssertLocal(a));

    fmt.Println("switch:", caseLocal(a, h), caseLocal(a, new hertz(Hz: 8)), caseTagForeign(a, p), caseTagForeign(a, new lib.Pitch(Hz: 8)));
}

internal static void namedAndUnnamed() {
    ref var h = ref heap<hertz>(new hertz(Hz: 220), out var Ꮡh);
    ref var p = ref heap<lib.Pitch>(new lib.Pitch(Hz: 440), out var Ꮡp);
    ref var a = ref heap(new anonBox_V(), out var Ꮡa);
    a.Hz = 8;

    fmt.Println("convert to unnamed:", convLocal(h), convForeign(p), convOperand(p), h.Hz, p.Hz);
    fmt.Println("declare:", varLocal(h), varForeign(p));
    fmt.Println("assign:", assignLocal(h), assignForeign(p));
    fmt.Println("argument:", argLocal(h), argForeign(p));
    fmt.Println("result:", retLocal(h).Hz, retForeign(p).Hz);
    fmt.Println("field:", fieldLocal(h), fieldForeign(p));
    fmt.Println("element:", elemLocal(h), elemForeign(p));
    fmt.Println("map value:", mapLocal(h), mapForeign(p));
    fmt.Println("send:", sendLocal(h), sendForeign(p));

    fmt.Println("convert to named:", backConvLocal(a), backConvForeign(a));
    fmt.Println("declare named:", backVarLocal(a), backVarForeign(a));
    fmt.Println("assign named:", backAssignLocal(a), backAssignForeign(a));
    fmt.Println("argument named:", backArgLocal(a), backArgForeign(a));
    fmt.Println("result named:", backRetLocal(a).Hz, backRetForeign(a).Hz);
    fmt.Println("field named:", backFieldLocal(a), backFieldForeign(a));
    fmt.Println("element named:", backElemLocal(a), backElemForeign(a));
    fmt.Println("map value named:", backMapLocal(a), backMapForeign(a));
    fmt.Println("send named:", backSendLocal(a), backSendForeign(a));

    var (eq, ne) = sameLocal(a, h);
    fmt.Println("compare local:", eq, ne);
    (eq, ne) = sameLocal(a, new hertz(Hz: 8));
    fmt.Println("compare local, equal:", eq, ne);
    (eq, ne) = sameForeign(a, p);
    fmt.Println("compare foreign:", eq, ne);
    (eq, ne) = sameForeign(a, new lib.Pitch(Hz: 8));
    fmt.Println("compare foreign, equal:", eq, ne);

    var pa = Ꮡh.Reinterpret<hertz, anonBox_V>();
    pa.Value.Hz = 5;
    var pf = Ꮡp.Reinterpret<lib.Pitch, anonBox_V>();
    pf.Value.Hz = 6;
    fmt.Println("pointer to unnamed:", h.Hz, p.Hz);
    var ph = Ꮡa.Reinterpret<anonBox_V, hertz>();
    ph.Hz = 10;
    var pp = Ꮡa.Reinterpret<anonBox_V, lib.Pitch>();
    fmt.Println("pointer to named:", a.Hz, ph.half(), pp.Double());
}

internal partial struct liftVar_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftVar() {
    liftVar_x x = laterTempo();
    x.Bpm++;
    return x.Bpm + laterTempo().Bpm;
}

internal partial struct liftAssign_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftAssign() {
    liftAssign_x x = default;
    x = laterTempo();
    return x.Bpm;
}

internal partial struct liftField_v /*dyn*/ {
    public nint Bpm;
}

internal static nint liftField() {
    liftField_v v = default;
    v.Bpm = 70;
    var b = new laterBox(V: v);
    b.V.Bpm++;
    return b.V.Bpm + v.Bpm;
}

internal partial struct liftSetField_v /*dyn*/ {
    public nint Bpm;
}

internal static nint liftSetField() {
    liftSetField_v v = default;
    v.Bpm = 71;
    laterBox b = default;
    b.V = v;
    return b.V.Bpm;
}

internal partial struct liftElements_s /*dyn*/ {
    public nint Bpm;
}

internal static nint liftElements() {
    var sᴷ = new stackAppend<liftElements_s>();

    var s = slice<liftElements_s>([laterTempo()]);
    var m = new map<liftElements_s, nint>{[laterTempo()] = 1};
    s = append(sᴷ, s, new liftElements_s(laterTempo().Bpm));
    return s[0].Bpm + m[laterTempo()] + len(s);
}

internal partial struct liftRange_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftRange() {
    liftRange_x x = default;
    nint n = 0;

    foreach (var (_, vᴛ1) in range(laterTempos())) {
        x = vᴛ1;

        n += x.Bpm;
    }

    return n;
}

internal partial struct liftReceive_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftReceive() {
    liftReceive_x x = default;
    bool ok = default;
    (x, ok) = ᐸꟷ(laterChan(), ꟷ);

    if (!ok) {
        return -1;
    }

    return x.Bpm;
}

internal partial struct liftTuple_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftTuple() {
    liftTuple_x x = default;
    bool ok = default;
    (x, ok) = laterPair();

    if (!ok) {
        return -1;
    }

    return x.Bpm;
}

internal partial struct liftArgument_v /*dyn*/ {
    public nint Bpm;
}

internal static nint liftArgument() {
    liftArgument_v v = default;
    v.Bpm = 72;
    return laterTake(v);
}

internal partial struct liftCompare_x /*dyn*/ {
    public nint Bpm;
}

internal static (bool, bool) liftCompare(nint bpm) {
    liftCompare_x x = default;
    x.Bpm = bpm;
    return (x == (liftCompare_x)laterTempo(), (liftCompare_x)laterTempo() != x);
}

internal partial struct liftSwitch_x /*dyn*/ {
    public nint Bpm;
}

internal static nint liftSwitch(nint bpm) {
    liftSwitch_x x = default;
    x.Bpm = bpm;
    nint n = 0;

    switch (x) {
    case var ᴛ when ᴛ == (liftSwitch_x)laterTempo(): {
        n++;
        break;
    }}

    switch (laterTempo()) {
    case var ᴛ when (liftSwitch_x)ᴛ == x: {
        n += 2;
        break;
    }}

    return n;
}

internal partial struct laterTempo_r /*dyn*/ {
    public nint Bpm;
}

internal static laterTempo_r /*r*/ laterTempo() {
    laterTempo_r r = default;

    r.Bpm = 60;
    return r;
}

internal static slice<laterTempo_r> laterTempos() {
    return [new(Bpm: 1), new(Bpm: 2)];
}

internal static channel<laterTempo_r> laterChan() {
    var c = new channel<laterTempo_r>(1);
    c.ᐸꟷ(laterTempo());
    return c;
}

internal static (laterTempo_r r, bool ok) laterPair() {
    laterTempo_r r = default;

    r.Bpm = 61;
    return (r, true);
}

internal static nint laterTake(laterTempo_r x) { return x.Bpm; }

partial struct laterBox_V /*dyn*/ {
    public nint Bpm;
}

partial struct laterBox {
    public laterBox_V V;
}

internal static void twoLifts() {
    fmt.Println("lifts: var, assign:", liftVar(), liftAssign());
    fmt.Println("lifts: field, set field:", liftField(), liftSetField());
    fmt.Println("lifts: elements, range:", liftElements(), liftRange());
    fmt.Println("lifts: receive, two results, argument:", liftReceive(), liftTuple(), liftArgument());
    var (eq, ne) = liftCompare(60);
    fmt.Println("lifts: compare, equal:", eq, ne);
    (eq, ne) = liftCompare(59);
    fmt.Println("lifts: compare:", eq, ne);
    fmt.Println("lifts: switch:", liftSwitch(60), liftSwitch(59));
}

internal static void main() {
    foreignToLocal();
    namedAndUnnamed();
    everyOtherPlace();
    twoLifts();
}

} // end main_package

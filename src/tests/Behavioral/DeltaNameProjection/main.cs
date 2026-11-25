namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using lib = DeltaNameProjection.lib_package;
using DeltaNameProjection;
using encoding;

partial class main_package {

partial struct P {
    [GoName("Δt")] public float64 Δt;
}

public partial struct ΔMatrix {
    internal nint n;
}

partial struct Matrix {
    internal nint m;
}

partial interface Namer {
    @string Name();
}

public static @string Name(this ΔMatrix m) { return fmt.Sprint("ΔMatrix ", m.n); }

public static @string Name(this Matrix m) { return fmt.Sprint("Matrix ", m.m); }

partial struct S {
}

[GoName("ΔFoo")] public static @string ΔFoo(this S _) { return "ΔFoo called"; }

partial interface Fooer {
    @string Foo();
}

public partial struct Δu {
    [GoName("Δu")] public nint ΔΔu;
}

public partial struct Δv {
    internal nint v;
}

partial struct R {
    internal nint Δslice;
}

partial struct L {
    internal nint δt;
    public nint X;
}

public partial struct ΔEmb {
    internal nint e;
}

partial struct Outer {
    [GoName("ΔEmb")] /*embed*/ public ΔEmb ΔEmb;
    internal nint k;
}

partial struct Pair {
    internal nint a, b;
}

partial struct Δslice {
    [GoName("Δslice")] public nint ΔΔslice;
}

partial struct Node {
    public nint ΔNode;
}

public partial interface ΔFooer {
    [GoName("ΔFoo")] @string ΔFoo();
}

partial struct Wrap {
    [GoName("ΔFooer")] /*embed*/ public ΔFooer ΔFooer;
}

partial struct T {
    internal nint n;
}

[GoName("ΔString")] public static @string ΔString(this T _) { return "never"; }

public static void Bump(this ref ΔMatrix m) { m.n++; }

[GoName("ΔScale")] public static void ΔScale(this ref P p, float64 k) { p.Δt *= k; }

[GoName("ΔHello")] public static @string ΔHello(this ΔEmb _) { return "ΔHello"; }

[GoName("Δf")] public static partial (@string, nint) Δf(Pair p) {
    var (pc, _, line, _) = runtime.Caller(0);
    return (runtime.FuncForPC(pc).Name(), line + p.a - p.b);
}

partial struct S2 {
    internal @string s;
}

[GoName("Δfoo")] public static @string Δfoo(this S2 s) { return "Δfoo of "u8 + s.s; }

partial struct OuterL {
    [GoName("ΔEmbL")] /*embed*/ public DeltaNameProjection.lib_package.ΔEmbL ΔEmbL;
}

partial struct HasRec {
    public lib.Δrec R;
}

public partial struct Δ {
    [GoName("Δ")] public nint ΔΔ;
}

[GoName("Δg")] public delegate @string Δg();

partial struct X {
    /*embed*/ internal Δslice Δslice;
    [GoName("Δslicex")] public nint Δslicex;
}

internal partial struct main_Δx /*dyn*/ {
    internal nint q;
}

internal partial interface main_h /*dyn*/ {
    [GoName("ΔHello")] @string ΔHello();
}

internal partial struct main_v /*dyn*/ {
    public @file F;
}

internal partial interface main_lo /*dyn*/ {
    [GoName("Δfoo")] @string Δfoo();
}

internal partial interface main_type /*dyn*/ {
    [GoName("Δm")] @string Δm();
}

[GoLocalName("Δh")] internal delegate nint main_Δh(nint _Δp0);

internal static void main() {
    var (b, _) = json.Marshal(new P(1D));
    fmt.Println("json:", (@string)b);
    fmt.Println("field name:", reflect.TypeOf(new P()).Field(0).Name);
    fmt.Println("type name:", reflect.TypeOf(new ΔMatrix()).Name(), reflect.TypeOf(new Matrix()).Name());
    fmt.Printf("%%T: %T %T\n"u8, new ΔMatrix(), new Matrix());
    foreach (var (_, n) in range(slice<Namer>([new ΔMatrix(1), new Matrix(2)]))) {
        fmt.Printf("namer: %s %T\n"u8, n.Name(), n);
    }
    any v = new S();
    var (_, ok) = v._<Fooer>(ᐧ);
    fmt.Println("assert Foo:", ok, reflect.TypeOf(new S()).Method(0).Name);
    (_, ok) = v._<lib.Fooer>(ᐧ);
    fmt.Println("assert ΔFoo:", ok, lib.Call(new S()));

    var (name, line) = Δf(new Pair(3, 3));
    fmt.Println("frame:", name, line);

    fmt.Println("field like struct:", reflect.TypeOf(new Δu()).Field(0).Name, new Δu(ΔΔu: 5).ΔΔu);
    fmt.Println("struct Δv field:", reflect.TypeOf(new Δv()).Field(0).Name, new Δv(v: 6).v);
    fmt.Println("renamed field:", reflect.TypeOf(new R()).Field(0).Name);
    var (lb, _) = json.Marshal(new L(1, 2));
    fmt.Println("lower δ:", reflect.TypeOf(new L()).Field(0).Name, reflect.TypeOf(new L()).Field(0).IsExported(), (@string)lb);
    var of = reflect.TypeOf(new Outer()).Field(0);
    fmt.Println("embedded:", of.Name, of.Anonymous, new Outer(ΔEmb: new ΔEmb(4)).e);
    fmt.Printf("lifted: %T %s\n"u8, new main_Δx(), reflect.TypeOf(new main_Δx()).Name());

    nint Δlocal = 8;
    fmt.Println("local and param:", Δlocal, twice(Δlocal));

    ref var r = ref heap<lib.Rec>(new lib.Rec(Δt: 9), out var Ꮡr);
    var (rb, _) = json.Marshal(r);
    var evalᴛ1 = lib.ΔF(5);
    fmt.Println("other package:", r.Δt, evalᴛ1, (nint)(lib.ΔK), lib.ΔV, (@string)rb);

    var sl = new Δslice(ΔΔslice: 10);
    sl.ΔΔslice++;
    fmt.Println("emitted-equal field:", reflect.TypeOf(sl).Name(), reflect.TypeOf(sl).Field(0).Name, sl.ΔΔslice);
    fmt.Println("plain field like struct:", reflect.TypeOf(new Node()).Field(0).Name, new Node(ΔNode: 12).ΔNode);

    ref var w = ref heap<Wrap>(new Wrap(new S()), out var Ꮡw);
    var wf = reflect.TypeOf(w).Field(0);
    fmt.Println("embedded interface:", wf.Name, wf.Anonymous, w.ΔFooer.ΔFoo(), methodNames(reflect.TypeOf(w)));

    fmt.Println("near String:", new T(13), fmt.Sprintf("%v %+v"u8, new T(14), new T(15)));

    ref var m = ref heap<ΔMatrix>(new ΔMatrix(20), out var Ꮡm);
    m.Bump();
    var pm = Ꮡm;
    pm.Bump();
    var evalᴛ2 = pm.Name();
    var evalᴛ3 = reflect.TypeOf(pm.OrTypedNil());
    var evalᴛ4 = methodNames(evalᴛ3);
    var evalᴛ5 = reflect.TypeOf(m);
    var evalᴛ6 = methodNames(evalᴛ5);
    fmt.Println("pointer methods:", m.n, evalᴛ2, evalᴛ4, evalᴛ6);

    var pp = Ꮡ(new P(2D));
    pp.ΔScale(3D);
    var evalᴛ7 = reflect.TypeOf(pp.OrTypedNil());
    var evalᴛ8 = methodNames(evalᴛ7);
    fmt.Println("Δ pointer method:", pp.Δt, evalᴛ8);

    var o = new Outer(ΔEmb: new ΔEmb(30));
    fmt.Println("promoted Δ method:", o.ΔHello(), methodNames(reflect.TypeOf(o)));

    main_h h = o;
    fmt.Println("assert promoted:", h.ΔHello());
    var (_, isStringer) = ((any)new T())._<fmt.Stringer>(ᐧ);
    fmt.Println("T is Stringer:", isStringer);

    ΔFooer fw = new WrapжΔFooer(Ꮡw);
    fmt.Println("embedded interface by pointer:", fw.ΔFoo(), lib.Call(new WrapжFooer(Ꮡw)), lib.Call(w));
    any hv = new H();
    var (_, hNamer) = hv._<Namer>(ᐧ);
    var (_, hFooer) = hv._<ΔFooer>(ᐧ);
    fmt.Println("named not embedded:", hNamer, hFooer, methodNames(reflect.TypeOf(new H())), methodNames(reflect.TypeOf(Ꮡ(new H()))));

    var (fb, _) = json.Marshal(new main_v(new @file(16)));
    fmt.Println("doubled marker:", reflect.TypeOf(new @file()).Name(), reflect.TypeOf(new @file()).Field(0).Name, new @file(ΔΔfile: 16).ΔΔfile,
        reflect.TypeOf(new Δunion()).Name(), reflect.TypeOf(new Δunion()).Field(0).Name, new Δunion(ΔΔunion: 17).ΔΔunion, (@string)fb);
    fmt.Printf("%%+v: %+v %+v\n"u8, new @file(18), new Δunion(19));

    fmt.Println("lower after Δ, direct:", lib.CallLower(new S2("S2")));
    main_lo lo = new S2("iface");
    fmt.Println("lower after Δ, through an interface:", lib.CallLower(new main_loᴠLower(lo)), methodNames(reflect.TypeOf(new S2())));
    var ol = new OuterL(new lib.ΔEmbL(N: 40));
    var (_, olOK) = ((any)ol)._<main_type>(ᐧ);
    fmt.Println("promoted foreign Δm:", ol.ΔEmbL.Δm(), olOK, methodNames(reflect.TypeOf(ol)));
    var rp = Ꮡr.ᏑΔt;
    rp.Value = 41;
    fmt.Println("address of a foreign Δ field:", r.Δt);
    var dr = new lib.Δrec(Δt: 42);
    var hr = new HasRec(dr);
    fmt.Printf("foreign Δ type: %T %+v %v %s\n"u8, dr, hr, hr.R.Δt, reflect.TypeOf(hr).Field(0).Type.Name());
    fmt.Printf("bare Δ: %T %+v %s %T %+v %s\n"u8, new Δ(43), new Δ(ΔΔ: 44), reflect.TypeOf(new Δ()).Field(0).Name,
        new lib.Δ(ΔΔ: 45), new lib.Δ(ΔΔ: 46), reflect.TypeOf(new lib.Δ()).Field(0).Name);
    ref var wr = ref heap<lib.WrapRec>(new lib.WrapRec(), out var Ꮡwr);
    wr.Δt = 47;
    var wp = Ꮡwr.of(lib.WrapRec.ᏑΔt);
    wp.Value++;
    fmt.Println("promoted foreign Δ field:", wr.Δt, wr.Rec.Δt);
    ref var x = ref heap<X>(new X(new Δslice(48), 49), out var Ꮡx);
    x.ΔΔslice++;
    var xp = Ꮡx.of(X.ᏑΔΔslice);
    xp.Value++;
    x.Δslicex++;
    var xf = reflect.TypeOf(x);
    var evalᴛ9 = xf.Field(0);
    var evalᴛ10 = xf.Field(0);
    var evalᴛ11 = xf.Field(1);
    var evalᴛ12 = fmt.Sprintf("%+v"u8, x);
    fmt.Println("embed named Δslice in C#:", x.ΔΔslice, x.Δslice.ΔΔslice, x.Δslicex, evalᴛ9.Name, evalᴛ10.Anonymous,
        evalᴛ11.Name, evalᴛ12);
    lib.ΔFn dbl = [GoFunc("main.func1")] (nint n) => n * 2;
    Δg g = [GoFunc("main.func2")] () => "Δg called";
    lib.Δfn lf = [GoFunc("main.func3")] () => "lib.Δfn called";
    main_Δh hf = [GoFunc("main.func4")] (nint n) => n + 1;
    fmt.Printf("Δ func types: %d %d %s %s %d %T %T %T %T %s %s\n"u8, lib.ApplyΔ(dbl, 50), lib.ApplyΔ(NilSafeDelegateConversion<lib.ΔFn, main_Δh>(hf), 51), g(), lf(), hf(52),
        (dbl).OrTypedNilFunc(), (g).OrTypedNilFunc(), (lf).OrTypedNilFunc(), (hf).OrTypedNilFunc(), reflect.TypeOf((g).OrTypedNilFunc()).Name(), reflect.TypeOf((hf).OrTypedNilFunc()).Name());
}

internal static slice<@string> methodNames(reflect.Type t) {
    slice<@string> names = default;
    foreach (var i in range(t.NumMethod())) {
        names = append(names, t.Method(i).Name);
    }
    return names;
}

internal static nint twice(nint Δn) { return Δn * 2; }

partial struct H {
    public ж<ΔMatrix> Matrix;
    public ΔFooer Fooer;
}

partial struct @file {
    [GoName("file")] internal nint ΔΔfile;
}

partial struct Δunion {
    [GoName("union")] internal nint ΔΔunion;
}

} // end main_package

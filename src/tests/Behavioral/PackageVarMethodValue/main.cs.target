namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static void inc(this ref T p) {
    p.n++;
}

internal static nint add(this ref T p, nint d) {
    p.n += d;
    return p.n;
}

internal static nint peek(this T t) {
    return t.n;
}

partial struct H {
    internal T t;
}

partial struct V {
    /*embed*/ public T T;
}

partial struct E {
    /*embed*/ public ж<T> T;
}

partial struct Tab {
    internal nint n;
    internal array<nint> vals = new(8);
}

internal static void inc(this ref Tab p) {
    p.n++;
}

internal static T mk() {
    return new T(n: 10);
}

internal static void apply(Action f) {
    f();
}

internal static readonly ж<T> Ꮡg = new StaticBox<T>(default(T));
internal static ref T g => ref ((StaticBox<T>)Ꮡg).ValueSlot;
internal static readonly ж<H> Ꮡgh = new StaticBox<H>(default(H));
internal static ref H gh => ref ((StaticBox<H>)Ꮡgh).ValueSlot;
internal static readonly ж<array<T>> Ꮡga = new StaticBox<array<T>>(new array<T>(2));
internal static ref array<T> ga => ref ((StaticBox<array<T>>)Ꮡga).ValueSlot;
internal static readonly ж<T> Ꮡgp = new StaticBox<T>(default(T));
internal static ref T gp => ref ((StaticBox<T>)Ꮡgp).ValueSlot;
internal static readonly ж<T> Ꮡgf = new StaticBox<T>(default(T));
internal static ref T gf => ref ((StaticBox<T>)Ꮡgf).ValueSlot;
internal static readonly ж<T> Ꮡgl = new StaticBox<T>(mk());
internal static ref T gl => ref ((StaticBox<T>)Ꮡgl).ValueSlot;
internal static readonly ж<V> Ꮡgvv = new StaticBox<V>(new V(nil));
internal static ref V gvv => ref ((StaticBox<V>)Ꮡgvv).ValueSlot;
internal static readonly ж<T> Ꮡgr = new StaticBox<T>(default(T));
internal static ref T gr => ref ((StaticBox<T>)Ꮡgr).ValueSlot;
internal static readonly ж<T> Ꮡgw = new StaticBox<T>(default(T));
internal static ref T gw => ref ((StaticBox<T>)Ꮡgw).ValueSlot;
internal static T gv;
internal static ж<T> gpp = Ꮡ(new T(nil));
internal static slice<T> gs = new slice<T>(1);
internal static E ge = new E(Ꮡ(new T(nil)));

[GoLazy] partial class lazyᴛlz { internal static readonly ж<Tab> Ꮡlz = new StaticBox<Tab>(new Tab(vals: new nint[]{1, 2, 3, 4, 5, 6, 7, 8 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1}.array())); }
internal static ref Tab lz => ref ((StaticBox<Tab>)Ꮡlz).ValueSlot;

internal static void Main() {
    var f = Ꮡg.inc;
    f();
    f();
    fmt.Println("pkgvar  ", g.n);

    var fh = Ꮡgh.of(H.Ꮡt).inc;
    fh();
    fmt.Println("field   ", gh.t.n);

    var fa = Ꮡga.at<T>(1).inc;
    fa();
    fmt.Println("array   ", ga[0].n, ga[1].n);

    apply(Ꮡgp.inc);
    fmt.Println("argument", gp.n);

    ((Action)(() => {
        var ff = Ꮡgf.inc;
        ff();
    }))();
    fmt.Println("funclit ", gf.n);

    var fl = Ꮡgl.inc;
    fl();
    fmt.Println("initcall", gl.n);

    var fz = Ꮡlz.inc;
    fz();
    fmt.Println("lazy    ", lz.n, lz.vals[7]);

    var fv = Ꮡgvv.inc;
    fv();
    fmt.Println("embedval", gvv.n);

    var fr = Ꮡgr.add;
    fmt.Println("result  ", fr(5), fr(2), gr.n);

    var fw = Ꮡgw.inc;
    gw = new T(n: 100);
    fw();
    fmt.Println("reassign", gw.n);

    var recvʗ1 = gv;
    var peek = () => recvʗ1.peek();
    gv.n = 7;
    fmt.Println("valrecv ", peek(), gv.n);

    var recvʗ2 = gpp;
    var fp = () => recvʗ2.inc();
    var held = gpp;
    gpp = Ꮡ(new T(n: 50));
    fp();
    fmt.Println("pointer ", (~held).n, (~gpp).n);

    var fs = Ꮡ(gs, 0).inc;
    fs();
    fmt.Println("slice   ", gs[0].n);

    var fe = Ꮡ(ge).inc;
    fe();
    fmt.Println("embedptr", ge.n);
}

} // end main_package

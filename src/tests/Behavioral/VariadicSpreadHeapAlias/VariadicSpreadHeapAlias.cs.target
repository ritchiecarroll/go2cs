namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using ꓸꓸꓸany = variadic<any>;
using ꓸꓸꓸnint = variadic<nint>;

partial class main_package {

internal static void assign(any dest, any src) {
    switch (dest.type()) {
    case ж<any> d: {
        d.ValueSlot = src;
        break;
    }}
}

internal static void row(slice<any> dest) {
    foreach (var i in range(len(dest))) {
        assign(Ꮡ(dest, i), i * 10);
    }
}

internal static void fillAll(params ꓸꓸꓸany destʗp) {
    var dest = destʗp.slice();

    row(dest);
}

internal static void setTail(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    var ys = xs[1..];
    ys[0] = 99;
}

internal static slice<nint> keep(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return xs;
}

internal static slice<nint> grow(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return append(xs, (nint)4);
}

internal static Action later(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    var xsʗ1 = xs;
    return [GoFunc("later.func1")] () => {
        xsʗ1[0] = 7;
    };
}

internal static void boxed(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    any v = xs;
    v._<slice<nint>>()[0] = -1;
}

internal static nint sum(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    nint t = 0;
    foreach (var (_, x) in range(xs)) {
        t += x;
    }
    return t;
}

internal static slice<nint> stashed;

internal static void stash(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    stashed = xs;
    xs[0] = 42;
}

internal static void relay(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp;

    stash(xs.ꓸꓸꓸ);
}

internal static nint relayRead(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    stash(xs.ꓸꓸꓸ);
    return xs[0];
}

internal static nint bump(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    xs[0]++;
    return xs[0] + xs[2];
}

partial struct ints /*[]nint*/;

internal static void viaConstraint<S, E>(S s, Actionꓸꓸꓸ<E> g)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{ g(s.ꓸꓸꓸ); }

internal static void writeFirst(params ꓸꓸꓸany xsʗp) {
    var xs = xsʗp.slice();

    xs[0] = (@string)"w";
}

internal static void main() {
    var vals = new slice<any>(3);
    fillAll(vals.ꓸꓸꓸ);
    fmt.Println("fillAll spread:", vals);

    var (a, b) = ((any)default, (any)default);
    fillAll(a, b);
    fmt.Println("fillAll literal args stay local:", a, b);

    var s = slice<nint>([1, 2, 3]);
    setTail(s.ꓸꓸꓸ);
    fmt.Println("setTail:", s);

    setTail(s[1..].ꓸꓸꓸ);
    fmt.Println("setTail of a subslice:", s);

    var k = keep(s.ꓸꓸꓸ);
    k[0] = 5;
    fmt.Println("keep:", s, len(k), cap(k));

    var c = new slice<nint>(3, 4);
    var g = grow(c.ꓸꓸꓸ);
    fmt.Println("grow within cap:", c[..4], g);

    var d = new slice<nint>(3, 4);
    var g2 = grow(d.slice(0, 2, 2).ꓸꓸꓸ);
    fmt.Println("grow past a capped spread reallocates:", d[..4], g2);

    var t = slice<nint>([1, 2, 3]);
    var f = later(t.ꓸꓸꓸ);
    fmt.Println("sum before:", sum(t.ꓸꓸꓸ));
    f();
    fmt.Println("later:", t);

    var u = slice<nint>([1, 2, 3]);
    boxed(u.ꓸꓸꓸ);
    fmt.Println("boxed:", u);

    slice<nint> n = default;
    fmt.Println("keep of nil is nil:", keep(n.ꓸꓸꓸ) == nil);
    fmt.Println("keep of no args is nil:", keep() == nil);
    var e = slice<nint>([]);
    fmt.Println("keep of empty is not nil:", keep(e.ꓸꓸꓸ) != nil);

    var w = slice<nint>([1, 2, 3]);
    fmt.Println("sum of a spread:", sum(w.ꓸꓸꓸ));
    fmt.Println("keep literal args:", keep(8, 9));

    var r = slice<nint>([1, 2, 3]);
    relay(r.ꓸꓸꓸ);
    fmt.Println("relay spread:", r, stashed[0] == r[0]);

    fmt.Println("relayRead literal args:", relayRead(1, 2));
    var r2 = slice<nint>([1, 2, 3]);
    fmt.Println("relayRead spread:", relayRead(r2.ꓸꓸꓸ), r2);

    fmt.Println("bump of constants:", bump(1, 2, 3), bump(1, 2, 3));

    var ns = new ints(new nint[]{1, 2, 3}.slice());
    stash(ns.ꓸꓸꓸ);
    fmt.Println("named slice spread:", ns);

    var c2 = slice<nint>([1, 2, 3]);
    viaConstraint<slice<nint>, nint>(c2, stash);
    fmt.Println("spread through a constraint:", c2);

    var r3 = new ints(new nint[]{1, 2, 3}.slice());
    reflect.ValueOf(stash).CallSlice(slice<reflect.Value>([reflect.ValueOf(r3)]));
    fmt.Println("reflect CallSlice of a named slice:", r3);

    var r4 = slice<nint>([1, 2, 3]);
    reflect.ValueOf(stash).CallSlice(slice<reflect.Value>([reflect.ValueOf(r4)]));
    fmt.Println("reflect CallSlice:", r4);

    reflect.ValueOf(stash).Call(slice<reflect.Value>([reflect.ValueOf((nint)5), reflect.ValueOf((nint)6)]));
    fmt.Println("reflect Call builds its own slice:", stashed);

    for (nint i = 0; i < 2; i++) {
        any aΔ1 = default;
        any bΔ1 = default;
        writeFirst(aΔ1, bΔ1);
        writeFirst((any)(default), (any)(default));
        fmt.Println("nil constants stay nil:", aΔ1, bΔ1);
    }
}

} // end main_package

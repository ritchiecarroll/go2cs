namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial interface I {
    @string Name();
}

partial struct A {
    internal @string n;
}

public static @string Name(this A a) { return a.n; }

partial struct B {
    internal nint n;
}

public static @string Name(this ref B b) { return fmt.Sprint("b", b.n); }

internal delegate void cancelFunc(error _);

internal static (nint, @string) pair() { return (1, "x"); }

internal static (A, ж<B>) ab() { return (new A("a"), Ꮡ(new B(2))); }

internal static (I, ж<B>) mixed() { return (new A("m"), Ꮡ(new B(3))); }

internal static (ж<B>, ж<B>) ptrs() { return (Ꮡ(new B(4)), default); }

internal static (any, any) anys() { return ((nint)1, default); }

internal static (any, any) tupleᴛ1ʗ = anys();
internal static any ga = tupleᴛ1ʗ.Item1;
internal static any gb = tupleᴛ1ʗ.Item2;

internal static any gi = (nint)5;

internal static (nint, bool) tupleᴛ2ʗ = gi._<nint>(ᐧ);
internal static any gy = tupleᴛ2ʗ.Item1;
internal static any gok = tupleᴛ2ʗ.Item2;

internal static map<@string, nint> pm = new map<@string, nint>{["k"] = 7};

internal static (nint, bool) tupleᴛ3ʗ = pm["k", ꟷ];
internal static any gmv = tupleᴛ3ʗ.Item1;
internal static any gmok = tupleᴛ3ʗ.Item2;

internal static (nint, bool) tupleᴛ4ʗ = pm["k", ꟷ];
internal static nint guv = tupleᴛ4ʗ.Item1;
internal static bool guok = tupleᴛ4ʗ.Item2;

internal static channel<nint> pch = func([GoFunc("init.func1")] channel<nint> () => {
    var c = new channel<nint>(1);
    c.ᐸꟷ(5);
    return c;
})();

internal static (nint, bool) tupleᴛ5ʗ = ᐸꟷ(pch, ꟷ);
internal static nint grv = tupleᴛ5ʗ.Item1;
internal static bool grok = tupleᴛ5ʗ.Item2;

internal static (any, any) tupleᴛ6ʗ = anys();
internal static any qa = tupleᴛ6ʗ.Item1;
internal static any qb = tupleᴛ6ʗ.Item2;

internal static void tpm<M>(M m)
    where M : /* ~map[string]int */ IMap<@string, nint>, ISupportMake<M>, new()
{

    var (vᴛ1, okᴛ1) = m["k"u8, ꟷ];
    any v = vᴛ1;
    any ok = okᴛ1;

    var (w, wok) = m["z"u8, ꟷ];
    fmt.Println(v, ok, w, wok);
}

internal static (cancelFunc, cancelFunc) funcs() {
    var (_, cancel) = context.WithCancelCause(context.Background());
    return (builtin.NilSafeDelegateConversion<cancelFunc, context.CancelCauseFunc>(cancel), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ba2ˢ = (@string)"ba2"u8;

internal static void main() {

    var (aᴛ1, bᴛ1) = pair();
    any a = aᴛ1;
    any b = bᴛ1;
    fmt.Println(a, b);

    var (xᴛ1, yᴛ1) = ab();
    I x = xᴛ1;
    I y = new BжI(yᴛ1);
    fmt.Println(x.Name(), y.Name());

    var (pᴛ1, qᴛ1) = mixed();
    I p = pᴛ1;
    I q = new BжI(qᴛ1);
    fmt.Println(p.Name(), q.Name());

    var (fᴛ1, gᴛ1) = funcs();
    Action<error> f = builtin.NilSafeDelegateConversion<Action<error>, cancelFunc>(fᴛ1);
    Action<error> g = builtin.NilSafeDelegateConversion<Action<error>, cancelFunc>(gᴛ1);
    f(errors.New("f"));
    fmt.Println(g == nil);

    var (_, rᴛ1) = pair();
    any r = rᴛ1;

    var (sᴛ1, _) = pair();
    any s = sᴛ1;
    fmt.Println(r, s);

    var (hᴛ1, kᴛ1) = pair();
    ref var h = ref heap<any>(out var Ꮡh);
    h = hᴛ1;
    any k = kᴛ1;
    var hp = Ꮡh;
    hp.ValueSlot = (@string)"h2";
    fmt.Println(h, k, hp.ValueSlot);

    var (pmᴛ1, pnᴛ1) = ptrs();
    any pm = pmᴛ1.OrTypedNil();
    any pn = pnᴛ1.OrTypedNil();
    fmt.Println(pm, pm != nil, pn, pn != nil);

    fmt.Println(ga, gb, gy, gok);

    var m = new map<@string, nint>{["k"] = 7};

    var (mvᴛ1, mokᴛ1) = m["k", ꟷ];
    any mv = mvᴛ1;
    any mok = mokᴛ1;

    var (mzᴛ1, mzokᴛ1) = m["z", ꟷ];
    any mz = mzᴛ1;
    any mzok = mzokᴛ1;
    fmt.Println(mv, mok, mz, mzok);

    any tx = (nint)3;

    var (tyᴛ1, tokᴛ1) = tx._<nint>(ᐧ);
    any ty = tyᴛ1;
    any tok = tokᴛ1;

    var (tnᴛ1, tnokᴛ1) = tx._<@string>(ᐧ);
    any tn = tnᴛ1;
    any tnok = tnokᴛ1;
    fmt.Println(ty, tok, tn, tnok);

    var ch = new channel<nint>(1);
    ch.ᐸꟷ(9);

    var (cvᴛ1, cokᴛ1) = ᐸꟷ(ch, ꟷ);
    any cv = cvᴛ1;
    any cok = cokᴛ1;
    close(ch);

    var (czᴛ1, czokᴛ1) = ᐸꟷ(ch, ꟷ);
    any cz = czᴛ1;
    any czok = czokᴛ1;
    fmt.Println(cv, cok, cz, czok);

    fmt.Println(gmv, gmok, guv, guok, grv, grok, qa, qb);

    tpm(new map<@string, nint>{["k"] = 7});

    var (uv, uok) = m["k", ꟷ];
    var ch2 = new channel<nint>(1);
    ch2.ᐸꟷ(8);

    var (rv, rok) = ᐸꟷ(ch2, ꟷ);

    var (baᴛ1, bbᴛ1) = anys();
    ref var ba = ref heap<any>(out var Ꮡba);
    ba = baᴛ1;
    any bb = bbᴛ1;
    var bp = Ꮡba;
    bp.ValueSlot = ba2ˢ;
    fmt.Println(uv, uok, rv, rok, ba, bb);

    var ch3 = new channel<nint>(1);
    ch3.ᐸꟷ(6);

    var (qvᴛ1, qokᴛ1) = m["k", ꟷ];
    any qv = qvᴛ1;
    any qok = qokᴛ1;

    var (qrᴛ1, qrokᴛ1) = ᐸꟷ(ch3, ꟷ);
    any qr = qrᴛ1;
    any qrok = qrokᴛ1;

    var (qtᴛ1, qtokᴛ1) = tx._<nint>(ᐧ);
    any qt = qtᴛ1;
    any qtok = qtokᴛ1;

    var (qc, qd) = pair();
    fmt.Println(qv, qok, qr, qrok, qt, qtok, qc, qd);
}

} // end main_package

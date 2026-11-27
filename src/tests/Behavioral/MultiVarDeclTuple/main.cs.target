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

internal static (cancelFunc, cancelFunc) funcs() {
    var (_, cancel) = context.WithCancelCause(context.Background());
    return (builtin.NilSafeDelegateConversion<cancelFunc, context.CancelCauseFunc>(cancel), default);
}

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
}

} // end main_package

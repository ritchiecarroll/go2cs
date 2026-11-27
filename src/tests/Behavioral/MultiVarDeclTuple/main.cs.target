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
}

} // end main_package

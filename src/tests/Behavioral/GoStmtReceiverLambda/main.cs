namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct counter {
    internal nint n;
    internal channel<bool> done;
}

internal static error bump(this ref counter c, nint delta) {
    c.n += delta;
    c.done.ᐸꟷ(true);
    return default;
}

internal static error report(this ref counter c) {
    c.done.ᐸꟷ(true);
    return default;
}

partial struct engine {
    internal ж<counter> tally;
}

internal static partial void start(this ж<engine> Ꮡe, nint delta) {
    goǃ((ᴛ0, ᴛ1) => ᴛ0.bump(ᴛ1), Ꮡe.Value.tally, delta, GoStack.Bounded);
}

internal static partial void ping(this ж<engine> Ꮡe) {
    goǃ(ᴛ0 => ᴛ0.report(), Ꮡe.Value.tally, GoStack.Bounded);
}

partial struct valueSender {
    internal channel<nint> c;
}

internal static void send(this valueSender v, nint n) {
    v.c.ᐸꟷ(n);
}

internal static void ping(this valueSender v) {
    v.c.ᐸꟷ(99);
}

internal static partial void Main() {
    var e = Ꮡ(new engine(tally: Ꮡ(new counter(done: new channel<bool>(1)))));

    e.start(5);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("after start:", (~(~e).tally).n);

    e.start(7);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("after second start:", (~(~e).tally).n);

    e.ping();
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("pinged:", (~(~e).tally).n);

    ref var vs = ref heap<valueSender>(out var Ꮡvs);
    vs = new valueSender(c: new channel<nint>(0));
    var vsʗ1 = vs;
    goǃ(() => vsʗ1.send(7), GoStack.Bounded);
    fmt.Println("value-recv go:", ᐸꟷ(vs.c));
    var vsʗ2 = vs;
    goǃ(() => vsʗ2.ping(), GoStack.Bounded);
    fmt.Println("value-recv nullary go:", ᐸꟷ(vs.c));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
    internal channel<bool> done;
}

[GoRecv] internal static error bump(this ref counter c, nint delta) {
    c.n += delta;
    c.done.ᐸꟷ(true);
    return default!;
}

[GoRecv] internal static error report(this ref counter c) {
    c.done.ᐸꟷ(true);
    return default!;
}

[GoType] partial struct engine {
    internal ж<counter> tally;
}

internal static partial void start(this ж<engine> Ꮡe, nint delta) {
    goǃ(ᴛ1 => Ꮡe.Value.tally.bump(ᴛ1), delta);
}

internal static partial void ping(this ж<engine> Ꮡe) {
    goǃ(() => Ꮡe.Value.tally.report());
}

[GoType] partial struct valueSender {
    internal channel<nint> c;
}

internal static void send(this valueSender v, nint n) {
    v.c.ᐸꟷ(n);
}

internal static void ping(this valueSender v) {
    v.c.ᐸꟷ(99);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterStartˢ = (@string)"after start:"u8;
private static readonly object afterSecondStartˢ = (@string)"after second start:"u8;
private static readonly object pingedˢ = (@string)"pinged:"u8;
private static readonly object valueRecvGoˢ = (@string)"value-recv go:"u8;
private static readonly object valueRecvNullaryGoˢ = (@string)"value-recv nullary go:"u8;

internal static partial void Main() {
    var e = Ꮡ(new engine(tally: Ꮡ(new counter(done: new channel<bool>(1)))));
    e.start(5);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println(afterStartˢ, (~(~e).tally).n);
    e.start(7);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println(afterSecondStartˢ, (~(~e).tally).n);
    e.ping();
    ᐸꟷ((~(~e).tally).done);
    fmt.Println(pingedˢ, (~(~e).tally).n);
    ref var vs = ref heap<valueSender>(out var Ꮡvs);
    vs = new valueSender(c: new channel<nint>(0));
    var vsʗ1 = vs;
    goǃ(ᴛ1 => vsʗ1.send(ᴛ1), 7);
    fmt.Println(valueRecvGoˢ, ᐸꟷ(vs.c));
    var vsʗ2 = vs;
    goǃ(() => vsʗ2.ping());
    fmt.Println(valueRecvNullaryGoˢ, ᐸꟷ(vs.c));
}

} // end main_package

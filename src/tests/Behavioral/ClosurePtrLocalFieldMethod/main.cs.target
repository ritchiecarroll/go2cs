namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static void bump(this ref counter c, nint delta) {
    c.n += delta;
}

[GoRecv] internal static nint get(this ref counter c) {
    return c.n;
}

[GoType] partial struct cachelike {
    internal counter flush;
    internal nint pad;
}

internal static partial void run(Action f) {
    f();
}

internal static partial ж<cachelike> alloc() {
    return Ꮡ(new cachelike(nil));
}

internal static (nint, nint) allocShape() {
    ref var c = ref heap<ж<cachelike>>(out var Ꮡc);
    run(() => {
        Ꮡc.ValueSlot = alloc();
        Ꮡc.ValueSlot.of(cachelike.Ꮡflush).bump(41);
        Ꮡc.ValueSlot.Value.pad = 7;
    });
    c.of(cachelike.Ꮡflush).bump(1);
    return (c.of(cachelike.Ꮡflush).get(), (~c).pad);
}

[GoType] partial struct gauge {
    internal counter v;
}

internal static partial ж<gauge> newGauge() {
    return Ꮡ(new gauge(nil));
}

internal static nint namedAfterType() {
    ref var gauge = ref heap<ж<gauge>>(out var Ꮡgauge);
    gauge = newGauge();
    run(() => {
        Ꮡgauge.ValueSlot = newGauge();
        Ꮡgauge.ValueSlot.of(main_package.gauge.Ꮡv).bump(5);
    });
    gauge.of(main_package.gauge.Ꮡv).bump(2);
    return gauge.of(main_package.gauge.Ꮡv).get();
}

internal static void Main() {
    var (a, b) = allocShape();
    fmt.Println(a, b);
    fmt.Println(namedAfterType());
}

} // end main_package

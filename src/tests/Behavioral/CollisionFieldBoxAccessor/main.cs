namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct Δmark {
    internal nint id;
}

[GoType] partial struct tagger {
}

internal static void mark(this tagger _) {
}

[GoType] partial struct holder {
    internal nint mark;
    internal nint extra;
}

internal static ж<holder> Ꮡh = new StandardBox<holder>(default(holder));
internal static ref holder h => ref Ꮡh.Value;

internal static ж<Δmark> Ꮡgm = new StandardBox<Δmark>(default(Δmark));
internal static ref Δmark gm => ref Ꮡgm.Value;

internal static partial nint localShadowsCollisionType() {
    var m = Ꮡ(new Δmark(id: 10));
    var pid = m.of(main_package.Δmark.Ꮡid);
    pid.Value = 55;
    nint Δmark = 7;
    return pid.Value + Δmark;
}

[GoType] partial struct w {
    internal nint park;
    internal nint other;
}

internal static partial void run(Action f) {
    f();
}

internal static partial nint capturedLocalNamedAfterType() {
    var w = Ꮡ(new w(park: 30, other: 4));
    nint got = 0;
    var wʗ1 = w;
    run(() => {
        var p = wʗ1.of(main_package.w.Ꮡpark);
        p.Value = p.Value + 3;
        got = p.Value;
    });
    return got + (~w).other;
}

internal static partial nint boxRefCapturedValueNamedAfterType() {
    ref var w = ref heap<w>(out var Ꮡw);
    w = new w(park: 100, other: 9);
    nint got = 0;
    run(() => {
        var p = Ꮡw.of(main_package.w.Ꮡpark);
        p.Value = p.Value + 5;
        got = p.Value;
    });
    w.park = w.park + 1;
    return got + w.park + w.other;
}

internal static void Main() {
    var p = Ꮡh.of(holder.Ꮡmark);
    p.Value = 42;
    var q = Ꮡh.of(holder.Ꮡextra);
    q.Value = 7;
    fmt.Println(h.mark, h.extra);
    Δmark m = default!;
    m.id = 3;
    tagger t = default!;
    t.mark();
    fmt.Println(m.id);
    var pid = Ꮡgm.of(main_package.Δmark.Ꮡid);
    pid.Value = 99;
    fmt.Println(gm.id);
    var h2 = new holder(mark: 5, extra: 6);
    fmt.Println(h2.mark, h2.extra);
    fmt.Println(localShadowsCollisionType());
    fmt.Println(capturedLocalNamedAfterType());
    fmt.Println(boxRefCapturedValueNamedAfterType());
}

} // end main_package

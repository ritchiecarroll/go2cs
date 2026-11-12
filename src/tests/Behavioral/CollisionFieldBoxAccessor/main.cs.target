namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct mark {
    internal nint id;
}

partial struct tagger {
}

} partial class mainᴛtagger {

internal static void mark(this tagger _) { }

} partial class main_package {

partial struct holder {
    internal nint mark;
    internal nint extra;
}

internal static readonly ж<holder> Ꮡh = new StaticBox<holder>(default(holder));
internal static ref holder h => ref ((StaticBox<holder>)Ꮡh).ValueSlot;

internal static readonly ж<mark> Ꮡgm = new StaticBox<mark>(default(mark));
internal static ref mark gm => ref ((StaticBox<mark>)Ꮡgm).ValueSlot;

internal static partial nint localShadowsCollisionType() {
    var m = Ꮡ(new mark(id: 10));
    var pid = m.Ꮡid;
    pid.Value = 55;
    nint mark = 7;
    return pid.Value + mark;
}

partial struct w {
    internal nint park;
    internal nint other;
}

internal static partial void run(Action f) { f(); }

internal static partial nint capturedLocalNamedAfterType() {
    var w = Ꮡ(new w(park: 30, other: 4));
    nint got = 0;
    var wʗ1 = w;
    run(() => {
        var p = wʗ1.Ꮡpark;
        p.Value = p.Value + 3;
        got = p.Value;
    });
    return got + w.other;
}

internal static partial nint boxRefCapturedValueNamedAfterType() {
    ref var w = ref heap<w>(new w(park: 100, other: 9), out var Ꮡw);
    nint got = 0;
    run(() => {
        var p = Ꮡw.Ꮡpark;
        p.Value = p.Value + 5;
        got = p.Value;
    });
    w.park = w.park + 1;
    return got + w.park + w.other;
}

internal static void main() {
    var p = Ꮡh.Ꮡmark;
    p.Value = 42;

    var q = Ꮡh.Ꮡextra;
    q.Value = 7;

    fmt.Println(h.mark, h.extra);

    mark m = default;
    m.id = 3;
    tagger t = default;
    t.mark();
    fmt.Println(m.id);

    var pid = Ꮡgm.Ꮡid;
    pid.Value = 99;
    fmt.Println(gm.id);

    var h2 = new holder(mark: 5, extra: 6);
    fmt.Println(h2.mark, h2.extra);

    fmt.Println(localShadowsCollisionType());

    fmt.Println(capturedLocalNamedAfterType());

    fmt.Println(boxRefCapturedValueNamedAfterType());
}

} // end main_package

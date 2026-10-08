namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct inner {
    internal channel<nint> ch;
}

internal static void send(this ref inner @in) {
    @in.ch.ᐸꟷ(3);
}

partial struct tracker {
    internal channel<nint> done;
    internal nint n;
    internal inner @in;
}

internal static void loop(this ref tracker t) {
    t.done.ᐸꟷ(t.n);
}

internal static void loopArg(this ref tracker t, nint k) {
    t.done.ᐸꟷ(k);
}

internal static void bump(this ref tracker t) {
    t.n++;
}

internal static partial void start(this ж<tracker> Ꮡt) {
    goǃ(Ꮡt.loop);
}

internal static partial void startArg(this ж<tracker> Ꮡt) {
    goǃ(Ꮡt.loopArg, (nint)(9));
}

internal static partial void startInner(this ж<tracker> Ꮡt) {
    goǃ(Ꮡt.of(tracker.Ꮡin).send);
}

internal static void stop(this ж<tracker> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡt.bump, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void other(this ref tracker t, ж<tracker> Ꮡo) {
    goǃ(Ꮡo.loop);
}

internal static void Main() {
    var t = Ꮡ(new tracker(done: new channel<nint>(0), n: 7, @in: new inner(ch: new channel<nint>(0))));
    t.start();
    fmt.Println(ᐸꟷ((~t).done));
    t.startArg();
    fmt.Println(ᐸꟷ((~t).done));
    t.startInner();
    fmt.Println(ᐸꟷ((~t).@in.ch));
    t.stop();
    fmt.Println((~t).n);
    var o = Ꮡ(new tracker(done: new channel<nint>(0), n: 11));
    t.other(o);
    fmt.Println(ᐸꟷ((~o).done));
}

} // end main_package

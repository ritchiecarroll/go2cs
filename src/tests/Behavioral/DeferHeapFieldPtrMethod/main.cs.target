namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tracker {
    internal slice<@string> lines;
}

internal static void flush(this ref tracker t) {
    fmt.Printf("flush: %d lines\n"u8, len(t.lines));

    foreach (var (_, l) in range(t.lines)) {
        fmt.Println("line:", l);
    }
}

partial struct parser {
    internal @string name;
    internal tracker trk;
}

internal static void seed(ref parser p) {
    p.trk.lines = append(p.trk.lines, (@string)"seed");
}

internal static void run() {
    GoFrame ᒐ = default;
    try {
        ref var p = ref heap<parser>(new parser(name: "p1"), out var Ꮡp);
        seed(ref p);
        defer(Ꮡp.Ꮡtrk.flush, ref ᒐ);
        p.trk.lines = append(p.trk.lines, (@string)"after-defer");
        p.trk.lines = append(p.trk.lines, (@string)"final");
        fmt.Println("run done:", p.name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    run();
}

} // end main_package

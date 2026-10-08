namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tracker {
    internal slice<@string> lines;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lineˢ = (@string)"line:"u8;

internal static void flush(this ref tracker t) {
    fmt.Printf("flush: %d lines\n"u8, len(t.lines));
    foreach (var (_, l) in t.lines) {
        fmt.Println(lineˢ, l);
    }
}

partial struct parser {
    internal @string name;
    internal tracker trk;
}

internal static void seed(ref parser p) {
    p.trk.lines = append(p.trk.lines, "seed"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object runDoneˢ = (@string)"run done:"u8;

internal static void run() {
    GoFrame ᒐ = default;
    try {
        ref var p = ref heap<parser>(out var Ꮡp);
        p = new parser(name: "p1"u8);
        seed(ref p);
        defer(Ꮡp.of(parser.Ꮡtrk).flush, ref ᒐ);
        p.trk.lines = append(p.trk.lines, "after-defer"u8);
        p.trk.lines = append(p.trk.lines, "final"u8);
        fmt.Println(runDoneˢ, p.name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    run();
}

} // end main_package

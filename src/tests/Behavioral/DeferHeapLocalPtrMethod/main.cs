namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct state {
    internal nint n;
    internal @string label;
}

internal static void free(this ref state s) {
    fmt.Printf("free: %s n=%d\n"u8, s.label, s.n);
}

internal static void add(this ref state s, nint d) {
    s.n += d;
}

internal static void run() {
    GoFrame ᒐ = default;
    try {
        ref var st = ref heap<state>(out var Ꮡst);
        st = new state(n: 0, label: "run"u8);
        defer(Ꮡst.free, ref ᒐ);
        st.add(5);
        st.add(10);
        fmt.Printf("during run: n=%d\n"u8, st.n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void twice() {
    GoFrame ᒐ = default;
    try {
        ref var st = ref heap<state>(out var Ꮡst);
        st = new state(n: 100, label: "twice"u8);
        defer(Ꮡst.free, ref ᒐ);
        st.add(1);
        fmt.Printf("during twice: n=%d\n"u8, st.n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    run();
    twice();
}

} // end main_package

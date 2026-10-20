namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct path {
    internal @string name;
}

internal static error Close(this path p) {
    fmt.Println("close", p.name);
    return default!;
}

internal static void Log(this path p, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    fmt.Println("log", p.name, args);
}

partial struct state {
    internal path cur;
}

internal static void run(this ж<state> Ꮡs, nint k) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Close(), Ꮡs.Value.cur, ref ᒐ);
        defer((ᴛ0, ᴛ1, ᴛ2) => ᴛ0.Log(ᴛ1, ᴛ2), Ꮡs.Value.cur, (@string)"k", k, ref ᒐ);
        s.cur = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void viaPointer(ж<path> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Close(), Ꮡp.Value, ref ᒐ);
        p = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void local() {
    GoFrame ᒐ = default;
    try {
        ref var k = ref heap<path>(out var Ꮡk);
        k = new path("local");
        var kʗ1 = k;
        defer(() => kʗ1.Close(), ref ᒐ);
        k = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var s = Ꮡ(new state(cur: new path("orig")));
    s.run(1);
    viaPointer(Ꮡ(new path("pointee")));
    local();
}

} // end main_package

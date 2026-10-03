namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

[GoType] partial struct path {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closeˢ = (@string)"close"u8;

internal static error Close(this path p) {
    fmt.Println(closeˢ, p.name);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object logˢ = (@string)"log"u8;

internal static void Log(this path p, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    fmt.Println(logˢ, p.name, args);
}

[GoType] partial struct state {
    internal path cur;
}

internal static void run(this ж<state> Ꮡs, nint k) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Close(), Ꮡs.Value.cur, ref ᒐ);
        defer((ᴛ0, ᴛ1, ᴛ2) => ᴛ0.Log(ᴛ1, ᴛ2), Ꮡs.Value.cur, (@string)"k", k, ref ᒐ);
        s.cur = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void viaPointer(ж<path> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Close(), Ꮡp.Value, ref ᒐ);
        p = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void local() {
    GoFrame ᒐ = default;
    try {
        ref var k = ref heap<path>(out var Ꮡk);
        k = new path("local"u8);
        var kʗ1 = k;
        defer(() => kʗ1.Close(), ref ᒐ);
        k = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var s = Ꮡ(new state(cur: new path("orig"u8)));
    s.run(1);
    viaPointer(Ꮡ(new path("pointee"u8)));
    local();
}

} // end main_package

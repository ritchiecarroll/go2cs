namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct path {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object popˢ = (@string)"pop"u8;

internal static void Pop(this path p, nint k) {
    fmt.Println(popˢ, p.name, k);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void Done(this path p) {
    fmt.Println(doneˢ, p.name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pairˢ = (@string)"pair"u8;

internal static void Pair(this path p, nint a, @string b) {
    fmt.Println(pairˢ, p.name, a, b);
}

partial struct state {
    internal path cur;
}

internal static void fieldOfPointer(this ж<state> Ꮡs, nint k) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), Ꮡs.Value.cur, k, ref ᒐ);
        defer((ᴛ0, ᴛ1, ᴛ2) => ᴛ0.Pair(ᴛ1, ᴛ2), Ꮡs.Value.cur, k, (@string)"b", ref ᒐ);
        s.cur = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void fieldOfValue(this state s, nint k) {
    GoFrame ᒐ = default;
    try {
        var sʗ1 = s;
        defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), sʗ1.cur, k, ref ᒐ);
        s.cur = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void nullary(this ж<state> Ꮡs) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Done(), Ꮡs.Value.cur, ref ᒐ);
        s.cur = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void local(nint k) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref heap<path>(out var Ꮡp);
        p = new path("local"u8);
        var pʗ1 = p;
        defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), pʗ1, k, ref ᒐ);
        p = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void viaPointer(ж<path> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Done(), Ꮡp.Value, ref ᒐ);
        p = new path("replaced"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var s = Ꮡ(new state(cur: new path("orig"u8)));
    s.fieldOfPointer(1);
    s.Value.cur = new path("orig"u8);
    (~s).fieldOfValue(2);
    s.nullary();
    local(3);
    viaPointer(Ꮡ(new path("pointee"u8)));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct conn {
    internal nint id;
}

partial struct tracker {
    internal map<ж<conn>, @string> m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unknownˢ = "unknown"u8;

internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var (s, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
            return s;
        }
    }
    return unknownˢ;
}

internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.m[Ꮡc] = s;
}

internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.m[Ꮡc];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closedˢ = (@string)"closed"u8;

internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var t = ref Ꮡt.DerefOrNull();

        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), closedˢ, c.id, ref ᒐ);
        {
            var (_, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
                delete(t.m, Ꮡc);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string busyˢ = "busy"u8;
private static readonly @string newˢ = "new"u8;

internal static void Main() {
    var a = Ꮡ(new conn(id: 1));
    var b = Ꮡ(new conn(id: 2));
    var t = Ꮡ(new tracker(m: new map<ж<conn>, @string>{[a] = "idle"u8}));
    fmt.Println(a.status(t), b.status(t));
    a.rename(t, busyˢ);
    fmt.Println(a.label(t), len((~t).m));
    b.rename(t, newˢ);
    fmt.Println(b.label(t), a.label(t), len((~t).m));
    a.close(t);
    fmt.Println(len((~t).m), a.status(t), b.status(t));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct conn {
    internal nint id;
}

partial struct tracker {
    internal map<ж<conn>, @string> m;
}

internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var (s, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
            return s;
        }
    }
    return "unknown";
}

internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.m[Ꮡc] = s;
}

internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.m[Ꮡc];
}

internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var t = ref Ꮡt.DerefOrNull();

        defer(ᴛ1 => fmt.Println("closed", ᴛ1), c.id, ref ᒐ);
        {
            var (_, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
                delete(t.m, Ꮡc);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var a = Ꮡ(new conn(id: 1));
    var b = Ꮡ(new conn(id: 2));
    var t = Ꮡ(new tracker(m: new map<ж<conn>, @string>{[a] = "idle"}));

    fmt.Println(a.status(t), b.status(t));

    a.rename(t, "busy");
    fmt.Println(a.label(t), len(t.m));

    b.rename(t, "new");
    fmt.Println(b.label(t), a.label(t), len(t.m));

    a.close(t);
    fmt.Println(len(t.m), a.status(t), b.status(t));
}

} // end main_package

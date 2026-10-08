namespace go;

using fmt = fmt_package;
using Δsync = sync_package;

partial class main_package {

partial struct state {
    public partial ref sync_package.Mutex Mutex { get; }
    internal bool broken;
}

partial struct conn {
    internal @string name;
    internal partial ref ж<state> state { get; }
}

internal static (nint, error) Write(this ж<conn> Ꮡc, slice<byte> b) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.state.of(state.ᏑMutex).Lock();
        ᒐd1 = true;
        if (c.broken) {
            return (0, fmt.Errorf("%s is broken"u8, c.name));
        }
        return (len(b), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().state.of(state.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static void Main() {
    var s = Ꮡ(new state(nil));
    var c = Ꮡ(new conn(name: "c1"u8, state: s));
    var (n, err) = c.Write(slice<byte>("hello"u8));
    fmt.Println(n, err == default!);
    s.Value.broken = true;
    (n, err) = c.Write(slice<byte>("hello"u8));
    fmt.Println(n, err != default!);
    c.Value.state.of(state.ᏑMutex).Lock();
    c.Value.broken = false;
    c.Value.state.of(state.ᏑMutex).Unlock();
    fmt.Println((~s).broken);
    (n, err) = c.Write(slice<byte>("xy"u8));
    fmt.Println(n, err == default!);
}

} // end main_package

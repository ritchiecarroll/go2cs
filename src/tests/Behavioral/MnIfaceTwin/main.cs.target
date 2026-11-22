namespace go;

using fmt = fmt_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial interface Parker {
    nint Park(time.Duration d);
}

partial struct gate {
    internal Δsync.Mutex mu;
    internal nint n;
}

internal static nint Park(this ж<gate> Ꮡg, time.Duration d) {
    ref var g = ref Ꮡg.DerefOrNull();

    g.mu.Lock();
    g.n++;
    g.mu.Unlock();
    time.Sleep(d);
    return 1;
}

partial struct napper /*num:nint*/;

internal static nint Park(this napper n, time.Duration d) {
    time.Sleep(d);
    return (nint)n;
}

partial struct @fixed {
    internal nint v;
}

internal static nint Park(this @fixed f, time.Duration d) {
    return f.v;
}

partial struct relay {
    internal nint v;
}

internal static nint Park(this ref relay r, time.Duration d) {
    time.Sleep(d);
    return r.v;
}

partial struct late {
    internal nint v;
}

internal static nint Park(this late l, time.Duration d) {
    return l.v;
}

public static Parker Hand;

internal static partial nint drive(Parker p, nint n, time.Duration hold) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var results = new channel<nint>(n);
    for (nint i = 0; i < n; i++) {
        Ꮡwg.Add(1);
        var resultsʗ1 = results;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                resultsʗ1.ᐸꟷ(p.Park(hold));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    nint sum = 0;
    for (nint i = 0; i < n; i++) {
        sum += ᐸꟷ(results);
    }
    return sum;
}

internal static nint envInt(@string name, nint fallback) {
    var (v, err) = strconv.Atoi(os.Getenv(name));
    if (err != nil) {
        return fallback;
    }
    return v;
}

internal static void main() {
    nint n = envInt("MN_IFACE_N", 100);
    var hold = ((time.Duration)(int64)envInt("MN_IFACE_HOLD_MS", 20)) * time.Millisecond;
    @string kind = os.Getenv("MN_IFACE_KIND");

    if (kind == ""u8 || kind == "gate"u8) {
        fmt.Println("gate", drive(new gateжParker(Ꮡ(new gate())), n, hold));
    }
    if (kind == ""u8 || kind == "napper"u8) {
        fmt.Println("napper", drive((napper)2, n, hold));
    }
    if (kind == ""u8 || kind == "fixed"u8) {
        fmt.Println("fixed", drive(new @fixed(3), n, hold));
    }
    if (kind == ""u8 || kind == "relay"u8) {
        fmt.Println("relay", drive(new relayжParker(Ꮡ(new relay(4))), n, hold));
    }
    if (Hand != nil && (kind == ""u8 || kind == "hand"u8)) {
        fmt.Println("hand", drive(Hand, n, hold));
    }

    any x = new late(6);
    var (p, okΔ1) = x._<Parker>(ᐧ); if (okΔ1) {
        fmt.Println("late", okΔ1, p.Park(0));
    } else {
        fmt.Println("late", okΔ1);
    }
    any y = (nint)42;
    var (_, ok) = y._<Parker>(ᐧ);
    fmt.Println("int", ok);
    fmt.Println("methods", reflect.TypeOf((ж<Parker>)nil).Elem().NumMethod());
}

} // end main_package

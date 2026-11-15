namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using lib = CollisionRenamedGlobalReceiver.lib_package;
using CollisionRenamedGlobalReceiver;
using sync;

partial class main_package {

internal static readonly ж<atomic.Int32> ᏑΔtid = new StaticBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 Δtid => ref ((StaticBox<atomic.Int32>)ᏑΔtid).ValueSlot;

internal static readonly ж<counter> ᏑΔcount = new StaticBox<counter>(default(counter));
internal static ref counter Δcount => ref ((StaticBox<counter>)ᏑΔcount).ValueSlot;

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c) {
    c.n++;
    return c.n;
}

internal static ж<counter> self(this ж<counter> Ꮡc) { return Ꮡc; }

internal static int32 shadow() {
    ref var Δtid = ref heap(new atomic.Int32(), out var Ꮡtid);
    Ꮡtid.Add(5);
    var p = Ꮡtid;
    p.Add(1);
    return Ꮡtid.Load();
}

partial struct TLS {
    internal int32 id;
}

internal static int32 tid(this ref TLS tls) { return tls.id; }

internal static nint count(this ref TLS tls) { return (nint)tls.id * 10; }

internal static void main() {
    var first = ᏑΔtid.Add(1);
    ref var second = ref heap<int32>(ᏑΔtid.Add(1), out var Ꮡsecond);
    var tls = Ꮡ(new TLS(id: second));
    fmt.Println(first, second, tls.tid(), ᏑΔtid.Load());

    var p = ᏑΔtid;
    p.Add(40);
    fmt.Println(ᏑΔtid.Load(), p.Load());

    Δcount.bump();
    fmt.Println(Δcount.bump(), Δcount.n, tls.count());

    ᏑΔcount.self().bump();
    fmt.Println(ᏑΔcount.self().bump(), Δcount.n, ᏑΔcount.self() == ᏑΔcount);

    var before = ᏑΔtid.Load();
    fmt.Println(before, shadow(), ᏑΔtid.Load());

    lib.ᏑΔTid.Add(1);
    var q = lib.ᏑΔTid;
    q.Add(10);
    fmt.Println(lib.ᏑΔTid.Load(), q.Load(), lib.Next(), (Ꮡ(new lib.T(ID: 9))).Tid());
    fmt.Println(viaDot(), lib.ᏑΔTid.Load());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
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
}

} // end main_package

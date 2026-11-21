namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using lib = CollisionRenamedGlobalReceiver.lib_package;
using CollisionRenamedGlobalReceiver;
using sync;

partial class main_package {

internal static readonly ж<atomic.Int32> Ꮡtid = new StaticBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 tid => ref ((StaticBox<atomic.Int32>)Ꮡtid).ValueSlot;

internal static readonly ж<counter> Ꮡcount = new StaticBox<counter>(default(counter));
internal static ref counter count => ref ((StaticBox<counter>)Ꮡcount).ValueSlot;

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c) {
    c.n++;
    return c.n;
}

internal static ж<counter> self(this ж<counter> Ꮡc) { return Ꮡc; }

internal static int32 shadow() {
    ref var tid = ref heap(new atomic.Int32(), out var Ꮡtid);
    Ꮡtid.Add(5);
    var p = Ꮡtid;
    p.Add(1);
    return Ꮡtid.Load();
}

partial struct TLS {
    internal int32 id;
}

internal static int32 tid⁔(/*this*/ ref TLS tls) { return tls.id; }

internal static nint count⁔(/*this*/ ref TLS tls) { return (nint)tls.id * 10; }

internal static void main() {
    var first = Ꮡtid.Add(1);
    ref var second = ref heap<int32>(Ꮡtid.Add(1), out var Ꮡsecond);
    var tls = Ꮡ(new TLS(id: second));
    fmt.Println(first, second, tls.tid(), Ꮡtid.Load());

    var p = Ꮡtid;
    p.Add(40);
    fmt.Println(Ꮡtid.Load(), p.Load());

    count.bump();
    fmt.Println(count.bump(), count.n, tls.count());

    Ꮡcount.self().bump();
    fmt.Println(Ꮡcount.self().bump(), count.n, Ꮡcount.self() == Ꮡcount);

    var before = Ꮡtid.Load();
    fmt.Println(before, shadow(), Ꮡtid.Load());

    lib.ᏑTid.Add(1);
    var q = lib.ᏑTid;
    q.Add(10);
    fmt.Println(lib.ᏑTid.Load(), q.Load(), lib.Next(), (Ꮡ(new lib.T(ID: 9))).Tid());
    fmt.Println(viaDot(), lib.ᏑTid.Load());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct entry<V> {
    internal ж<V> key;
    internal atomic.Pointer<V> v;
    internal ж<entry<V>> next;
}

partial struct Cache<V> {
    internal atomic.Pointer<entry<V>> head;
}

public static void Put<V>(this ж<Cache<V>> Ꮡc, ж<V> key, ж<V> val) {
    var e = Ꮡ(new entry<V>(key: key));
    e.Ꮡv.Store(val);
    e.next = Ꮡc.Ꮡhead.Load();
    Ꮡc.Ꮡhead.Store(e);
}

public static ж<V> Get<V>(this ж<Cache<V>> Ꮡc, ж<V> Ꮡkey) {
    ref var key = ref Ꮡkey.DerefOrNull();

    for (var e = Ꮡc.Ꮡhead.Load(); e != nil; e = e.next) {
        if (e.key == Ꮡkey) {
            return e.Ꮡv.Load();
        }
    }
    return default;
}

internal static void Main() {
    ref var c = ref heap(new Cache<nint>(), out var Ꮡc);
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    ref var av = ref heap<nint>(out var Ꮡav);
    av = 10;
    ref var bv = ref heap<nint>(out var Ꮡbv);
    bv = 20;
    Ꮡc.Put(Ꮡa, Ꮡav);
    Ꮡc.Put(Ꮡb, Ꮡbv);

    fmt.Println("get a:", Ꮡc.Get(Ꮡa).Value);
    fmt.Println("get b:", Ꮡc.Get(Ꮡb).Value);

    ref var newAv = ref heap<nint>(99, out var ᏑnewAv);
    Ꮡc.Put(Ꮡa, ᏑnewAv);
    fmt.Println("get a again:", Ꮡc.Get(Ꮡa).Value);
    fmt.Println("missing:", Ꮡc.Get(@new<nint>()) == nil);
}

} // end main_package

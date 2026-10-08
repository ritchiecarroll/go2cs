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

public static void Put<V>(this ж<Cache<V>> Ꮡc, ж<V> Ꮡkey, ж<V> Ꮡval) {
    var e = Ꮡ(new entry<V>(key: Ꮡkey));
    e.of(entry<V>.Ꮡv).Store(Ꮡval);
    e.Value.next = Ꮡc.of(Cache<V>.Ꮡhead).Load();
    Ꮡc.of(Cache<V>.Ꮡhead).Store(e);
}

public static ж<V> Get<V>(this ж<Cache<V>> Ꮡc, ж<V> Ꮡkey) {
    ref var key = ref Ꮡkey.DerefOrNull();

    for (var e = Ꮡc.of(Cache<V>.Ꮡhead).Load(); e != nil; e = e.Value.next) {
        if ((~e).key == Ꮡkey) {
            return e.of(entry<V>.Ꮡv).Load();
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object getAˢ = (@string)"get a:"u8;
private static readonly object getBˢ = (@string)"get b:"u8;
private static readonly object getAAgainˢ = (@string)"get a again:"u8;
private static readonly object missingˢ = (@string)"missing:"u8;

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
    fmt.Println(getAˢ, Ꮡc.Get(Ꮡa).Value);
    fmt.Println(getBˢ, Ꮡc.Get(Ꮡb).Value);
    ref var newAv = ref heap<nint>(out var ᏑnewAv);
    newAv = 99;
    Ꮡc.Put(Ꮡa, ᏑnewAv);
    fmt.Println(getAAgainˢ, Ꮡc.Get(Ꮡa).Value);
    fmt.Println(missingˢ, Ꮡc.Get(@new<nint>()) == nil);
}

} // end main_package

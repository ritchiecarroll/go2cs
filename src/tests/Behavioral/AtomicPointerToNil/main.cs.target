namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueˢ = (@string)"value"u8;

internal static void Main() {
    ref var p = ref heap(new atomic.Pointer<any>(), out var Ꮡp);

    fmt.Println("zero:", Ꮡp.Load() == nil);

    ref var i = ref heap<any>(out var Ꮡi);
    var pi = Ꮡi;
    Ꮡp.Store(pi);
    fmt.Println("store &nil:", Ꮡp.Load() == nil, Ꮡp.Load() == pi, Ꮡp.Load().ValueSlot == nil);

    pi.ValueSlot = valueˢ;
    fmt.Println("through pointer:", Ꮡp.Load().ValueSlot);

    var old = Ꮡp.Swap(nil);
    fmt.Println("swap out:", old == pi, Ꮡp.Load() == nil);

    ref var j = ref heap<any>(out var Ꮡj);
    var pj = Ꮡj;
    old = Ꮡp.Swap(pj);
    fmt.Println("swap in:", old == nil, Ꮡp.Load() == pj);

    var a = @new<any>();
    var b = @new<any>();
    fmt.Println("sentinels:", a == nil, b == nil, a == b, a.ValueSlot == nil);

    Ꮡp.Store(a);
    fmt.Println("cas nil:", Ꮡp.CompareAndSwap(nil, b));
    fmt.Println("cas wrong sentinel:", Ꮡp.CompareAndSwap(b, nil));
    fmt.Println("cas right sentinel:", Ꮡp.CompareAndSwap(a, b), Ꮡp.Load() == b);

    Ꮡp.Store(nil);
    fmt.Println("cas from nil:", Ꮡp.CompareAndSwap(nil, a), Ꮡp.Load() == a);

    ref var q = ref heap(new atomic.Pointer<ж<nint>>(), out var Ꮡq);
    var pp = @new<ж<nint>>();
    Ꮡq.Store(pp);
    fmt.Println("ptr-to-nil-ptr:", Ꮡq.Load() == nil, Ꮡq.Load() == pp, Ꮡq.Load().ValueSlot == nil);
}

} // end main_package

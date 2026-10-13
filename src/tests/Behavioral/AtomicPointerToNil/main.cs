namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zeroˢ = (@string)"zero:"u8;
private static readonly object storeNilˢ = (@string)"store &nil:"u8;
private static readonly object valueˢ = (@string)"value"u8;
private static readonly object throughPointerˢ = (@string)"through pointer:"u8;
private static readonly object swapOutˢ = (@string)"swap out:"u8;
private static readonly object swapInˢ = (@string)"swap in:"u8;
private static readonly object sentinelsˢ = (@string)"sentinels:"u8;
private static readonly object casNilˢ = (@string)"cas nil:"u8;
private static readonly object casWrongSentinelˢ = (@string)"cas wrong sentinel:"u8;
private static readonly object casRightSentinelˢ = (@string)"cas right sentinel:"u8;
private static readonly object casFromNilˢ = (@string)"cas from nil:"u8;
private static readonly object ptrToNilPtrˢ = (@string)"ptr-to-nil-ptr:"u8;

internal static void Main() {
    ref var p = ref heap(new atomic.Pointer<any>(), out var Ꮡp);

    fmt.Println(zeroˢ, Ꮡp.Load() == nil);

    ref var i = ref heap<any>(out var Ꮡi);
    var pi = Ꮡi;
    Ꮡp.Store(pi);
    fmt.Println(storeNilˢ, Ꮡp.Load() == nil, Ꮡp.Load() == pi, Ꮡp.Load().ValueSlot == default!);

    pi.ValueSlot = valueˢ;
    fmt.Println(throughPointerˢ, Ꮡp.Load().ValueSlot);

    var old = Ꮡp.Swap(nil);
    fmt.Println(swapOutˢ, old == pi, Ꮡp.Load() == nil);

    ref var j = ref heap<any>(out var Ꮡj);
    var pj = Ꮡj;
    old = Ꮡp.Swap(pj);
    fmt.Println(swapInˢ, old == nil, Ꮡp.Load() == pj);

    var a = @new<any>();
    var b = @new<any>();
    fmt.Println(sentinelsˢ, a == nil, b == nil, a == b, a.ValueSlot == default!);

    Ꮡp.Store(a);
    fmt.Println(casNilˢ, Ꮡp.CompareAndSwap(nil, b));
    fmt.Println(casWrongSentinelˢ, Ꮡp.CompareAndSwap(b, nil));
    fmt.Println(casRightSentinelˢ, Ꮡp.CompareAndSwap(a, b), Ꮡp.Load() == b);

    Ꮡp.Store(nil);
    fmt.Println(casFromNilˢ, Ꮡp.CompareAndSwap(nil, a), Ꮡp.Load() == a);

    ref var q = ref heap(new atomic.Pointer<ж<nint>>(), out var Ꮡq);
    var pp = @new<ж<nint>>();
    Ꮡq.Store(pp);
    fmt.Println(ptrToNilPtrˢ, Ꮡq.Load() == nil, Ꮡq.Load() == pp, Ꮡq.Load().ValueSlot == nil);
}

} // end main_package

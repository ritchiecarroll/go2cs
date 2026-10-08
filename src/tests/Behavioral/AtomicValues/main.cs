namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct holder {
    internal atomic.Int64 count;
}

internal static ж<holder> ᏑgHolder = new StandardBox<holder>(default(holder));
internal static ref holder gHolder => ref ᏑgHolder.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object addˢ = (@string)"add:"u8;
private static readonly object loadˢ = (@string)"load:"u8;
private static readonly object swapˢ = (@string)"swap:"u8;
private static readonly object casOkˢ = (@string)"cas ok:"u8;
private static readonly object casNoˢ = (@string)"cas no:"u8;
private static readonly object finalˢ = (@string)"final:"u8;
private static readonly object ptrNilˢ = (@string)"ptr nil:"u8;
private static readonly object ptrLoadˢ = (@string)"ptr load:"u8;
private static readonly object ptrCasNoˢ = (@string)"ptr cas no:"u8;
private static readonly object ptrCasOkˢ = (@string)"ptr cas ok:"u8;
private static readonly object ptrFinalˢ = (@string)"ptr final:"u8;
private static readonly object ptrSwapˢ = (@string)"ptr swap:"u8;
private static readonly object globalFieldˢ = (@string)"global field:"u8;

internal static void Main() {
    ref var n = ref heap(new atomic.Int32(), out var Ꮡn);
    Ꮡn.Store(10);
    fmt.Println(addˢ, Ꮡn.Add(5));
    fmt.Println(loadˢ, Ꮡn.Load());
    fmt.Println(swapˢ, Ꮡn.Swap(100));
    fmt.Println(casOkˢ, Ꮡn.CompareAndSwap(100, 7));
    fmt.Println(casNoˢ, Ꮡn.CompareAndSwap(100, 8));
    fmt.Println(finalˢ, Ꮡn.Load());
    ref var p = ref heap(new atomic.Pointer<nint>(), out var Ꮡp);
    fmt.Println(ptrNilˢ, Ꮡp.Load() == nil);
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    Ꮡp.Store(Ꮡa);
    fmt.Println(ptrLoadˢ, Ꮡp.Load().Value);
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    fmt.Println(ptrCasNoˢ, Ꮡp.CompareAndSwap(Ꮡb, Ꮡb));
    fmt.Println(ptrCasOkˢ, Ꮡp.CompareAndSwap(Ꮡa, Ꮡb));
    fmt.Println(ptrFinalˢ, Ꮡp.Load().Value);
    var old = Ꮡp.Swap(Ꮡa);
    fmt.Println(ptrSwapˢ, old.Value, Ꮡp.Load().Value);
    ᏑgHolder.of(holder.Ꮡcount).Store(42);
    ᏑgHolder.of(holder.Ꮡcount).Add(8);
    fmt.Println(globalFieldˢ, ᏑgHolder.of(holder.Ꮡcount).Load());
}

} // end main_package

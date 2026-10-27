namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct holder {
    internal atomic.Int64 count;
}

internal static readonly ж<holder> ᏑgHolder = new StaticBox<holder>(default(holder));
internal static ref holder gHolder => ref ((StaticBox<holder>)ᏑgHolder).ValueSlot;

internal static void Main() {
    ref var n = ref heap(new atomic.Int32(), out var Ꮡn);
    Ꮡn.Store(10);
    fmt.Println("add:", Ꮡn.Add(5));
    fmt.Println("load:", Ꮡn.Load());
    fmt.Println("swap:", Ꮡn.Swap(100));
    fmt.Println("cas ok:", Ꮡn.CompareAndSwap(100, 7));
    fmt.Println("cas no:", Ꮡn.CompareAndSwap(100, 8));
    fmt.Println("final:", Ꮡn.Load());

    ref var p = ref heap(new atomic.Pointer<nint>(), out var Ꮡp);
    fmt.Println("ptr nil:", Ꮡp.Load() == nil);
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    Ꮡp.Store(Ꮡa);
    fmt.Println("ptr load:", Ꮡp.Load().Value);
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    fmt.Println("ptr cas no:", Ꮡp.CompareAndSwap(Ꮡb, Ꮡb));
    fmt.Println("ptr cas ok:", Ꮡp.CompareAndSwap(Ꮡa, Ꮡb));
    fmt.Println("ptr final:", Ꮡp.Load().Value);
    var old = Ꮡp.Swap(Ꮡa);
    fmt.Println("ptr swap:", old.Value, Ꮡp.Load().Value);

    ᏑgHolder.of(holder.Ꮡcount).Store(42);
    ᏑgHolder.of(holder.Ꮡcount).Add(8);
    fmt.Println("global field:", ᏑgHolder.of(holder.Ꮡcount).Load());
}

} // end main_package

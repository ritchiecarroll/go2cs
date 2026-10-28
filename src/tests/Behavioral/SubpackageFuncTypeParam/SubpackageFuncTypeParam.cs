namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

// type applyFunc is a methodless func type — rendered inline as its base delegate

internal static int32 run(Func<ж<atomic.Int32>, int32> f, ж<atomic.Int32> c) {
    return f(c);
}

internal static void Main() {
    ref var c = ref heap(new atomic.Int32(), out var Ꮡc);
    Ꮡc.Store(5);
    fmt.Println(run((ж<atomic.Int32> cΔ1) => cΔ1.Load(), Ꮡc));
    Ꮡc.Add(10);
    fmt.Println(run((ж<atomic.Int32> cΔ2) => cΔ2.Load(), Ꮡc));
}

} // end main_package

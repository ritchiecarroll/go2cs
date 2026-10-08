namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct counters {
    internal array<atomic.Int32> c = new(3);
    internal array<atomic.Uint64> d = new(2);
}

internal static void touch(ref atomic.Int32 p) {
}

internal static ж<atomic.Int32> last(any vals) {
    var ptrs = vals._<slice<ж<atomic.Int32>>>();
    return ptrs[len(ptrs) - 1];
}

internal static void Main() {
    ref var x = ref heap(new counters(), out var Ꮡx);
    touch(ref x.c[0]);
    touch(ref x.c[2]);
    slice<ж<atomic.Int32>> ptrs = default!;
    foreach (var (i, _) in x.c) {
        ptrs = append(ptrs, Ꮡx.at(counters.Ꮡc, i));
    }
    last(ptrs).Store(7);
    fmt.Println(len(x.c), len(x.d), Ꮡx.at(counters.Ꮡc, 2).Load());
}

} // end main_package

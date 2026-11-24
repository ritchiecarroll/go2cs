namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal delegate int32 applyFunc(ж<atomic.Int32> c);

internal static int32 run(applyFunc f, ж<atomic.Int32> c) {
    return f(c);
}

internal static void main() {
    ref var c = ref heap(new atomic.Int32(), out var Ꮡc);
    Ꮡc.Store(5);
    fmt.Println(run([GoFunc("main.func1")] (ж<atomic.Int32> cΔ1) => cΔ1.Load(), Ꮡc));
    Ꮡc.Add(10);
    fmt.Println(run([GoFunc("main.func2")] (ж<atomic.Int32> cΔ2) => cΔ2.Load(), Ꮡc));
}

} // end main_package

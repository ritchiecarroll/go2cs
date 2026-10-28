namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static void Main() {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    fmt.Println(Ꮡv.Load() == nil);

    @string a = "alpha";
    @string b = "beta";
    @string c = "gamma";

    Ꮡv.Store(a);
    fmt.Println(AreEqual(Ꮡv.Load(), a));

    var old = Ꮡv.Swap(b);
    fmt.Println(AreEqual(old, a), AreEqual(Ꮡv.Load(), b));

    fmt.Println(Ꮡv.CompareAndSwap(b, c), AreEqual(Ꮡv.Load(), c));
    fmt.Println(Ꮡv.CompareAndSwap(b, a), AreEqual(Ꮡv.Load(), c));
}

} // end main_package

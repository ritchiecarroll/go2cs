namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string betaˢ = "beta"u8;
private static readonly @string gammaˢ = "gamma"u8;

internal static void Main() {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    fmt.Println(Ꮡv.Load() == default!);

    @string a = alphaˢ;
    @string b = betaˢ;
    @string c = gammaˢ;

    Ꮡv.Store(a);
    fmt.Println(AreEqual(Ꮡv.Load(), a));

    var old = Ꮡv.Swap(b);
    fmt.Println(AreEqual(old, a), AreEqual(Ꮡv.Load(), b));

    fmt.Println(Ꮡv.CompareAndSwap(b, c), AreEqual(Ꮡv.Load(), c));
    fmt.Println(Ꮡv.CompareAndSwap(b, a), AreEqual(Ꮡv.Load(), c));
}

} // end main_package

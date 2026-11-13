namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct T {
    internal nint n;
}

partial struct holder {
    internal ж<T> p;
}

internal static holder hold;

partial struct Δq {
}

internal static void q(this ref T h) { }

internal static void poke(@unsafe.Pointer p, nint v) { ((ж<nint>)(uintptr)(p)).Value = v; }

internal static void viaReceiver(this ж<T> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    poke((uintptr)@unsafe.Pointer.FromRef(ref h), 7);
}

internal static void viaLocal(this ж<T> Ꮡh) {
    var p = Ꮡh;
    poke(@unsafe.Pointer.FromPinnedBox(p), 8);
}

internal static void viaParam(this ref T h, ж<T> Ꮡother) {
    ref var other = ref Ꮡother.DerefOrNull();

    poke((uintptr)@unsafe.Pointer.FromRef(ref other), 9);
}

internal static void viaFolded(this ж<T> Ꮡh) {
    var p = Ꮡh;
    (p.Reinterpret<T, nint>()).Value = 10;
}

internal static void viaField(this ref T h) { poke(@unsafe.Pointer.FromPinnedBox(hold.p), 11); }

internal static void viaRenamedReceiver(this ж<T> Ꮡq) {
    ref var Δq = ref Ꮡq.DerefOrNull();

    poke((uintptr)@unsafe.Pointer.FromRef(ref Δq), 12);
}

internal static void viaLocalChain(this ж<T> Ꮡh) {
    var p = Ꮡh;
    var r = p;
    poke(@unsafe.Pointer.FromPinnedBox(r), 13);
}

internal static void main() {
    ref var a = ref heap(new T(), out var Ꮡa);
    ref var b = ref heap(new T(), out var Ꮡb);
    T c = default;
    ref var d = ref heap(new T(), out var Ꮡd);
    ref var e = ref heap(new T(), out var Ꮡe);
    T f = default;
    ref var g = ref heap(new T(), out var Ꮡg);
    ref var k = ref heap(new T(), out var Ꮡk);
    Ꮡa.viaReceiver();
    Ꮡb.viaLocal();
    c.viaParam(Ꮡd);
    Ꮡe.viaFolded();
    hold.p = Ꮡg;
    f.viaField();
    Ꮡk.viaRenamedReceiver();
    ref var m = ref heap(new T(), out var Ꮡm);
    Ꮡm.viaLocalChain();

    fmt.Println(a.n, b.n, c.n, d.n, e.n, f.n, g.n, k.n, m.n);

    Δq _ᴛ1 = default;
}

} // end main_package

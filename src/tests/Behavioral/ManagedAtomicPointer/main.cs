namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

partial struct proc {
    internal nint addr;
}

partial struct lazyProc {
    internal ж<proc> p;
}

internal static bool find(this ж<lazyProc> Ꮡl) {
    if (atomic.LoadPointer(Ꮡl.of(lazyProc.Ꮡp)) == nil) {
        atomic.StorePointer(Ꮡl.of(lazyProc.Ꮡp), Ꮡ(new proc(addr: 42)));
        return true;
    }
    return false;
}

internal static void identity() {
    ref var a = ref heap(new int32(), out var Ꮡa);
    ref var b = ref heap(new int32(), out var Ꮡb);
    @unsafe.Pointer pa = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    @unsafe.Pointer pb = @unsafe.Pointer.FromPinnedBox(Ꮡb);
    @unsafe.Pointer pn = default!;
    fmt.Println(pa == pa, pa == pb, pa != pb);
    fmt.Println(pn == pn, pn == pa, pa != pn);
    fmt.Println(@unsafe.Pointer.FromPinnedBox(Ꮡa) == pa);
    var table = new @unsafe.Pointer[]{default!, pa, pb}.slice();
    nint same = 0;
    nint diff = 0;
    foreach (var (_, p) in table) {
        @unsafe.Pointer k = p;
        if (k == p) {
            same++;
        }
        if (k != table[0]) {
            diff++;
        }
    }
    fmt.Println(same, diff);
    ref var l = ref heap(new lazyProc(), out var Ꮡl);
    @unsafe.Pointer fp = @unsafe.Pointer.FromBox(Ꮡl.of(lazyProc.Ꮡp));
    fmt.Println(fp == @unsafe.Pointer.FromBox(Ꮡl.of(lazyProc.Ꮡp)), fp == pa);
}

internal static void Main() {
    ref var l = ref heap(new lazyProc(), out var Ꮡl);
    fmt.Println(Ꮡl.find());
    fmt.Println(Ꮡl.find());
    fmt.Println((~l.p).addr);
    identity();
}

} // end main_package

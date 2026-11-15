namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

partial struct rec {
    internal int32 cnt;
    internal int32 id;
}

partial struct rec2 {
    internal int32 cnt;
    internal int32 id;
}

partial struct outer {
    internal @string tag;
    internal rec @in;
}

internal static void main() {
    ref var g = ref heap(new rec(), out var Ꮡg);
    @unsafe.Pointer gp = @unsafe.Pointer.FromPinnedBox(Ꮡg);

    atomic.StoreInt32(@unsafe.Pointer.To<rec>(gp).Ꮡid, 3);
    fmt.Println("atomic store:", g.id);

    (@unsafe.Pointer.To<rec>(gp).Ꮡcnt).Value = 11;
    fmt.Println("union write:", g.cnt);

    var q = @unsafe.Pointer.To<rec>(gp).Ꮡid;
    q.Value = 9;
    fmt.Println("kept address:", g.id, q == Ꮡg.Ꮡid);

    fmt.Println("parenthesized CAS:", atomic.CompareAndSwapInt32((@unsafe.Pointer.To<rec>(gp)).Ꮡid, 9, 4), g.id);

    ref var o = ref heap(new outer(), out var Ꮡo);
    @unsafe.Pointer op = @unsafe.Pointer.FromPinnedBox(Ꮡo);
    atomic.AddInt32(@unsafe.Pointer.To<outer>(op).Ꮡin.Ꮡcnt, 5);
    fmt.Println("nested field:", o.@in.cnt);

    ref var h = ref heap(new rec2(), out var Ꮡh);
    atomic.StoreInt32(Ꮡh.Reinterpret<rec2, rec>().Ꮡcnt, 6);
    fmt.Println("named pointer conversion:", h.cnt);

    ж<rec> mk() => Ꮡg;
    atomic.AddInt32(mk().Ꮡcnt, 1);
    fmt.Println("call base:", g.cnt);
}

} // end main_package

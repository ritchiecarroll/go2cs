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

partial class recp /*ж<rec>*/;

internal static partial ж<rec> same(ж<rec> p) { return p.nilcheck(); }

internal static ж<T> through<T>(ж<T> p)
    /* where P : *T (erased: P renders as ж<T>) */
{ return p.nilcheck(); }

internal static any /*text*/ nilIndirection() {
    any text = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("nilIndirection.func1")] () => {
            text = recover();
        }, ref ᒐ);

        ж<rec> z = default;
        var q = same(z);
        fmt.Println("unreachable:", q == nil);

        text = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return text;
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

    [GoFunc("main.func1")] ж<rec> mk() => Ꮡg;
    atomic.AddInt32(mk().Ꮡcnt, 1);
    fmt.Println("call base:", g.cnt);

    atomic.StoreInt32(@unsafe.Pointer.To<rec>(gp).Ꮡid, 13);
    fmt.Println("explicit star:", g.id);

    (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<rec>(gp).Ꮡcnt), 4))).Value = 14;
    fmt.Println("explicit star, added:", g.id);

    (@unsafe.Pointer.To<rec>(gp).Ꮡid.Reinterpret<int32, uint16>()).Value = 0xBEEF;
    fmt.Printf("narrow store: %#x\n"u8, g.id);

    ref var r = ref heap<ж<rec>>(Ꮡg, out var Ꮡr);
    var s = r.nilcheck();
    s.cnt = 21;
    fmt.Println("address of indirection:", g.cnt, s == r);

    (@unsafe.Pointer.To<rec>(gp).nilcheck().Reinterpret<rec, uint32>()).Value = 31;
    (@unsafe.Pointer.To<rec>(gp).nilcheck().Reinterpret<rec, uint32>()).Value <<= 1;
    fmt.Println("whole-object union:", g.cnt, ~@unsafe.Pointer.To<rec>(gp).nilcheck().Reinterpret<rec, uint32>());

    recp np = Ꮡg;
    var t = ((ж<rec>)np).nilcheck();
    t.id = 22;
    var pp = Ꮡr;
    var u = (pp.ValueSlot).nilcheck();
    u.cnt = 23;
    fmt.Println("named pointer, two stars:", g.id, g.cnt, t == r, u == r);

    var w = through<rec>(r);
    w.id = 24;
    fmt.Println("type parameter:", g.id, w == r);

    (@unsafe.Pointer.To<rec>(gp).Ꮡcnt).Value++;
    (@unsafe.Pointer.To<rec>(gp).Ꮡid).Value--;
    var ip = Ꮡg.Ꮡcnt;
    ip.Value++;
    ip.Value++;
    ip.Value--;
    fmt.Println("increment:", g.cnt, g.id);

    var e = r.nilcheck();
    e.cnt = 41;
    ref var list = ref heap<slice<int32>>(slice<int32>([1, 2]), out var Ꮡlist);
    var lp = Ꮡlist;
    (lp.nilcheck()).ValueSlot = [7, 8, 9];
    fmt.Println("parenthesized indirection:", g.cnt, e == r, len(list), list[2], lp.nilcheck() == lp);

    fmt.Println("nil indirection:", nilIndirection());
}

} // end main_package

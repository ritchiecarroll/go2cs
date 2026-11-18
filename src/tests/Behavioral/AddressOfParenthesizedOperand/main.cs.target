namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class main_package {

partial struct mutex {
    internal int32 owner;
    internal int32 count;
}

partial struct @lock {
    internal int32 owner;
    internal int32 count;
}

partial struct point {
    public nint X, Y;
}

internal static void main() {
    ref var m = ref heap(new mutex(), out var Ꮡm);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡm);
    atomic.CompareAndSwapInt32(@unsafe.Pointer.To<mutex>(p).Ꮡowner, 0, 7);
    var pc = @unsafe.Pointer.To<mutex>(p).Ꮡcount;
    pc.Value = 3;
    var po = (@unsafe.Pointer.To<mutex>(p)).Ꮡowner;
    po.Value += 10;
    fmt.Println("converted:", m.owner, m.count);

    atomic.AddInt32(@unsafe.Pointer.To<mutex>(p).Ꮡcount, 1);
    fmt.Println("unparenthesized:", m.count);

    ref var l = ref heap(new @lock(), out var Ꮡl);
    @unsafe.Pointer lp = @unsafe.Pointer.FromPinnedBox(Ꮡl);
    atomic.StoreInt32(@unsafe.Pointer.To<@lock>(lp).Ꮡowner, 21);
    atomic.AddInt32(@unsafe.Pointer.To<@lock>(lp).Ꮡcount, 2);
    fmt.Println("only parenthesized:", l.owner, l.count);

    ref var pt = ref heap(new point(), out var Ꮡpt);
    var px = Ꮡpt.ᏑX;
    px.Value = 5;
    var py = Ꮡpt.ᏑY;
    py.Value = 6;
    fmt.Println("field:", pt.X, pt.Y);

    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    var pe = Ꮡarr.at<nint>(1);
    pe.Value = 9;
    var pa = Ꮡarr.at<nint>(2);
    pa.Value = 8;
    fmt.Println("element:", arr);

    ref var n = ref heap<nint>(1, out var Ꮡn);
    var pn = Ꮡn;
    pn.Value = 2;
    fmt.Println("local:", n);

    var pp = Ꮡpt;
    var pf = pp.ᏑX;
    pf.Value = 11;
    var pg = pp.ᏑY;
    pg.Value = 12;
    fmt.Println("pointer:", pt.X, pt.Y);
}

} // end main_package

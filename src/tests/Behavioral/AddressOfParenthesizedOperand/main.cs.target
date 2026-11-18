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

partial struct point {
    public nint X, Y;
}

internal static void main() {
    ref var m = ref heap(new mutex(), out var Ꮡm);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡm);
    atomic.CompareAndSwapInt32(Ꮡ(((@unsafe.Pointer.To<mutex>(p)).Value.owner)), 0, 7);
    var pc = Ꮡ(((@unsafe.Pointer.To<mutex>(p)).Value.count));
    pc.Value = 3;
    fmt.Println("converted:", m.owner, m.count);

    atomic.AddInt32(@unsafe.Pointer.To<mutex>(p).Ꮡcount, 1);
    fmt.Println("unparenthesized:", m.count);

    ref var pt = ref heap(new point(), out var Ꮡpt);
    var px = Ꮡ(((pt).X));
    px.Value = 5;
    var py = Ꮡ((pt.Y));
    py.Value = 6;
    fmt.Println("field:", pt.X, pt.Y);

    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    var pe = Ꮡ((arr[1]));
    pe.Value = 9;
    fmt.Println("element:", arr);

    nint n = 1;
    var pn = Ꮡ((n));
    pn.Value = 2;
    fmt.Println("local:", n);

    var pp = Ꮡpt;
    var pf = Ꮡ(((pp.Value).X));
    pf.Value = 11;
    var pg = Ꮡ((pp.Y));
    pg.Value = 12;
    fmt.Println("pointer:", pt.X, pt.Y);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct small /*[8]byte*/;

partial struct holder {
    internal int32 n;
    internal array<uint32> arr = new(4);
}

internal static readonly ж<array<uint32>> Ꮡpkg = new StaticBox<array<uint32>>(new array<uint32>(4));
internal static ref array<uint32> pkg => ref ((StaticBox<array<uint32>>)Ꮡpkg).ValueSlot;

internal static void main() {
    ref var a = ref heap(new array<uint32>(4), out var Ꮡa);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    a.Assign(new uint32[]{1, 2, 3, 4}.array());
    (@unsafe.Pointer.To<uint32>(p)).Value = 41;
    fmt.Println("local:", a[0]);

    ref var b = ref heap(new array<uint32>(4), out var Ꮡb);
    var n = (uintptr)Ꮡb;
    b.Assign(new uint32[]{1, 2, 3, 4}.array());
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)n)).Value = 42;
    fmt.Println("local by number:", b[0]);

    ref var c = ref heap(new array<uint32>(4), out var Ꮡc);
    array<uint32> other = new(4);
    other[0] = 9;
    @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(Ꮡc);
    c.Assign(other);
    (@unsafe.Pointer.To<uint32>(q)).Value = 43;
    fmt.Println("from a variable:", c[0], other[0]);

    ref var s = ref heap(new small(), out var Ꮡs);
    s[0] = 1;
    @unsafe.Pointer r = @unsafe.Pointer.FromPinnedBox(Ꮡs);
    s.Assign(new small(new byte[]{9, 9}.array(8)));
    (@unsafe.Pointer.To<byte>(r)).Value = 44;
    fmt.Println("named:", s[0]);

    var m = (uintptr)Ꮡpkg;
    pkg.Assign(new uint32[]{5, 6, 7, 8}.array());
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)m)).Value = 45;
    fmt.Println("package:", pkg[0]);

    ref var h = ref heap(new holder(), out var Ꮡh);
    @unsafe.Pointer f = @unsafe.Pointer.FromPinnedBox(Ꮡh.Ꮡarr);
    h.arr.Assign(new uint32[]{1, 2, 3, 4}.array());
    (@unsafe.Pointer.To<uint32>(f)).Value = 46;
    fmt.Println("field:", h.arr[0]);

    ref var d = ref heap(new array<uint32>(4), out var Ꮡd);
    @unsafe.Pointer t = @unsafe.Pointer.FromPinnedBox(Ꮡd);
    d.Assign(new uint32[]{7, 7, 7, 7}.array());
    fmt.Println("read:", ~@unsafe.Pointer.To<uint32>(t));

    ref var e = ref heap(new array<uint32>(4), out var Ꮡe);
    @unsafe.Pointer u = @unsafe.Pointer.FromPinnedBox(Ꮡe);
    e.Assign(new uint32[]{1, 2, 3, 4}.array());
    (@unsafe.Pointer.To<array<uint32>>(u)).Value[0] = 47;
    fmt.Println("array pointer:", e[0]);

    ref var g = ref heap(new array<uint32>(4), out var Ꮡg);
    @unsafe.Pointer v = @unsafe.Pointer.FromPinnedBox(Ꮡg);
    _ = (uintptr)v;
    g.Assign(new uint32[]{1, 2, 3, 4}.array());
    @unsafe.Pointer.View<byte>(v, 0, 4, 16)[0] = 48;
    fmt.Println("byte view:", g[0]);

    ref var k = ref heap(new array<uint32>(4), out var Ꮡk);
    @unsafe.Pointer w = @unsafe.Pointer.FromPinnedBox(Ꮡk);
    k[0] = 1;
    (@unsafe.Pointer.To<uint32>(w)).Value = 49;
    fmt.Println("control:", k[0]);
}

} // end main_package

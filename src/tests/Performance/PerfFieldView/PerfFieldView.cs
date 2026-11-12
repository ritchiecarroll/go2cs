namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct element {
    internal array<uint64> v = new(4);
}

partial struct point {
    internal element x, y, z;
}

internal static ж<element> Add(this ж<element> Ꮡe, ж<element> Ꮡa, ж<element> Ꮡb) {
    ref var e = ref Ꮡe.DerefOrNull();
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < 4; i++) {
        e.v[i] = a.v[i] + b.v[i];
    }
    return Ꮡe;
}

internal static ж<element> Mul(this ж<element> Ꮡe, ж<element> Ꮡa, ж<element> Ꮡb) {
    ref var e = ref Ꮡe.DerefOrNull();
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < 4; i++) {
        e.v[i] = a.v[i] * (b.v[i] | 1) ^ e.v[i] >> 7;
    }
    return Ꮡe;
}

internal static ж<point> step(this ж<point> Ꮡp) {
    Ꮡp.Ꮡx.Add(Ꮡp.Ꮡy, Ꮡp.Ꮡz);
    Ꮡp.Ꮡy.Mul(Ꮡp.Ꮡz, Ꮡp.Ꮡx);
    Ꮡp.Ꮡz.Add(Ꮡp.Ꮡx, Ꮡp.Ꮡy);
    Ꮡp.Ꮡz.Mul(Ꮡp.Ꮡz, Ꮡp.Ꮡy);
    return Ꮡp;
}

internal static void main() {
    var start = time.Now().UnixNano();

    ref var p = ref heap(new point(), out var Ꮡp);
    p.x.v = new uint64[]{0x9e3779b97f4a7c15UL, 0xbf58476d1ce4e5b9UL, 0x94d049bb133111ebUL, 0x2545f4914f6cdd1dUL}.array();
    p.y.v = new uint64[]{1, 2, 3, 4}.array();
    p.z.v = new uint64[]{5, 6, 7, 8}.array();
    for (nint r = 0; r < 2000000; r++) {
        Ꮡp.step();
    }

    var pts = new slice<point>(64, () => new());
    foreach (var i in range(len(pts))) {
        pts[i].x.v[0] = (uint64)i;
        pts[i].y.v[1] = (uint64)i * 3;
    }
    for (nint r = 0; r < 30000; r++) {
        foreach (var i in range(len(pts))) {
            Ꮡ(pts, i).step();
        }
    }

    var sum = p.x.v[0] ^ p.y.v[1] ^ p.z.v[2] ^ p.z.v[3];
    foreach (var i in range(len(pts))) {
        sum ^= (uint64)(pts[i].z.v[(i & 3)]);
    }

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", sum);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package

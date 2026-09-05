namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 20, 30, 40}.array();
    var s = (~Ꮡarr)[..4];
    nint sum = 0;
    foreach (var (i, _) in s) {
        sum += i;
    }
    nint total = 0;
    foreach (var (_, v) in s) {
        total += v;
    }
    foreach (var (i, _) in s) {
        var ps = Ꮡ(s, i);
        ps.Value += 1;
    }
    fmt.Println(sum, total, s[0]);
    ref var ip = ref heap<ж<array<nint>>>(out var Ꮡip);
    ip = Ꮡarr;
    if (never) {
        var back = (~Ꮡip.Reinterpret<ж<array<nint>>, ж<array<int64>>>()).Value.Clone();
        _ = back;
    }
    @unsafe.Pointer pick(bool u) {
        if (u) {
            return (@unsafe.Pointer)(uintptr)0;
        }
        return @unsafe.Pointer.FromPinnedBox(Ꮡip.ValueSlot);
    }
    _ = (uintptr)pick(true);
    var op = ((opaque)(ж<array<nint>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(ip)));
    _ = op;
}

[GoType("ж<array<nint>>")] partial class opaque;

internal static bool never;

} // end main_package

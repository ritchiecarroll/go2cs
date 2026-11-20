namespace go.github.com.dustin.go_humanize;

using big = go.math.big_package;
using go.math;

partial class humanize_package {

internal static (float64, nint) oomm(ж<big.Int> Ꮡn, ж<big.Int> Ꮡb, nint maxmag) {
    ref var n = ref Ꮡn.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    nint mag = 0;
    var m = Ꮡ(new big.Int());
    while (Ꮡn.Cmp(Ꮡb) >= 0) {
        Ꮡn.DivMod(Ꮡn, Ꮡb, m);
        mag++;
        if (mag == maxmag && maxmag >= 0) {
            break;
        }
    }
    return ((float64)n.Int64() + ((float64)m.Int64() / (float64)b.Int64()), mag);
}

internal static (float64, nint) oom(ж<big.Int> Ꮡn, ж<big.Int> Ꮡb) {
    ref var n = ref Ꮡn.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    nint mag = 0;
    var m = Ꮡ(new big.Int());
    while (Ꮡn.Cmp(Ꮡb) >= 0) {
        Ꮡn.DivMod(Ꮡn, Ꮡb, m);
        mag++;
    }
    return ((float64)n.Int64() + ((float64)m.Int64() / (float64)b.Int64()), mag);
}

} // end humanize_package

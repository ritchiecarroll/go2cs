namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Builder {
    internal nint n;
}

public static void With(this ж<Builder> Ꮡb, Action<ж<Builder>> f) {
    f(Ꮡb);
}

public static nint Sum(this ref Builder b) {
    return b.n;
}

internal static nint compute() {
    nint inner() {
        ref var bΔ1 = ref heap(new Builder(), out var ᏑbΔ1);
        ᏑbΔ1.With((ж<Builder> bΔ2) => {
            bΔ2.n += 1;
        });
        ᏑbΔ1.With((ж<Builder> bΔ3) => {
            bΔ3.n += 2;
        });
        ᏑbΔ1.With((ж<Builder> bΔ4) => {
            bΔ4.n += 3;
        });
        return bΔ1.Sum();
    }

    nint a = inner();

    ref var b = ref heap(new Builder(), out var Ꮡb);
    Ꮡb.With((ж<Builder> bΔ5) => {
        bΔ5.n += a;
    });
    Ꮡb.With((ж<Builder> bΔ6) => {
        bΔ6.n += 10;
    });
    return b.Sum();
}

internal static void Main() {
    fmt.Println(compute());
}

} // end main_package

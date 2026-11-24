namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Builder {
    internal nint n;
}

public static void With(this ж<Builder> Ꮡb, Action<ж<Builder>> f) { f(Ꮡb); }
public static nint Sum(this ref Builder b) { return b.n; }

internal static nint compute() {
    [GoFunc("compute.func1")] nint inner() {
        ref var bΔ1 = ref heap(new Builder(), out var ᏑbΔ1);
        ᏑbΔ1.With([GoFunc("compute.func1.1")] (ж<Builder> bΔ2) => {
            bΔ2.n += 1;
        });
        ᏑbΔ1.With([GoFunc("compute.func1.2")] (ж<Builder> bΔ3) => {
            bΔ3.n += 2;
        });
        ᏑbΔ1.With([GoFunc("compute.func1.3")] (ж<Builder> bΔ4) => {
            bΔ4.n += 3;
        });
        return bΔ1.Sum();
    }

    nint a = inner();

    ref var b = ref heap(new Builder(), out var Ꮡb);
    Ꮡb.With([GoFunc("compute.func2")] (ж<Builder> bΔ5) => {
        bΔ5.n += a;
    });
    Ꮡb.With([GoFunc("compute.func3")] (ж<Builder> bΔ6) => {
        bΔ6.n += 10;
    });
    return b.Sum();
}

internal static void main() {
    fmt.Println(compute());
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint compute(nint n) {
    nint total = 0;
    nint count = 0;
    for (;;) {
        for (nint offΔ1 = 0; offΔ1 < n; offΔ1++) {
            total += offΔ1;
        }
        for (nint offΔ2 = n; offΔ2 > 0; offΔ2--) {
            total += offΔ2;
        }
        nint off = n * 10;
        total += off;
        count++;
        if (count >= 2) {
            return total;
        }
    }
}

internal static void Main() {
    fmt.Println(compute(3));
}

} // end main_package

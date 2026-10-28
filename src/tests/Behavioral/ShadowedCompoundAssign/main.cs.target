namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    nint x = 100;
    {
        nint xΔ1 = 5;
        xΔ1 += 3;
        xΔ1 *= 2;
        fmt.Println(xΔ1);
    }
    fmt.Println(x);

    nint sum = 1000;
    nint i = 50;
    _ = sum;
    _ = i;
    nint total = func(nint () => {
        nint sumΔ1 = 0;
        for (nint iΔ1 = 1; iΔ1 <= 4; iΔ1++) {
            sumΔ1 += iΔ1;
        }
        return sumΔ1;
    })();
    fmt.Println(total);
    fmt.Println(sum);

    nint acc = 7;
    for (nint n = 0; n < 3; n++) {
        nint accΔ1 = 0;
        accΔ1 += n;
        accΔ1 -= 1;
        fmt.Print(accΔ1, " ");
    }
    fmt.Println();
    fmt.Println(acc);
}

} // end main_package

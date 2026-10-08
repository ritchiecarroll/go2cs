namespace go;

using fmt = fmt_package;

partial class main_package {

public static uintptr MaxU => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);

internal static UntypedInt ptrSize => 8;

internal partial struct main_rows /*dyn*/ {
    internal uintptr a, b;
}

internal static void Main() {
    uintptr zero = 0;
    uintptr one = 1;
    fmt.Println(MaxU > zero);
    fmt.Println(MaxU + one == zero);
    fmt.Println(MaxU - one < MaxU);
    uintptr big = ((uintptr)1 << (int)((4 * ptrSize)));
    fmt.Println(big);
    fmt.Println(big > 1);
    uintptr wide = ((uintptr)1 << (int)(40));
    fmt.Println(wide);
    var rows = new main_rows[]{
        new(((uintptr)1 << (int)((4 * ptrSize))), ((uintptr)1 << (int)((4 * ptrSize)))),
        new((uintptr)(4294967296L - 1), (uintptr)(4294967296L - 1))
    }.slice();
    fmt.Println(rows[0].a, rows[0].b);
    fmt.Println(rows[1].a, rows[1].b);
}

} // end main_package

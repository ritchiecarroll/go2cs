namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt procs => 14;

internal static UntypedFloat capacityPerProc => 1e9;

internal static UntypedInt three => 3;

internal static UntypedFloat half => 0.5;

internal static UntypedInt seven => 7;

internal static UntypedFloat two => 2.0;

internal static UntypedFloat quadrillion => 1e15;

internal static void Main() {
    uint64 capacity = 14000000000UL;
    fmt.Println(capacity == (uint64)(/* procs * capacityPerProc */ 14000000000UL), capacity == (uint64)(/* capacityPerProc * procs */ 14000000000UL));
    fmt.Println((uint64)(/* procs * capacityPerProc */ 14000000000UL), (uint64)(/* capacityPerProc * procs */ 14000000000UL));

    fmt.Println((float64)(/* three * half */ 1.5D), (float64)(/* half * three */ 1.5D), /* three * half */ 1.5D, /* half * three */ 1.5F);

    fmt.Println(three < half * seven, half * seven > three, three == seven * half - half, half * seven != three);

    fmt.Println((float64)(/* seven / two */ 3.5D), two / seven > 0, /* seven / two */ 3.5D, (nint)((nint)(/* seven / two * two */ 7L)), (nint)(seven / three));

    fmt.Println((uint64)(/* procs * quadrillion */ 14000000000000000UL), (uint64)(/* quadrillion * procs */ 14000000000000000UL));
}

} // end main_package

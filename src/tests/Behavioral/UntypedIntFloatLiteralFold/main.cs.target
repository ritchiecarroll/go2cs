namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt retainExtraPercent => 10;

internal static UntypedInt baseline => /* 100 << 20 */ 104857600;

internal static UntypedInt pct => 29;

internal static UntypedInt procs => 14;

internal static UntypedFloat capacityPerProc => 1e9;

partial struct duration /*num:int64*/;

internal static int64 advance(duration d) {
    return (int64)d;
}

internal static void Main() {
    fmt.Println((uint64)(/* 1.0 / (retainExtraPercent / 100.0) */ 10UL));
    fmt.Println((nint)((nint)(/* 1.2 * baseline */ 125829120L)), (int64)(/* 1.5 * baseline */ 157286400L), (nint)((nint)(/* 0.2 * baseline */ 20971520L)));
    fmt.Println((uint64)(/* pct / 100.0 * 100 */ 29UL));
    fmt.Println((nint)(unchecked((nint)(14000000000L))));
    uint64 capacity = 14000000000UL;
    fmt.Println(capacity == (uint64)(/* procs * capacityPerProc */ 14000000000UL), capacity == (uint64)(/* capacityPerProc * procs */ 14000000000UL));
    int64 want = /* 1.5 * baseline */ 157286400L;
    fmt.Println(want);
    fmt.Println(advance((duration)(28000000000L)));
}

} // end main_package

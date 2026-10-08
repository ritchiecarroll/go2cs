namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Time /*num:int64*/;

partial struct timestamp /*num:uint64*/;

internal static Time toTime(timestamp ts) {
    return ((Time)(int64)(uint64)ts);
}

internal static timestamp toTimestamp(Time t) {
    return ((timestamp)(uint64)(int64)t);
}

internal static void Main() {
    timestamp ts = 1234567890;
    var t = toTime(ts);
    fmt.Println((int64)t);
    var back = toTimestamp(t + 1);
    fmt.Println((uint64)back);
    timestamp big = 18446744073709551615UL;
    fmt.Println((int64)toTime(big));
}

} // end main_package

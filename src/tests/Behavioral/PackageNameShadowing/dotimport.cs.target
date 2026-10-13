namespace go;

using fmt = fmt_package;
using static time_package;
using Δtime = time_package;

partial class main_package {

internal static @string dotImported() {
    timeꓸMonth m = March;
    timeꓸWeekday w = Thursday;
    Δtime.Duration d = (Δtime.Duration)(3661000000001L);
    ж<timeꓸLocation> loc = ΔUTC;

    var stamp = Date(2011, m, 12, 3, 4, 5, 600000000, loc);

    return fmt.Sprint(
        m, (@string)" "u8, w, (@string)" "u8, d, (@string)" "u8, loc.OrTypedNil(), (@string)" "u8, ΔLocal != nil, (@string)" "u8,
        stamp.Format(RFC3339Nano), (@string)" "u8,
        stamp.Hour(), stamp.Minute(), stamp.Second(), stamp.Nanosecond());
}

} // end main_package

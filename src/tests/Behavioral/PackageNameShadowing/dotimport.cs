namespace go;

using fmt = fmt_package;
using static time_package;
using Δtime = time_package;

partial class main_package {

internal static @string dotImported() {
    Δtime.Month m = March;
    Δtime.Weekday w = Thursday;
    Δtime.Duration d = (Δtime.Duration)(3661000000001L);
    ж<Δtime.Location> loc = UTC;

    var stamp = Date(2011, m, 12, 3, 4, 5, 600000000, loc);

    return fmt.Sprint(
        m, " ", w, " ", d, " ", loc.OrTypedNil(), " ", Local != nil, " ",
        stamp.Format(RFC3339Nano), " ",
        stamp.Hour(), stamp.Minute(), stamp.Second(), stamp.Nanosecond());
}

} // end main_package

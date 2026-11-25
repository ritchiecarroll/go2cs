namespace go;

using fmt = fmt_package;
using gotime = time_package;
using time = time_package;

partial class main_package {

partial struct tzLocation {
    internal @string tz;
    internal ж<gotime.Location> loc;
}

internal static ж<gotime.Location> @fixed(@string name, nint offset) {
    return gotime.FixedZone(name, offset);
}

internal static gotime.Duration timeout(nint n) {
    return ((gotime.Duration)(int64)n) * gotime.Second + 250 * gotime.Millisecond;
}

internal static void main() {
    var z = new tzLocation(tz: "UTC", loc: gotime.UTC);
    fmt.Println(z.tz, z.loc.String());

    slice<ж<gotime.Location>> locs = default;
    locs = append(locs, gotime.UTC, @fixed("EST", -5 * 3600));
    foreach (var (_, l) in range(locs)) {
        fmt.Println(l.OrTypedNil());
    }

    fmt.Println(gotime.Local != nil, gotime.Local == gotime.UTC || gotime.Local != gotime.UTC);

    fmt.Println(timeout(3), gotime.Second * 2, gotime.Millisecond < gotime.Second);

    var t = gotime.Date(2024, gotime.March, 5, 6, 7, 8, 0, gotime.UTC);
    fmt.Println(t.In(@fixed("EST", -5 * 3600)).Format(gotime.RFC3339), t.Location() == gotime.UTC);

    gotime.Month m = (gotime.Month)(2 + 1);
    fmt.Println(m, t.Month() == m, (gotime.Month)12);
}

} // end main_package

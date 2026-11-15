namespace go;

using fmt = fmt_package;
using gotime = time_package;
using time = time_package;

partial class main_package {

partial struct tzLocation {
    internal @string tz;
    internal ж<gotime.ΔLocation> loc;
}

internal static ж<gotime.ΔLocation> @fixed(@string name, nint offset) {
    return gotime.FixedZone(name, offset);
}

internal static gotime.Duration timeout(nint n) {
    return ((gotime.Duration)(int64)n) * gotime.ΔSecond + 250 * gotime.Millisecond;
}

internal static void main() {
    var locsᴷ = new stackAppend<ж<gotime.ΔLocation>>();

    var z = new tzLocation(tz: "UTC", loc: gotime.ΔUTC);
    fmt.Println(z.tz, z.loc.String());

    slice<ж<gotime.ΔLocation>> locs = default;
    locs = append(locsᴷ, locs, gotime.ΔUTC, @fixed("EST", -5 * 3600));
    foreach (var (_, l) in range(locs)) {
        fmt.Println(l.OrTypedNil());
    }

    fmt.Println(gotime.ΔLocal != nil, gotime.ΔLocal == gotime.ΔUTC || gotime.ΔLocal != gotime.ΔUTC);

    fmt.Println(timeout(3), gotime.ΔSecond * 2, gotime.Millisecond < gotime.ΔSecond);

    var t = gotime.Date(2024, gotime.March, 5, 6, 7, 8, 0, gotime.ΔUTC);
    fmt.Println(t.In(@fixed("EST", -5 * 3600)).Format(gotime.RFC3339), t.Location() == gotime.ΔUTC);

    gotime.ΔMonth m = (gotime.ΔMonth)(2 + 1);
    fmt.Println(m, t.Month() == m, (gotime.ΔMonth)12);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using Δtime = time_package;
// blank import: go.time.tzdata_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class main_package {

internal static readonly @string layout = "2006-01-02T15:04:05.000000000Z07:00"u8;

internal static @string describe(Δtime.Time timeΔ1) {
    return fmt.Sprintf("%d-%v-%02d %02d:%02d:%02d.%09d %v"u8,
        timeΔ1.Year(), timeΔ1.Month(), timeΔ1.Day(),
        timeΔ1.Hour(), timeΔ1.Minute(), timeΔ1.Second(), timeΔ1.Nanosecond(), timeΔ1.Weekday());
}

internal static @string qualifiedDurations() {
    return fmt.Sprint(Δtime.ΔNanosecond, " ", Δtime.ΔSecond, " ", Δtime.ΔMinute, " ", Δtime.ΔHour);
}

internal static @string qualifiedLocations() {
    return fmt.Sprint(Δtime.ΔUTC.OrTypedNil(), " ", Δtime.ΔLocal != nil);
}

internal static (Δtime.Duration, float64, nint) foldedConstant() {
    var d = (Δtime.Duration)(28800000000000L);
    return (d, d.Seconds(), (nint)((Δtime.Duration)(28800000000000L)).Seconds());
}

internal static void Main() {
    var stamp = Δtime.Date(2009, Δtime.February, 4, 21, 0, 57, 12345600, Δtime.ΔUTC);

    fmt.Println("1:", describe(stamp));

    var timeΔ1 = stamp;
    fmt.Println("2:", timeΔ1.Format(layout));

    fmt.Println("3:", qualifiedDurations(), qualifiedLocations());

    var (d, secs, east) = foldedConstant();
    fmt.Println("4:", d, secs, east);

    nint frac = 4;
    fmt.Println("5:", ((@string)"012345678"u8.slice(0, frac)) + "000000000"u8.slice(0, 9 - frac));

    fmt.Println("6:", dotImported());
}

} // end main_package

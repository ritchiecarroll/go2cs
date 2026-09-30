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
    return fmt.Sprint(Δtime.ΔNanosecond, (@string)" "u8, Δtime.ΔSecond, (@string)" "u8, Δtime.ΔMinute, (@string)" "u8, Δtime.ΔHour);
}

internal static @string qualifiedLocations() {
    return fmt.Sprint(Δtime.ΔUTC.OrTypedNil(), (@string)" "u8, Δtime.ΔLocal != nil);
}

internal static (Δtime.Duration, float64, nint) foldedConstant() {
    var d = (Δtime.Duration)(28800000000000L);
    return (d, d.Seconds(), (nint)((Δtime.Duration)(28800000000000L)).Seconds());
}

internal static void Main() {
    var stamp = Δtime.Date(2009, Δtime.February, 4, 21, 0, 57, 12345600, Δtime.ΔUTC);
    fmt.Println((@string)"1:"u8, describe(stamp));
    var timeΔ1 = stamp;
    fmt.Println((@string)"2:"u8, timeΔ1.Format(layout));
    fmt.Println((@string)"3:"u8, qualifiedDurations(), qualifiedLocations());
    var (d, secs, east) = foldedConstant();
    fmt.Println((@string)"4:"u8, d, secs, east);
    nint frac = 4;
    fmt.Println((@string)"5:"u8, ((@string)"012345678"u8.slice(0, frac)) + "000000000"u8.slice(0, 9 - frac));
    fmt.Println((@string)"6:"u8, dotImported());
}

} // end main_package

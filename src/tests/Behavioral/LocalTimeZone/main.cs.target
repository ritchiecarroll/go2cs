namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    var t = time.Unix(1700000000, 0).In(time.ΔLocal);

    fmt.Println(t.Format("2006-01-02 15:04:05 MST"));
    fmt.Printf("%v\n"u8, t);
    fmt.Println(t.Weekday(), t.YearDay());

    var (name, offset) = t.Zone();
    fmt.Println(name, offset);
    fmt.Println(len(name) > 0, offset % 60 == 0);

    var local = time.Date(2023, time.November, 14, 22, 13, 20, 0, time.ΔLocal);
    fmt.Println(local.UTC().Format(time.RFC3339));
    fmt.Println(local.Equal(t));

    var (_, janOffset) = time.Date(2023, time.January, 15, 12, 0, 0, 0, time.ΔLocal).Zone();
    var (_, julOffset) = time.Date(2023, time.July, 15, 12, 0, 0, 0, time.ΔLocal).Zone();
    fmt.Println(janOffset == offset || julOffset == offset);

    fmt.Println(local.Location() == time.ΔLocal, time.ΔLocal.String());
}

} // end main_package

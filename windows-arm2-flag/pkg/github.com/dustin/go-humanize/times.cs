namespace go.github.com.dustin.go_humanize;

using fmt = fmt_package;
using math = math_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;

partial class humanize_package {

public static time.Duration Day => /* 24 * time.Hour */ 86400000000000;
public static time.Duration Week => /* 7 * Day */ 604800000000000;
public static time.Duration Month => /* 30 * Day */ 2592000000000000;
public static time.Duration Year => /* 12 * Month */ 31104000000000000;
public static time.Duration LongTime => /* 37 * Year */ 1150848000000000000;

public static @string Time(time.Time then) {
    return RelTime(then, time.Now(), "ago", "from now");
}

partial struct RelTimeMagnitude {
    public time.Duration D;
    public @string Format;
    public time.Duration DivBy;
}

[GoLazy] partial class lazyᴛdefaultMagnitudes { internal static slice<RelTimeMagnitude> defaultMagnitudes = [
    new(time.Second, "now", time.Second),
    new(2 * time.Second, "1 second %s", 1),
    new(time.Minute, "%d seconds %s", time.Second),
    new((time.Duration)(120000000000L), "1 minute %s", 1),
    new(time.Hour, "%d minutes %s", time.Minute),
    new((time.Duration)(7200000000000L), "1 hour %s", 1),
    new(Day, "%d hours %s", time.Hour),
    new((time.Duration)(172800000000000L), "1 day %s", 1),
    new(Week, "%d days %s", Day),
    new((time.Duration)(1209600000000000L), "1 week %s", 1),
    new(Month, "%d weeks %s", Week),
    new((time.Duration)(5184000000000000L), "1 month %s", 1),
    new(Year, "%d months %s", Month),
    new((time.Duration)(46656000000000000L), "1 year %s", 1),
    new((time.Duration)(62208000000000000L), "2 years %s", 1),
    new(LongTime, "%d years %s", Year),
    new(math.MaxInt64, "a long while %s", 1)
]; }

public static @string RelTime(time.Time a, time.Time b, @string albl, @string blbl) {
    return CustomRelTime(a, b, albl, blbl, defaultMagnitudes);
}

public static @string CustomRelTime(time.Time a, time.Time b, @string albl, @string blbl, slice<RelTimeMagnitude> magnitudes) {
    var argsᴷ = new stackAppend<any>();

    @string lbl = albl;
    var diff = b.Sub(a);

    if (a.After(b)) {
        lbl = blbl;
        diff = a.Sub(b);
    }

    var magnitudesʗ1 = magnitudes;
    nint n = sort.Search(len(magnitudes), (nint i) => magnitudesʗ1[i].D > diff);

    if (n >= len(magnitudes)) {
        n = len(magnitudes) - 1;
    }
    var mag = magnitudes[n];
    var args = slice<any>([]);
    var escaped = false;
    foreach (var (_, ch) in mag.Format) {
        if (escaped) {
            switch (ch) {
            case 's': {
                args = append(argsᴷ, args, (any)(lbl));
                break;
            }
            case 'd': {
                args = append(args, (any)(diff / mag.DivBy));
                break;
            }}

            escaped = false;
        } else {
            escaped = ch == '%';
        }
    }
    @string s = fmt.Sprintf(mag.Format, args.ꓸꓸꓸ);
    if (lbl == ""u8) {
        s = strings.TrimRight(s, " ");
    }
    return s;
}

} // end humanize_package

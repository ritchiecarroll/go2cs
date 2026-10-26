namespace go.github.com.spf13;

using fmt = fmt_package;
using strings = strings_package;
using time = time_package;

partial class pflag_package {

partial struct timeValue {
    /*embed*/ public ж<time_package.Time> Time;
    internal slice<@string> formats;
}

internal static ж<timeValue> newTimeValue(time.Time val, ж<time.Time> Ꮡp, slice<@string> formats) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡ(new timeValue(
        Time: Ꮡp,
        formats: formats
    ));
}

internal static error Set(this ref timeValue d, @string s) {
    s = strings.TrimSpace(s);
    foreach (var (_, f) in d.formats) {
        var (v, err) = time.Parse(f, s);
        if (err != default!) {
            continue;
        }
        d.Time.Value = v;
        return default!;
    }

    @string formatsString = "";
    foreach (var (i, f) in d.formats) {
        if (i > 0) {
            formatsString += ", "u8;
        }
        formatsString += fmt.Sprintf("`%s`"u8, f);
    }

    return fmt.Errorf("invalid time format `%s` must be one of: %s"u8, s, formatsString);
}

internal static @string Type(this ref timeValue d) {
    return "time";
}

internal static @string String(this ref timeValue d) {
    if ((~d.Time).IsZero()){
        return "";
    } else {
        return (~d.Time).Format(time.RFC3339Nano);
    }
}

public static (time.Time, error) GetTime(this ж<FlagSet> Ꮡf, @string name) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        var err = fmt.Errorf("flag accessed but not defined: %s"u8, name);
        return (new time.Time(nil), err);
    }

    if ((~flag).Value.Type() != "time"u8) {
        var err = fmt.Errorf("trying to get %s value of flag of type %s"u8, "time", (~flag).Value.Type());
        return (new time.Time(nil), err);
    }

    var (val, ok) = (~flag).Value._<ж<timeValue>>(ᐧ);
    if (!ok) {
        return (new time.Time(nil), fmt.Errorf("value %s is not a time"u8, (~flag).Value));
    }

    return ((~val).Time.Value, default!);
}

public static void TimeVar(this ж<FlagSet> Ꮡf, ж<time.Time> Ꮡp, @string name, time.Time value, slice<@string> formats, @string usage) {
    Ꮡf.TimeVarP(Ꮡp, name, "", value, formats, usage);
}

public static void TimeVarP(this ж<FlagSet> Ꮡf, ж<time.Time> Ꮡp, @string name, @string shorthand, time.Time value, slice<@string> formats, @string usage) {
    Ꮡf.VarP(new timeValueжValue(newTimeValue(value, Ꮡp, formats)), name, shorthand, usage);
}

public static void TimeVar(ж<time.Time> Ꮡp, @string name, time.Time value, slice<@string> formats, @string usage) {
    CommandLine.TimeVarP(Ꮡp, name, "", value, formats, usage);
}

public static void TimeVarP(ж<time.Time> Ꮡp, @string name, @string shorthand, time.Time value, slice<@string> formats, @string usage) {
    CommandLine.VarP(new timeValueжValue(newTimeValue(value, Ꮡp, formats)), name, shorthand, usage);
}

public static ж<time.Time> Time(this ж<FlagSet> Ꮡf, @string name, time.Time value, slice<@string> formats, @string usage) {
    return Ꮡf.TimeP(name, "", value, formats, usage);
}

public static ж<time.Time> TimeP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, time.Time value, slice<@string> formats, @string usage) {
    var p = @new<time.Time>();
    Ꮡf.TimeVarP(p, name, shorthand, value, formats, usage);
    return p;
}

public static ж<time.Time> Time(@string name, time.Time value, slice<@string> formats, @string usage) {
    return CommandLine.TimeP(name, "", value, formats, usage);
}

public static ж<time.Time> TimeP(@string name, @string shorthand, time.Time value, slice<@string> formats, @string usage) {
    return CommandLine.TimeP(name, shorthand, value, formats, usage);
}

} // end pflag_package

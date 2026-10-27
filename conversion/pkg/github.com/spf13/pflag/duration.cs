namespace go.github.com.spf13;

using time = time_package;

partial class pflag_package {

partial struct durationValue /*global::go.time_package.Duration*/;

internal static ж<durationValue> newDurationValue(time.Duration val, ж<time.Duration> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<time.Duration, durationValue>();
}

internal static error Set(this ref durationValue d, @string s) {
    var (v, err) = time.ParseDuration(s);
    d = (durationValue)v;
    return err;
}

internal static @string Type(this ref durationValue d) {
    return "duration";
}

internal static @string String(this ж<durationValue> Ꮡd) {
    return (Ꮡd.Reinterpret<durationValue, time.Duration>()).Value.String();
}

internal static (any, error) durationConv(@string sval) {
    var (ᴛ1, ᴛ2) = time.ParseDuration(sval);
    return (ᴛ1, ᴛ2);
}

public static (time.Duration, error) GetDuration(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "duration", durationConv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<time.Duration>(), default!);
}

public static void DurationVar(this ж<FlagSet> Ꮡf, ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    Ꮡf.VarP(new durationValueжValue(newDurationValue(value, Ꮡp)), name, "", usage);
}

public static void DurationVarP(this ж<FlagSet> Ꮡf, ж<time.Duration> Ꮡp, @string name, @string shorthand, time.Duration value, @string usage) {
    Ꮡf.VarP(new durationValueжValue(newDurationValue(value, Ꮡp)), name, shorthand, usage);
}

public static void DurationVar(ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    CommandLine.VarP(new durationValueжValue(newDurationValue(value, Ꮡp)), name, "", usage);
}

public static void DurationVarP(ж<time.Duration> Ꮡp, @string name, @string shorthand, time.Duration value, @string usage) {
    CommandLine.VarP(new durationValueжValue(newDurationValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<time.Duration> Duration(this ж<FlagSet> Ꮡf, @string name, time.Duration value, @string usage) {
    var p = @new<time.Duration>();
    Ꮡf.DurationVarP(p, name, "", value, usage);
    return p;
}

public static ж<time.Duration> DurationP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, time.Duration value, @string usage) {
    var p = @new<time.Duration>();
    Ꮡf.DurationVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<time.Duration> Duration(@string name, time.Duration value, @string usage) {
    return CommandLine.DurationP(name, "", value, usage);
}

public static ж<time.Duration> DurationP(@string name, @string shorthand, time.Duration value, @string usage) {
    return CommandLine.DurationP(name, shorthand, value, usage);
}

} // end pflag_package

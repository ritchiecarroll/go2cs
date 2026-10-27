namespace go.github.com.spf13;

using fmt = fmt_package;
using strings = strings_package;
using time = time_package;

partial class pflag_package {

partial struct durationSliceValue {
    internal ж<slice<time.Duration>> value;
    internal bool changed;
}

internal static ж<durationSliceValue> newDurationSliceValue(slice<time.Duration> val, ж<slice<time.Duration>> Ꮡp) {
    var dsv = @new<durationSliceValue>();
    dsv.Value.value = Ꮡp;
    (~dsv).value.ValueSlot = val;
    return dsv;
}

internal static error Set(this ref durationSliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<time.Duration>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = time.ParseDuration(d);
        if (err != default!) {
            return err;
        }
    }
    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref durationSliceValue s) {
    return "durationSlice";
}

internal static @string String(this ref durationSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%s"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (time.Duration, error) fromString(this ref durationSliceValue s, @string val) {
    return time.ParseDuration(val);
}

internal static @string toString(this ref durationSliceValue s, time.Duration val) {
    return fmt.Sprintf("%s"u8, val);
}

internal static error Append(this ref durationSliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref durationSliceValue s, slice<@string> val) {
    var @out = new slice<time.Duration>(len(val));
    foreach (var (i, d) in val) {
        error err = default!;
        (@out[i], err) = s.fromString(d);
        if (err != default!) {
            return err;
        }
    }
    s.value.ValueSlot = @out;
    return default!;
}

internal static slice<@string> GetSlice(this ref durationSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) durationSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new time.Duration[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<time.Duration>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = time.ParseDuration(d);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (slice<time.Duration>, error) GetDurationSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "durationSlice", durationSliceConv);
    if (err != default!) {
        return (new time.Duration[]{}.slice(), err);
    }
    return (val._<slice<time.Duration>>(), default!);
}

public static void DurationSliceVar(this ж<FlagSet> Ꮡf, ж<slice<time.Duration>> Ꮡp, @string name, slice<time.Duration> value, @string usage) {
    Ꮡf.VarP(new durationSliceValueжValue(newDurationSliceValue(value, Ꮡp)), name, "", usage);
}

public static void DurationSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<time.Duration>> Ꮡp, @string name, @string shorthand, slice<time.Duration> value, @string usage) {
    Ꮡf.VarP(new durationSliceValueжValue(newDurationSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void DurationSliceVar(ж<slice<time.Duration>> Ꮡp, @string name, slice<time.Duration> value, @string usage) {
    CommandLine.VarP(new durationSliceValueжValue(newDurationSliceValue(value, Ꮡp)), name, "", usage);
}

public static void DurationSliceVarP(ж<slice<time.Duration>> Ꮡp, @string name, @string shorthand, slice<time.Duration> value, @string usage) {
    CommandLine.VarP(new durationSliceValueжValue(newDurationSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<time.Duration>> DurationSlice(this ж<FlagSet> Ꮡf, @string name, slice<time.Duration> value, @string usage) {
    ref var p = ref heap<slice<time.Duration>>(out var Ꮡp);
    p = new time.Duration[]{}.slice();
    Ꮡf.DurationSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<time.Duration>> DurationSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<time.Duration> value, @string usage) {
    ref var p = ref heap<slice<time.Duration>>(out var Ꮡp);
    p = new time.Duration[]{}.slice();
    Ꮡf.DurationSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<time.Duration>> DurationSlice(@string name, slice<time.Duration> value, @string usage) {
    return CommandLine.DurationSliceP(name, "", value, usage);
}

public static ж<slice<time.Duration>> DurationSliceP(@string name, @string shorthand, slice<time.Duration> value, @string usage) {
    return CommandLine.DurationSliceP(name, shorthand, value, usage);
}

} // end pflag_package

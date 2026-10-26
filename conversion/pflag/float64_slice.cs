namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct float64SliceValue {
    internal ж<slice<float64>> value;
    internal bool changed;
}

internal static ж<float64SliceValue> newFloat64SliceValue(slice<float64> val, ж<slice<float64>> Ꮡp) {
    var isv = @new<float64SliceValue>();
    isv.Value.value = Ꮡp;
    (~isv).value.ValueSlot = val;
    return isv;
}

internal static error Set(this ref float64SliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<float64>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.ParseFloat(d, 64);
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

internal static @string Type(this ref float64SliceValue s) {
    return "float64Slice";
}

internal static @string String(this ref float64SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%f"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (float64, error) fromString(this ref float64SliceValue s, @string val) {
    return strconv.ParseFloat(val, 64);
}

internal static @string toString(this ref float64SliceValue s, float64 val) {
    return fmt.Sprintf("%f"u8, val);
}

internal static error Append(this ref float64SliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref float64SliceValue s, slice<@string> val) {
    var @out = new slice<float64>(len(val));
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

internal static slice<@string> GetSlice(this ref float64SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) float64SliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new float64[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<float64>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.ParseFloat(d, 64);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (slice<float64>, error) GetFloat64Slice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "float64Slice", float64SliceConv);
    if (err != default!) {
        return (new float64[]{}.slice(), err);
    }
    return (val._<slice<float64>>(), default!);
}

public static void Float64SliceVar(this ж<FlagSet> Ꮡf, ж<slice<float64>> Ꮡp, @string name, slice<float64> value, @string usage) {
    Ꮡf.VarP(new float64SliceValueжValue(newFloat64SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Float64SliceVarP(this ж<FlagSet> Ꮡf, ж<slice<float64>> Ꮡp, @string name, @string shorthand, slice<float64> value, @string usage) {
    Ꮡf.VarP(new float64SliceValueжValue(newFloat64SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void Float64SliceVar(ж<slice<float64>> Ꮡp, @string name, slice<float64> value, @string usage) {
    CommandLine.VarP(new float64SliceValueжValue(newFloat64SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Float64SliceVarP(ж<slice<float64>> Ꮡp, @string name, @string shorthand, slice<float64> value, @string usage) {
    CommandLine.VarP(new float64SliceValueжValue(newFloat64SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<float64>> Float64Slice(this ж<FlagSet> Ꮡf, @string name, slice<float64> value, @string usage) {
    ref var p = ref heap<slice<float64>>(out var Ꮡp);
    p = new float64[]{}.slice();
    Ꮡf.Float64SliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<float64>> Float64SliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<float64> value, @string usage) {
    ref var p = ref heap<slice<float64>>(out var Ꮡp);
    p = new float64[]{}.slice();
    Ꮡf.Float64SliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<float64>> Float64Slice(@string name, slice<float64> value, @string usage) {
    return CommandLine.Float64SliceP(name, "", value, usage);
}

public static ж<slice<float64>> Float64SliceP(@string name, @string shorthand, slice<float64> value, @string usage) {
    return CommandLine.Float64SliceP(name, shorthand, value, usage);
}

} // end pflag_package

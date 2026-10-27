namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct float32SliceValue {
    internal ж<slice<float32>> value;
    internal bool changed;
}

internal static ж<float32SliceValue> newFloat32SliceValue(slice<float32> val, ж<slice<float32>> Ꮡp) {
    var isv = @new<float32SliceValue>();
    isv.Value.value = Ꮡp;
    (~isv).value.ValueSlot = val;
    return isv;
}

internal static error Set(this ref float32SliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<float32>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        float64 temp64 = default!;
        (temp64, err) = strconv.ParseFloat(d, 32);
        if (err != default!) {
            return err;
        }
        @out[i] = (float32)temp64;
    }
    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref float32SliceValue s) {
    return "float32Slice";
}

internal static @string String(this ref float32SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%f"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (float32, error) fromString(this ref float32SliceValue s, @string val) {
    var (t64, err) = strconv.ParseFloat(val, 32);
    if (err != default!) {
        return (0F, err);
    }
    return ((float32)t64, default!);
}

internal static @string toString(this ref float32SliceValue s, float32 val) {
    return fmt.Sprintf("%f"u8, val);
}

internal static error Append(this ref float32SliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref float32SliceValue s, slice<@string> val) {
    var @out = new slice<float32>(len(val));
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

internal static slice<@string> GetSlice(this ref float32SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) float32SliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new float32[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<float32>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        float64 temp64 = default!;
        (temp64, err) = strconv.ParseFloat(d, 32);
        if (err != default!) {
            return (default!, err);
        }
        @out[i] = (float32)temp64;
    }
    return (@out, default!);
}

public static (slice<float32>, error) GetFloat32Slice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "float32Slice", float32SliceConv);
    if (err != default!) {
        return (new float32[]{}.slice(), err);
    }
    return (val._<slice<float32>>(), default!);
}

public static void Float32SliceVar(this ж<FlagSet> Ꮡf, ж<slice<float32>> Ꮡp, @string name, slice<float32> value, @string usage) {
    Ꮡf.VarP(new float32SliceValueжValue(newFloat32SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Float32SliceVarP(this ж<FlagSet> Ꮡf, ж<slice<float32>> Ꮡp, @string name, @string shorthand, slice<float32> value, @string usage) {
    Ꮡf.VarP(new float32SliceValueжValue(newFloat32SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void Float32SliceVar(ж<slice<float32>> Ꮡp, @string name, slice<float32> value, @string usage) {
    CommandLine.VarP(new float32SliceValueжValue(newFloat32SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Float32SliceVarP(ж<slice<float32>> Ꮡp, @string name, @string shorthand, slice<float32> value, @string usage) {
    CommandLine.VarP(new float32SliceValueжValue(newFloat32SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<float32>> Float32Slice(this ж<FlagSet> Ꮡf, @string name, slice<float32> value, @string usage) {
    ref var p = ref heap<slice<float32>>(out var Ꮡp);
    p = new float32[]{}.slice();
    Ꮡf.Float32SliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<float32>> Float32SliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<float32> value, @string usage) {
    ref var p = ref heap<slice<float32>>(out var Ꮡp);
    p = new float32[]{}.slice();
    Ꮡf.Float32SliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<float32>> Float32Slice(@string name, slice<float32> value, @string usage) {
    return CommandLine.Float32SliceP(name, "", value, usage);
}

public static ж<slice<float32>> Float32SliceP(@string name, @string shorthand, slice<float32> value, @string usage) {
    return CommandLine.Float32SliceP(name, shorthand, value, usage);
}

} // end pflag_package

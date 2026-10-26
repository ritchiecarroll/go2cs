namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct int64SliceValue {
    internal ж<slice<int64>> value;
    internal bool changed;
}

internal static ж<int64SliceValue> newInt64SliceValue(slice<int64> val, ж<slice<int64>> Ꮡp) {
    var isv = @new<int64SliceValue>();
    isv.Value.value = Ꮡp;
    (~isv).value.ValueSlot = val;
    return isv;
}

internal static error Set(this ref int64SliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<int64>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.ParseInt(d, 0, 64);
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

internal static @string Type(this ref int64SliceValue s) {
    return "int64Slice";
}

internal static @string String(this ref int64SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%d"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (int64, error) fromString(this ref int64SliceValue s, @string val) {
    return strconv.ParseInt(val, 0, 64);
}

internal static @string toString(this ref int64SliceValue s, int64 val) {
    return fmt.Sprintf("%d"u8, val);
}

internal static error Append(this ref int64SliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref int64SliceValue s, slice<@string> val) {
    var @out = new slice<int64>(len(val));
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

internal static slice<@string> GetSlice(this ref int64SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) int64SliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new int64[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<int64>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.ParseInt(d, 0, 64);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (slice<int64>, error) GetInt64Slice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int64Slice", int64SliceConv);
    if (err != default!) {
        return (new int64[]{}.slice(), err);
    }
    return (val._<slice<int64>>(), default!);
}

public static void Int64SliceVar(this ж<FlagSet> Ꮡf, ж<slice<int64>> Ꮡp, @string name, slice<int64> value, @string usage) {
    Ꮡf.VarP(new int64SliceValueжValue(newInt64SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Int64SliceVarP(this ж<FlagSet> Ꮡf, ж<slice<int64>> Ꮡp, @string name, @string shorthand, slice<int64> value, @string usage) {
    Ꮡf.VarP(new int64SliceValueжValue(newInt64SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void Int64SliceVar(ж<slice<int64>> Ꮡp, @string name, slice<int64> value, @string usage) {
    CommandLine.VarP(new int64SliceValueжValue(newInt64SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Int64SliceVarP(ж<slice<int64>> Ꮡp, @string name, @string shorthand, slice<int64> value, @string usage) {
    CommandLine.VarP(new int64SliceValueжValue(newInt64SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<int64>> Int64Slice(this ж<FlagSet> Ꮡf, @string name, slice<int64> value, @string usage) {
    ref var p = ref heap<slice<int64>>(out var Ꮡp);
    p = new int64[]{}.slice();
    Ꮡf.Int64SliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<int64>> Int64SliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<int64> value, @string usage) {
    ref var p = ref heap<slice<int64>>(out var Ꮡp);
    p = new int64[]{}.slice();
    Ꮡf.Int64SliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<int64>> Int64Slice(@string name, slice<int64> value, @string usage) {
    return CommandLine.Int64SliceP(name, "", value, usage);
}

public static ж<slice<int64>> Int64SliceP(@string name, @string shorthand, slice<int64> value, @string usage) {
    return CommandLine.Int64SliceP(name, shorthand, value, usage);
}

} // end pflag_package

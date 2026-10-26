namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct int32SliceValue {
    internal ж<slice<int32>> value;
    internal bool changed;
}

internal static ж<int32SliceValue> newInt32SliceValue(slice<int32> val, ж<slice<int32>> Ꮡp) {
    var isv = @new<int32SliceValue>();
    isv.Value.value = Ꮡp;
    (~isv).value.ValueSlot = val;
    return isv;
}

internal static error Set(this ref int32SliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<int32>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        int64 temp64 = default!;
        (temp64, err) = strconv.ParseInt(d, 0, 32);
        if (err != default!) {
            return err;
        }
        @out[i] = (int32)temp64;
    }
    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref int32SliceValue s) {
    return "int32Slice";
}

internal static @string String(this ref int32SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%d"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (int32, error) fromString(this ref int32SliceValue s, @string val) {
    var (t64, err) = strconv.ParseInt(val, 0, 32);
    if (err != default!) {
        return (0, err);
    }
    return ((int32)t64, default!);
}

internal static @string toString(this ref int32SliceValue s, int32 val) {
    return fmt.Sprintf("%d"u8, val);
}

internal static error Append(this ref int32SliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref int32SliceValue s, slice<@string> val) {
    var @out = new slice<int32>(len(val));
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

internal static slice<@string> GetSlice(this ref int32SliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) int32SliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new int32[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<int32>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        int64 temp64 = default!;
        (temp64, err) = strconv.ParseInt(d, 0, 32);
        if (err != default!) {
            return (default!, err);
        }
        @out[i] = (int32)temp64;
    }
    return (@out, default!);
}

public static (slice<int32>, error) GetInt32Slice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int32Slice", int32SliceConv);
    if (err != default!) {
        return (new int32[]{}.slice(), err);
    }
    return (val._<slice<int32>>(), default!);
}

public static void Int32SliceVar(this ж<FlagSet> Ꮡf, ж<slice<int32>> Ꮡp, @string name, slice<int32> value, @string usage) {
    Ꮡf.VarP(new int32SliceValueжValue(newInt32SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Int32SliceVarP(this ж<FlagSet> Ꮡf, ж<slice<int32>> Ꮡp, @string name, @string shorthand, slice<int32> value, @string usage) {
    Ꮡf.VarP(new int32SliceValueжValue(newInt32SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void Int32SliceVar(ж<slice<int32>> Ꮡp, @string name, slice<int32> value, @string usage) {
    CommandLine.VarP(new int32SliceValueжValue(newInt32SliceValue(value, Ꮡp)), name, "", usage);
}

public static void Int32SliceVarP(ж<slice<int32>> Ꮡp, @string name, @string shorthand, slice<int32> value, @string usage) {
    CommandLine.VarP(new int32SliceValueжValue(newInt32SliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<int32>> Int32Slice(this ж<FlagSet> Ꮡf, @string name, slice<int32> value, @string usage) {
    ref var p = ref heap<slice<int32>>(out var Ꮡp);
    p = new int32[]{}.slice();
    Ꮡf.Int32SliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<int32>> Int32SliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<int32> value, @string usage) {
    ref var p = ref heap<slice<int32>>(out var Ꮡp);
    p = new int32[]{}.slice();
    Ꮡf.Int32SliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<int32>> Int32Slice(@string name, slice<int32> value, @string usage) {
    return CommandLine.Int32SliceP(name, "", value, usage);
}

public static ж<slice<int32>> Int32SliceP(@string name, @string shorthand, slice<int32> value, @string usage) {
    return CommandLine.Int32SliceP(name, shorthand, value, usage);
}

} // end pflag_package

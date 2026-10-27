namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct uintSliceValue {
    internal ж<slice<nuint>> value;
    internal bool changed;
}

internal static ж<uintSliceValue> newUintSliceValue(slice<nuint> val, ж<slice<nuint>> Ꮡp) {
    var uisv = @new<uintSliceValue>();
    uisv.Value.value = Ꮡp;
    (~uisv).value.ValueSlot = val;
    return uisv;
}

internal static error Set(this ref uintSliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<nuint>(len(ss));
    foreach (var (i, d) in ss) {
        var (u, err) = strconv.ParseUint(d, 10, 0);
        if (err != default!) {
            return err;
        }
        @out[i] = (nuint)u;
    }
    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref uintSliceValue s) {
    return "uintSlice";
}

internal static @string String(this ref uintSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%d"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static (nuint, error) fromString(this ref uintSliceValue s, @string val) {
    var (t, err) = strconv.ParseUint(val, 10, 0);
    if (err != default!) {
        return (0, err);
    }
    return ((nuint)t, default!);
}

internal static @string toString(this ref uintSliceValue s, nuint val) {
    return fmt.Sprintf("%d"u8, val);
}

internal static error Append(this ref uintSliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref uintSliceValue s, slice<@string> val) {
    var @out = new slice<nuint>(len(val));
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

internal static slice<@string> GetSlice(this ref uintSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) uintSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new nuint[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<nuint>(len(ss));
    foreach (var (i, d) in ss) {
        var (u, err) = strconv.ParseUint(d, 10, 0);
        if (err != default!) {
            return (default!, err);
        }
        @out[i] = (nuint)u;
    }
    return (@out, default!);
}

public static (slice<nuint>, error) GetUintSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "uintSlice", uintSliceConv);
    if (err != default!) {
        return (new nuint[]{}.slice(), err);
    }
    return (val._<slice<nuint>>(), default!);
}

public static void UintSliceVar(this ж<FlagSet> Ꮡf, ж<slice<nuint>> Ꮡp, @string name, slice<nuint> value, @string usage) {
    Ꮡf.VarP(new uintSliceValueжValue(newUintSliceValue(value, Ꮡp)), name, "", usage);
}

public static void UintSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<nuint>> Ꮡp, @string name, @string shorthand, slice<nuint> value, @string usage) {
    Ꮡf.VarP(new uintSliceValueжValue(newUintSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void UintSliceVar(ж<slice<nuint>> Ꮡp, @string name, slice<nuint> value, @string usage) {
    CommandLine.VarP(new uintSliceValueжValue(newUintSliceValue(value, Ꮡp)), name, "", usage);
}

public static void UintSliceVarP(ж<slice<nuint>> Ꮡp, @string name, @string shorthand, slice<nuint> value, @string usage) {
    CommandLine.VarP(new uintSliceValueжValue(newUintSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<nuint>> UintSlice(this ж<FlagSet> Ꮡf, @string name, slice<nuint> value, @string usage) {
    ref var p = ref heap<slice<nuint>>(out var Ꮡp);
    p = new nuint[]{}.slice();
    Ꮡf.UintSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<nuint>> UintSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<nuint> value, @string usage) {
    ref var p = ref heap<slice<nuint>>(out var Ꮡp);
    p = new nuint[]{}.slice();
    Ꮡf.UintSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<nuint>> UintSlice(@string name, slice<nuint> value, @string usage) {
    return CommandLine.UintSliceP(name, "", value, usage);
}

public static ж<slice<nuint>> UintSliceP(@string name, @string shorthand, slice<nuint> value, @string usage) {
    return CommandLine.UintSliceP(name, shorthand, value, usage);
}

} // end pflag_package

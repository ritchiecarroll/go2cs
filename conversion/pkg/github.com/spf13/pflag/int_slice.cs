namespace go.github.com.spf13;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct intSliceValue {
    internal ж<slice<nint>> value;
    internal bool changed;
}

internal static ж<intSliceValue> newIntSliceValue(slice<nint> val, ж<slice<nint>> Ꮡp) {
    var isv = @new<intSliceValue>();
    isv.Value.value = Ꮡp;
    (~isv).value.ValueSlot = val;
    return isv;
}

internal static error Set(this ref intSliceValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new slice<nint>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.Atoi(d);
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

internal static @string Type(this ref intSliceValue s) {
    return "intSlice";
}

internal static @string String(this ref intSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = fmt.Sprintf("%d"u8, d);
    }
    return "["u8 + strings.Join(@out, ",") + "]"u8;
}

internal static error Append(this ref intSliceValue s, @string val) {
    var (i, err) = strconv.Atoi(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref intSliceValue s, slice<@string> val) {
    var @out = new slice<nint>(len(val));
    foreach (var (i, d) in val) {
        error err = default!;
        (@out[i], err) = strconv.Atoi(d);
        if (err != default!) {
            return err;
        }
    }
    s.value.ValueSlot = @out;
    return default!;
}

internal static slice<@string> GetSlice(this ref intSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = strconv.Itoa(d);
    }
    return @out;
}

internal static (any, error) intSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new nint[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<nint>(len(ss));
    foreach (var (i, d) in ss) {
        error err = default!;
        (@out[i], err) = strconv.Atoi(d);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (slice<nint>, error) GetIntSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "intSlice", intSliceConv);
    if (err != default!) {
        return (new nint[]{}.slice(), err);
    }
    return (val._<slice<nint>>(), default!);
}

public static void IntSliceVar(this ж<FlagSet> Ꮡf, ж<slice<nint>> Ꮡp, @string name, slice<nint> value, @string usage) {
    Ꮡf.VarP(new intSliceValueжValue(newIntSliceValue(value, Ꮡp)), name, "", usage);
}

public static void IntSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<nint>> Ꮡp, @string name, @string shorthand, slice<nint> value, @string usage) {
    Ꮡf.VarP(new intSliceValueжValue(newIntSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void IntSliceVar(ж<slice<nint>> Ꮡp, @string name, slice<nint> value, @string usage) {
    CommandLine.VarP(new intSliceValueжValue(newIntSliceValue(value, Ꮡp)), name, "", usage);
}

public static void IntSliceVarP(ж<slice<nint>> Ꮡp, @string name, @string shorthand, slice<nint> value, @string usage) {
    CommandLine.VarP(new intSliceValueжValue(newIntSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<nint>> IntSlice(this ж<FlagSet> Ꮡf, @string name, slice<nint> value, @string usage) {
    ref var p = ref heap<slice<nint>>(out var Ꮡp);
    p = new nint[]{}.slice();
    Ꮡf.IntSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<nint>> IntSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<nint> value, @string usage) {
    ref var p = ref heap<slice<nint>>(out var Ꮡp);
    p = new nint[]{}.slice();
    Ꮡf.IntSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<nint>> IntSlice(@string name, slice<nint> value, @string usage) {
    return CommandLine.IntSliceP(name, "", value, usage);
}

public static ж<slice<nint>> IntSliceP(@string name, @string shorthand, slice<nint> value, @string usage) {
    return CommandLine.IntSliceP(name, shorthand, value, usage);
}

} // end pflag_package

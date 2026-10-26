namespace go.github.com.spf13;

using bytes = bytes_package;
using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct stringToIntValue {
    internal ж<map<@string, nint>> value;
    internal bool changed;
}

internal static ж<stringToIntValue> newStringToIntValue(map<@string, nint> val, ж<map<@string, nint>> Ꮡp) {
    var ssv = @new<stringToIntValue>();
    ssv.Value.value = Ꮡp;
    (~ssv).value.ValueSlot = val;
    return ssv;
}

internal static error Set(this ref stringToIntValue s, @string val) {
    var ss = strings.Split(val, ",");
    var @out = new map<@string, nint>(len(ss));
    foreach (var (_, pair) in ss) {
        var kv = strings.SplitN(pair, "=", 2);
        if (len(kv) != 2) {
            return fmt.Errorf("%s must be formatted as key=value"u8, pair);
        }
        error err = default!;
        (@out[kv[0]], err) = strconv.Atoi(kv[1]);
        if (err != default!) {
            return err;
        }
    }
    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        foreach (var (k, v) in @out) {
            (s.value.ValueSlot)[k] = v;
        }
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref stringToIntValue s) {
    return "stringToInt";
}

internal static @string String(this ref stringToIntValue s) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    nint i = 0;
    foreach (var (k, v) in s.value.ValueSlot) {
        if (i > 0) {
            buf.WriteRune((rune)',');
        }
        buf.WriteString(k);
        buf.WriteRune((rune)'=');
        buf.WriteString(strconv.Itoa(v));
        i++;
    }
    return "["u8 + Ꮡbuf.String() + "]"u8;
}

internal static (any, error) stringToIntConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new map<@string, nint>{}, default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new map<@string, nint>(len(ss));
    foreach (var (_, pair) in ss) {
        var kv = strings.SplitN(pair, "=", 2);
        if (len(kv) != 2) {
            return (default!, fmt.Errorf("%s must be formatted as key=value"u8, pair));
        }
        error err = default!;
        (@out[kv[0]], err) = strconv.Atoi(kv[1]);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (map<@string, nint>, error) GetStringToInt(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "stringToInt", stringToIntConv);
    if (err != default!) {
        return (new map<@string, nint>{}, err);
    }
    return (val._<map<@string, nint>>(), default!);
}

public static void StringToIntVar(this ж<FlagSet> Ꮡf, ж<map<@string, nint>> Ꮡp, @string name, map<@string, nint> value, @string usage) {
    Ꮡf.VarP(new stringToIntValueжValue(newStringToIntValue(value, Ꮡp)), name, "", usage);
}

public static void StringToIntVarP(this ж<FlagSet> Ꮡf, ж<map<@string, nint>> Ꮡp, @string name, @string shorthand, map<@string, nint> value, @string usage) {
    Ꮡf.VarP(new stringToIntValueжValue(newStringToIntValue(value, Ꮡp)), name, shorthand, usage);
}

public static void StringToIntVar(ж<map<@string, nint>> Ꮡp, @string name, map<@string, nint> value, @string usage) {
    CommandLine.VarP(new stringToIntValueжValue(newStringToIntValue(value, Ꮡp)), name, "", usage);
}

public static void StringToIntVarP(ж<map<@string, nint>> Ꮡp, @string name, @string shorthand, map<@string, nint> value, @string usage) {
    CommandLine.VarP(new stringToIntValueжValue(newStringToIntValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<map<@string, nint>> StringToInt(this ж<FlagSet> Ꮡf, @string name, map<@string, nint> value, @string usage) {
    ref var p = ref heap<map<@string, nint>>(out var Ꮡp);
    p = new map<@string, nint>{};
    Ꮡf.StringToIntVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<map<@string, nint>> StringToIntP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, map<@string, nint> value, @string usage) {
    ref var p = ref heap<map<@string, nint>>(out var Ꮡp);
    p = new map<@string, nint>{};
    Ꮡf.StringToIntVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<map<@string, nint>> StringToInt(@string name, map<@string, nint> value, @string usage) {
    return CommandLine.StringToIntP(name, "", value, usage);
}

public static ж<map<@string, nint>> StringToIntP(@string name, @string shorthand, map<@string, nint> value, @string usage) {
    return CommandLine.StringToIntP(name, shorthand, value, usage);
}

} // end pflag_package

namespace go.github.com.spf13;

using bytes = bytes_package;
using csv = go.encoding.csv_package;
using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using go.encoding;
using io = io_package;

partial class pflag_package {

partial struct stringToStringValue {
    internal ж<map<@string, @string>> value;
    internal bool changed;
}

internal static ж<stringToStringValue> newStringToStringValue(map<@string, @string> val, ж<map<@string, @string>> Ꮡp) {
    var ssv = @new<stringToStringValue>();
    ssv.Value.value = Ꮡp;
    (~ssv).value.ValueSlot = val;
    return ssv;
}

internal static error Set(this ref stringToStringValue s, @string val) {
    var ssᴷ = new stackAppend<@string>();

    slice<@string> ss = default!;
    nint n = strings.Count(val, "=");
    switch (n) {
    case 0: {
        return fmt.Errorf("%s must be formatted as key=value"u8, val);
    }
    case 1: {
        ss = append(ssᴷ, ss, strings.Trim(val, @""""));
        break;
    }
    default: {
        var r = csv.NewReader(new strings_ReaderжReader(strings.NewReader(val)));
        error err = default!;
        (ss, err) = r.Read();
        if (err != default!) {
            return err;
        }
        break;
    }}

    var @out = new map<@string, @string>(len(ss));
    foreach (var (_, pair) in ss) {
        var kv = strings.SplitN(pair, "=", 2);
        if (len(kv) != 2) {
            return fmt.Errorf("%s must be formatted as key=value"u8, pair);
        }
        @out[kv[0]] = kv[1];
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

internal static @string Type(this ref stringToStringValue s) {
    return "stringToString";
}

internal static @string String(this ref stringToStringValue s) {
    var keysᴷ = new stackAppend<@string>();
    var recordsᴷ = new stackAppend<@string>();

    var keys = new slice<@string>(0, len(s.value.ValueSlot));
    foreach (var (k, _) in s.value.ValueSlot) {
        keys = append(keysᴷ, keys, k);
    }
    sort.Strings(keys);

    var records = new slice<@string>(0, (len(s.value.ValueSlot) >> 1));
    foreach (var (_, k) in keys) {
        @string v = (s.value.ValueSlot)[k];
        records = append(recordsᴷ, records, k + "="u8 + v);
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = csv.NewWriter(new bytes_BufferжWriter(Ꮡbuf));
    {
        var err = w.Write(records); if (err != default!) {
            throw panic(err);
        }
    }
    w.Flush();
    return "["u8 + strings.TrimSpace(Ꮡbuf.String()) + "]"u8;
}

internal static (any, error) stringToStringConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new map<@string, @string>{}, default!);
    }
    var r = csv.NewReader(new strings_ReaderжReader(strings.NewReader(val)));
    var (ss, err) = r.Read();
    if (err != default!) {
        return (default!, err);
    }
    var @out = new map<@string, @string>(len(ss));
    foreach (var (_, pair) in ss) {
        var kv = strings.SplitN(pair, "=", 2);
        if (len(kv) != 2) {
            return (default!, fmt.Errorf("%s must be formatted as key=value"u8, pair));
        }
        @out[kv[0]] = kv[1];
    }
    return (@out, default!);
}

public static (map<@string, @string>, error) GetStringToString(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "stringToString", stringToStringConv);
    if (err != default!) {
        return (new map<@string, @string>{}, err);
    }
    return (val._<map<@string, @string>>(), default!);
}

public static void StringToStringVar(this ж<FlagSet> Ꮡf, ж<map<@string, @string>> Ꮡp, @string name, map<@string, @string> value, @string usage) {
    Ꮡf.VarP(new stringToStringValueжValue(newStringToStringValue(value, Ꮡp)), name, "", usage);
}

public static void StringToStringVarP(this ж<FlagSet> Ꮡf, ж<map<@string, @string>> Ꮡp, @string name, @string shorthand, map<@string, @string> value, @string usage) {
    Ꮡf.VarP(new stringToStringValueжValue(newStringToStringValue(value, Ꮡp)), name, shorthand, usage);
}

public static void StringToStringVar(ж<map<@string, @string>> Ꮡp, @string name, map<@string, @string> value, @string usage) {
    CommandLine.VarP(new stringToStringValueжValue(newStringToStringValue(value, Ꮡp)), name, "", usage);
}

public static void StringToStringVarP(ж<map<@string, @string>> Ꮡp, @string name, @string shorthand, map<@string, @string> value, @string usage) {
    CommandLine.VarP(new stringToStringValueжValue(newStringToStringValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<map<@string, @string>> StringToString(this ж<FlagSet> Ꮡf, @string name, map<@string, @string> value, @string usage) {
    ref var p = ref heap<map<@string, @string>>(out var Ꮡp);
    p = new map<@string, @string>{};
    Ꮡf.StringToStringVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<map<@string, @string>> StringToStringP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, map<@string, @string> value, @string usage) {
    ref var p = ref heap<map<@string, @string>>(out var Ꮡp);
    p = new map<@string, @string>{};
    Ꮡf.StringToStringVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<map<@string, @string>> StringToString(@string name, map<@string, @string> value, @string usage) {
    return CommandLine.StringToStringP(name, "", value, usage);
}

public static ж<map<@string, @string>> StringToStringP(@string name, @string shorthand, map<@string, @string> value, @string usage) {
    return CommandLine.StringToStringP(name, shorthand, value, usage);
}

} // end pflag_package

namespace go.github.com.spf13;

partial class pflag_package {

partial struct stringArrayValue {
    internal ж<slice<@string>> value;
    internal bool changed;
}

internal static ж<stringArrayValue> newStringArrayValue(slice<@string> val, ж<slice<@string>> Ꮡp) {
    var ssv = @new<stringArrayValue>();
    ssv.Value.value = Ꮡp;
    (~ssv).value.ValueSlot = val;
    return ssv;
}

internal static error Set(this ref stringArrayValue s, @string val) {
    if (!s.changed){
        s.value.ValueSlot = new @string[]{val}.slice();
        s.changed = true;
    } else {
        s.value.ValueSlot = append(s.value.ValueSlot, val);
    }
    return default!;
}

internal static error Append(this ref stringArrayValue s, @string val) {
    s.value.ValueSlot = append(s.value.ValueSlot, val);
    return default!;
}

internal static error Replace(this ref stringArrayValue s, slice<@string> val) {
    var @out = new slice<@string>(len(val));
    foreach (var (i, d) in val) {
        @out[i] = d;
    }
    s.value.ValueSlot = @out;
    return default!;
}

internal static slice<@string> GetSlice(this ref stringArrayValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = d;
    }
    return @out;
}

internal static @string Type(this ref stringArrayValue s) {
    return "stringArray";
}

internal static @string String(this ref stringArrayValue s) {
    var (str, _) = writeAsCSV(s.value.ValueSlot);
    return "["u8 + str + "]"u8;
}

internal static (any, error) stringArrayConv(@string sval) {
    sval = sval.slice(1, len(sval) - 1);
    if (len(sval) == 0) {
        return (new @string[]{}.slice(), default!);
    }
    var (ᴛ1, ᴛ2) = readAsCSV(sval);
    return (ᴛ1, ᴛ2);
}

public static (slice<@string>, error) GetStringArray(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "stringArray", stringArrayConv);
    if (err != default!) {
        return (new @string[]{}.slice(), err);
    }
    return (val._<slice<@string>>(), default!);
}

public static void StringArrayVar(this ж<FlagSet> Ꮡf, ж<slice<@string>> Ꮡp, @string name, slice<@string> value, @string usage) {
    Ꮡf.VarP(new stringArrayValueжValue(newStringArrayValue(value, Ꮡp)), name, "", usage);
}

public static void StringArrayVarP(this ж<FlagSet> Ꮡf, ж<slice<@string>> Ꮡp, @string name, @string shorthand, slice<@string> value, @string usage) {
    Ꮡf.VarP(new stringArrayValueжValue(newStringArrayValue(value, Ꮡp)), name, shorthand, usage);
}

public static void StringArrayVar(ж<slice<@string>> Ꮡp, @string name, slice<@string> value, @string usage) {
    CommandLine.VarP(new stringArrayValueжValue(newStringArrayValue(value, Ꮡp)), name, "", usage);
}

public static void StringArrayVarP(ж<slice<@string>> Ꮡp, @string name, @string shorthand, slice<@string> value, @string usage) {
    CommandLine.VarP(new stringArrayValueжValue(newStringArrayValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<@string>> StringArray(this ж<FlagSet> Ꮡf, @string name, slice<@string> value, @string usage) {
    ref var p = ref heap<slice<@string>>(out var Ꮡp);
    p = new @string[]{}.slice();
    Ꮡf.StringArrayVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<@string>> StringArrayP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<@string> value, @string usage) {
    ref var p = ref heap<slice<@string>>(out var Ꮡp);
    p = new @string[]{}.slice();
    Ꮡf.StringArrayVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<@string>> StringArray(@string name, slice<@string> value, @string usage) {
    return CommandLine.StringArrayP(name, "", value, usage);
}

public static ж<slice<@string>> StringArrayP(@string name, @string shorthand, slice<@string> value, @string usage) {
    return CommandLine.StringArrayP(name, shorthand, value, usage);
}

} // end pflag_package

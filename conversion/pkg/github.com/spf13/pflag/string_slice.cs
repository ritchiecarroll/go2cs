namespace go.github.com.spf13;

using bytes = bytes_package;
using csv = go.encoding.csv_package;
using strings = strings_package;
using go.encoding;
using io = io_package;

partial class pflag_package {

partial struct stringSliceValue {
    internal ж<slice<@string>> value;
    internal bool changed;
}

internal static ж<stringSliceValue> newStringSliceValue(slice<@string> val, ж<slice<@string>> Ꮡp) {
    var ssv = @new<stringSliceValue>();
    ssv.Value.value = Ꮡp;
    (~ssv).value.ValueSlot = val;
    return ssv;
}

internal static (slice<@string>, error) readAsCSV(@string val) {
    if (val == ""u8) {
        return (new @string[]{}.slice(), default!);
    }
    var stringReader = strings.NewReader(val);
    var csvReader = csv.NewReader(new strings_ReaderжReader(stringReader));
    return csvReader.Read();
}

internal static (@string, error) writeAsCSV(slice<@string> vals) {
    var b = Ꮡ(new bytes.Buffer(nil));
    var w = csv.NewWriter(new bytes_BufferжWriter(b));
    var err = w.Write(vals);
    if (err != default!) {
        return ("", err);
    }
    w.Flush();
    return (strings.TrimSuffix(b.String(), "\n"), default!);
}

internal static error Set(this ref stringSliceValue s, @string val) {
    var (v, err) = readAsCSV(val);
    if (err != default!) {
        return err;
    }
    if (!s.changed){
        s.value.ValueSlot = v;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, v);
    }
    s.changed = true;
    return default!;
}

internal static @string Type(this ref stringSliceValue s) {
    return "stringSlice";
}

internal static @string String(this ref stringSliceValue s) {
    var (str, _) = writeAsCSV(s.value.ValueSlot);
    return "["u8 + str + "]"u8;
}

internal static error Append(this ref stringSliceValue s, @string val) {
    s.value.ValueSlot = append(s.value.ValueSlot, val);
    return default!;
}

internal static error Replace(this ref stringSliceValue s, slice<@string> val) {
    s.value.ValueSlot = val;
    return default!;
}

internal static slice<@string> GetSlice(this ref stringSliceValue s) {
    return s.value.ValueSlot;
}

internal static (any, error) stringSliceConv(@string sval) {
    sval = sval.slice(1, len(sval) - 1);
    if (len(sval) == 0) {
        return (new @string[]{}.slice(), default!);
    }
    var (ᴛ1, ᴛ2) = readAsCSV(sval);
    return (ᴛ1, ᴛ2);
}

public static (slice<@string>, error) GetStringSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "stringSlice", stringSliceConv);
    if (err != default!) {
        return (new @string[]{}.slice(), err);
    }
    return (val._<slice<@string>>(), default!);
}

public static void StringSliceVar(this ж<FlagSet> Ꮡf, ж<slice<@string>> Ꮡp, @string name, slice<@string> value, @string usage) {
    Ꮡf.VarP(new stringSliceValueжValue(newStringSliceValue(value, Ꮡp)), name, "", usage);
}

public static void StringSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<@string>> Ꮡp, @string name, @string shorthand, slice<@string> value, @string usage) {
    Ꮡf.VarP(new stringSliceValueжValue(newStringSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void StringSliceVar(ж<slice<@string>> Ꮡp, @string name, slice<@string> value, @string usage) {
    CommandLine.VarP(new stringSliceValueжValue(newStringSliceValue(value, Ꮡp)), name, "", usage);
}

public static void StringSliceVarP(ж<slice<@string>> Ꮡp, @string name, @string shorthand, slice<@string> value, @string usage) {
    CommandLine.VarP(new stringSliceValueжValue(newStringSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<@string>> StringSlice(this ж<FlagSet> Ꮡf, @string name, slice<@string> value, @string usage) {
    ref var p = ref heap<slice<@string>>(out var Ꮡp);
    p = new @string[]{}.slice();
    Ꮡf.StringSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<@string>> StringSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<@string> value, @string usage) {
    ref var p = ref heap<slice<@string>>(out var Ꮡp);
    p = new @string[]{}.slice();
    Ꮡf.StringSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<@string>> StringSlice(@string name, slice<@string> value, @string usage) {
    return CommandLine.StringSliceP(name, "", value, usage);
}

public static ж<slice<@string>> StringSliceP(@string name, @string shorthand, slice<@string> value, @string usage) {
    return CommandLine.StringSliceP(name, shorthand, value, usage);
}

} // end pflag_package

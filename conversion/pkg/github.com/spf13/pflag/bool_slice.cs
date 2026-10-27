namespace go.github.com.spf13;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct boolSliceValue {
    internal ж<slice<bool>> value;
    internal bool changed;
}

internal static ж<boolSliceValue> newBoolSliceValue(slice<bool> val, ж<slice<bool>> Ꮡp) {
    var bsv = @new<boolSliceValue>();
    bsv.Value.value = Ꮡp;
    (~bsv).value.ValueSlot = val;
    return bsv;
}

internal static error Set(this ref boolSliceValue s, @string val) {
    var rmQuote = strings.NewReplacer(@"""", "", @"'", "", "`", "");

    var (boolStrSlice, err) = readAsCSV(rmQuote.Replace(val));
    if (err != default! && !AreEqual(err, io.EOF)) {
        return err;
    }

    var @out = new slice<bool>(0, len(boolStrSlice));
    foreach (var (_, boolStr) in boolStrSlice) {
        var (b, errΔ1) = strconv.ParseBool(strings.TrimSpace(boolStr));
        if (errΔ1 != default!) {
            return errΔ1;
        }
        @out = append(@out, b);
    }

    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }

    s.changed = true;

    return default!;
}

internal static @string Type(this ref boolSliceValue s) {
    return "boolSlice";
}

internal static @string String(this ref boolSliceValue s) {
    var boolStrSlice = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, b) in s.value.ValueSlot) {
        boolStrSlice[i] = strconv.FormatBool(b);
    }

    var (@out, _) = writeAsCSV(boolStrSlice);

    return "["u8 + @out + "]"u8;
}

internal static (bool, error) fromString(this ref boolSliceValue s, @string val) {
    return strconv.ParseBool(val);
}

internal static @string toString(this ref boolSliceValue s, bool val) {
    return strconv.FormatBool(val);
}

internal static error Append(this ref boolSliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref boolSliceValue s, slice<@string> val) {
    var @out = new slice<bool>(len(val));
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

internal static slice<@string> GetSlice(this ref boolSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) boolSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new bool[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<bool>(len(ss));
    foreach (var (i, t) in ss) {
        error err = default!;
        (@out[i], err) = strconv.ParseBool(t);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (@out, default!);
}

public static (slice<bool>, error) GetBoolSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "boolSlice", boolSliceConv);
    if (err != default!) {
        return (new bool[]{}.slice(), err);
    }
    return (val._<slice<bool>>(), default!);
}

public static void BoolSliceVar(this ж<FlagSet> Ꮡf, ж<slice<bool>> Ꮡp, @string name, slice<bool> value, @string usage) {
    Ꮡf.VarP(new boolSliceValueжValue(newBoolSliceValue(value, Ꮡp)), name, "", usage);
}

public static void BoolSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<bool>> Ꮡp, @string name, @string shorthand, slice<bool> value, @string usage) {
    Ꮡf.VarP(new boolSliceValueжValue(newBoolSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void BoolSliceVar(ж<slice<bool>> Ꮡp, @string name, slice<bool> value, @string usage) {
    CommandLine.VarP(new boolSliceValueжValue(newBoolSliceValue(value, Ꮡp)), name, "", usage);
}

public static void BoolSliceVarP(ж<slice<bool>> Ꮡp, @string name, @string shorthand, slice<bool> value, @string usage) {
    CommandLine.VarP(new boolSliceValueжValue(newBoolSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<bool>> BoolSlice(this ж<FlagSet> Ꮡf, @string name, slice<bool> value, @string usage) {
    ref var p = ref heap<slice<bool>>(out var Ꮡp);
    p = new bool[]{}.slice();
    Ꮡf.BoolSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<bool>> BoolSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<bool> value, @string usage) {
    ref var p = ref heap<slice<bool>>(out var Ꮡp);
    p = new bool[]{}.slice();
    Ꮡf.BoolSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<bool>> BoolSlice(@string name, slice<bool> value, @string usage) {
    return CommandLine.BoolSliceP(name, "", value, usage);
}

public static ж<slice<bool>> BoolSliceP(@string name, @string shorthand, slice<bool> value, @string usage) {
    return CommandLine.BoolSliceP(name, shorthand, value, usage);
}

} // end pflag_package

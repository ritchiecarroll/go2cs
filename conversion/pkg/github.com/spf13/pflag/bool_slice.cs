namespace go.github.com.spf13;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;

partial class pflag_package {

partial struct boolSliceValue {
    internal ж<slice<bool>> value;
    internal bool changed;
}

internal static ж<boolSliceValue> newBoolSliceValue(slice<bool> val, ж<slice<bool>> p) {
    var bsv = @new<boolSliceValue>();
    bsv.value = p;
    bsv.value.ValueSlot = val;
    return bsv;
}

internal static error Set(this ref boolSliceValue s, @string val) {
    var rmQuote = strings.NewReplacer(@"""", "", @"'", "", "`", "");

    var (boolStrSlice, err) = readAsCSV(rmQuote.Replace(val));
    if (err != nil && !AreEqual(err, io.EOF)) {
        return err;
    }

    var @out = new slice<bool>(0, len(boolStrSlice));
    foreach (var (_, boolStr) in range(boolStrSlice)) {
        var (b, errΔ1) = strconv.ParseBool(strings.TrimSpace(boolStr));
        if (errΔ1 != nil) {
            return errΔ1;
        }
        @out = append(@out, b);
    }

    if (!s.changed) {
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }

    s.changed = true;

    return default;
}

internal static @string Type(this ref boolSliceValue s) {
    return "boolSlice";
}

internal static @string String(this ref boolSliceValue s) {
    var boolStrSlice = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, b) in range(s.value.ValueSlot)) {
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
    if (err != nil) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default;
}

internal static error Replace(this ref boolSliceValue s, slice<@string> val) {
    var @out = new slice<bool>(len(val));
    foreach (var (i, d) in range(val)) {
        error err = default;
        (@out[i], err) = s.fromString(d);
        if (err != nil) {
            return err;
        }
    }
    s.value.ValueSlot = @out;
    return default;
}

internal static slice<@string> GetSlice(this ref boolSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in range(s.value.ValueSlot)) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) boolSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (slice<bool>([]), default);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<bool>(len(ss));
    foreach (var (i, t) in range(ss)) {
        error err = default;
        (@out[i], err) = strconv.ParseBool(t);
        if (err != nil) {
            return (default, err);
        }
    }
    return (@out, default);
}

public static (slice<bool>, error) GetBoolSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "boolSlice", boolSliceConv);
    if (err != nil) {
        return (slice<bool>([]), err);
    }
    return (val._<slice<bool>>(), default);
}

public static void BoolSliceVar(this ж<FlagSet> Ꮡf, ж<slice<bool>> p, @string name, slice<bool> value, @string usage) {
    Ꮡf.VarP(new boolSliceValueжValue(newBoolSliceValue(value, p)), name, "", usage);
}

public static void BoolSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<bool>> p, @string name, @string shorthand, slice<bool> value, @string usage) {
    Ꮡf.VarP(new boolSliceValueжValue(newBoolSliceValue(value, p)), name, shorthand, usage);
}

public static void BoolSliceVar(ж<slice<bool>> p, @string name, slice<bool> value, @string usage) {
    CommandLine.VarP(new boolSliceValueжValue(newBoolSliceValue(value, p)), name, "", usage);
}

public static void BoolSliceVarP(ж<slice<bool>> p, @string name, @string shorthand, slice<bool> value, @string usage) {
    CommandLine.VarP(new boolSliceValueжValue(newBoolSliceValue(value, p)), name, shorthand, usage);
}

public static ж<slice<bool>> BoolSlice(this ж<FlagSet> Ꮡf, @string name, slice<bool> value, @string usage) {
    ref var p = ref heap<slice<bool>>(slice<bool>([]), out var Ꮡp);
    Ꮡf.BoolSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<bool>> BoolSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<bool> value, @string usage) {
    ref var p = ref heap<slice<bool>>(slice<bool>([]), out var Ꮡp);
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

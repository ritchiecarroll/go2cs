namespace go.github.com.spf13;

using fmt = fmt_package;
using io = io_package;
using net = net_package;
using strings = strings_package;

partial class pflag_package {

partial struct ipNetSliceValue {
    internal ж<slice<net.IPNet>> value;
    internal bool changed;
}

internal static ж<ipNetSliceValue> newIPNetSliceValue(slice<net.IPNet> val, ж<slice<net.IPNet>> p) {
    var ipnsv = @new<ipNetSliceValue>();
    ipnsv.value = p;
    ipnsv.value.ValueSlot = val;
    return ipnsv;
}

internal static error Set(this ref ipNetSliceValue s, @string val) {
    var rmQuote = strings.NewReplacer(@"""", "", @"'", "", "`", "");

    var (ipNetStrSlice, err) = readAsCSV(rmQuote.Replace(val));
    if (err != nil && !AreEqual(err, io.EOF)) {
        return err;
    }

    var @out = new slice<net.IPNet>(0, len(ipNetStrSlice));
    foreach (var (_, ipNetStr) in range(ipNetStrSlice)) {
        var (_, n, errΔ1) = net.ParseCIDR(strings.TrimSpace(ipNetStr));
        if (errΔ1 != nil) {
            return fmt.Errorf("invalid string being converted to CIDR: %s"u8, ipNetStr);
        }
        @out = append(@out, n.Value);
    }

    if (!s.changed) {
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }

    s.changed = true;

    return default;
}

internal static @string Type(this ref ipNetSliceValue s) {
    return "ipNetSlice";
}

internal static @string String(this ref ipNetSliceValue s) {
    var ipNetStrSlice = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, vᴛ1) in range(s.value.ValueSlot)) {
        ref var n = ref heap(new net.IPNet(), out var Ꮡn);
        n = vᴛ1;

        ipNetStrSlice[i] = Ꮡn.String();
    }

    var (@out, _) = writeAsCSV(ipNetStrSlice);
    return "["u8 + @out + "]"u8;
}

internal static (any, error) ipNetSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (slice<net.IPNet>([]), default);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<net.IPNet>(len(ss));
    foreach (var (i, sval) in range(ss)) {
        var (_, n, err) = net.ParseCIDR(strings.TrimSpace(sval));
        if (err != nil) {
            return (default, fmt.Errorf("invalid string being converted to CIDR: %s"u8, sval));
        }
        @out[i] = n.Value;
    }
    return (@out, default);
}

public static (slice<net.IPNet>, error) GetIPNetSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ipNetSlice", ipNetSliceConv);
    if (err != nil) {
        return (slice<net.IPNet>([]), err);
    }
    return (val._<slice<net.IPNet>>(), default);
}

public static void IPNetSliceVar(this ж<FlagSet> Ꮡf, ж<slice<net.IPNet>> p, @string name, slice<net.IPNet> value, @string usage) {
    Ꮡf.VarP(new ipNetSliceValueжValue(newIPNetSliceValue(value, p)), name, "", usage);
}

public static void IPNetSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<net.IPNet>> p, @string name, @string shorthand, slice<net.IPNet> value, @string usage) {
    Ꮡf.VarP(new ipNetSliceValueжValue(newIPNetSliceValue(value, p)), name, shorthand, usage);
}

public static void IPNetSliceVar(ж<slice<net.IPNet>> p, @string name, slice<net.IPNet> value, @string usage) {
    CommandLine.VarP(new ipNetSliceValueжValue(newIPNetSliceValue(value, p)), name, "", usage);
}

public static void IPNetSliceVarP(ж<slice<net.IPNet>> p, @string name, @string shorthand, slice<net.IPNet> value, @string usage) {
    CommandLine.VarP(new ipNetSliceValueжValue(newIPNetSliceValue(value, p)), name, shorthand, usage);
}

public static ж<slice<net.IPNet>> IPNetSlice(this ж<FlagSet> Ꮡf, @string name, slice<net.IPNet> value, @string usage) {
    ref var p = ref heap<slice<net.IPNet>>(slice<net.IPNet>([]), out var Ꮡp);
    Ꮡf.IPNetSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<net.IPNet>> IPNetSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<net.IPNet> value, @string usage) {
    ref var p = ref heap<slice<net.IPNet>>(slice<net.IPNet>([]), out var Ꮡp);
    Ꮡf.IPNetSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<net.IPNet>> IPNetSlice(@string name, slice<net.IPNet> value, @string usage) {
    return CommandLine.IPNetSliceP(name, "", value, usage);
}

public static ж<slice<net.IPNet>> IPNetSliceP(@string name, @string shorthand, slice<net.IPNet> value, @string usage) {
    return CommandLine.IPNetSliceP(name, shorthand, value, usage);
}

} // end pflag_package

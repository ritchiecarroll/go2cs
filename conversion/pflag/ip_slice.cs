namespace go.github.com.spf13;

using fmt = fmt_package;
using io = io_package;
using net = net_package;
using strings = strings_package;

partial class pflag_package {

partial struct ipSliceValue {
    internal ж<slice<net.IP>> value;
    internal bool changed;
}

internal static ж<ipSliceValue> newIPSliceValue(slice<net.IP> val, ж<slice<net.IP>> Ꮡp) {
    var ipsv = @new<ipSliceValue>();
    ipsv.Value.value = Ꮡp;
    (~ipsv).value.ValueSlot = val;
    return ipsv;
}

internal static error Set(this ref ipSliceValue s, @string val) {
    var rmQuote = strings.NewReplacer(@"""", "", @"'", "", "`", "");

    var (ipStrSlice, err) = readAsCSV(rmQuote.Replace(val));
    if (err != default! && !AreEqual(err, io.EOF)) {
        return err;
    }

    var @out = new slice<net.IP>(0, len(ipStrSlice));
    foreach (var (_, ipStr) in ipStrSlice) {
        var ip = net.ParseIP(strings.TrimSpace(ipStr));
        if (ip == default!) {
            return fmt.Errorf("invalid string being converted to IP address: %s"u8, ipStr);
        }
        @out = append(@out, ip);
    }

    if (!s.changed){
        s.value.ValueSlot = @out;
    } else {
        s.value.ValueSlot = appendꓸꓸꓸ(s.value.ValueSlot, @out);
    }

    s.changed = true;

    return default!;
}

internal static @string Type(this ref ipSliceValue s) {
    return "ipSlice";
}

internal static @string String(this ref ipSliceValue s) {
    var ipStrSlice = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, ip) in s.value.ValueSlot) {
        ipStrSlice[i] = ip.String();
    }

    var (@out, _) = writeAsCSV(ipStrSlice);

    return "["u8 + @out + "]"u8;
}

internal static (net.IP, error) fromString(this ref ipSliceValue s, @string val) {
    return (net.ParseIP(strings.TrimSpace(val)), default!);
}

internal static @string toString(this ref ipSliceValue s, net.IP val) {
    return val.String();
}

internal static error Append(this ref ipSliceValue s, @string val) {
    var (i, err) = s.fromString(val);
    if (err != default!) {
        return err;
    }
    s.value.ValueSlot = append(s.value.ValueSlot, i);
    return default!;
}

internal static error Replace(this ref ipSliceValue s, slice<@string> val) {
    var @out = new slice<net.IP>(len(val));
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

internal static slice<@string> GetSlice(this ref ipSliceValue s) {
    var @out = new slice<@string>(len(s.value.ValueSlot));
    foreach (var (i, d) in s.value.ValueSlot) {
        @out[i] = s.toString(d);
    }
    return @out;
}

internal static (any, error) ipSliceConv(@string val) {
    val = strings.Trim(val, "[]");
    if (len(val) == 0) {
        return (new net.IP[]{}.slice(), default!);
    }
    var ss = strings.Split(val, ",");
    var @out = new slice<net.IP>(len(ss));
    foreach (var (i, sval) in ss) {
        var ip = net.ParseIP(strings.TrimSpace(sval));
        if (ip == default!) {
            return (default!, fmt.Errorf("invalid string being converted to IP address: %s"u8, sval));
        }
        @out[i] = ip;
    }
    return (@out, default!);
}

public static (slice<net.IP>, error) GetIPSlice(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ipSlice", ipSliceConv);
    if (err != default!) {
        return (new net.IP[]{}.slice(), err);
    }
    return (val._<slice<net.IP>>(), default!);
}

public static void IPSliceVar(this ж<FlagSet> Ꮡf, ж<slice<net.IP>> Ꮡp, @string name, slice<net.IP> value, @string usage) {
    Ꮡf.VarP(new ipSliceValueжValue(newIPSliceValue(value, Ꮡp)), name, "", usage);
}

public static void IPSliceVarP(this ж<FlagSet> Ꮡf, ж<slice<net.IP>> Ꮡp, @string name, @string shorthand, slice<net.IP> value, @string usage) {
    Ꮡf.VarP(new ipSliceValueжValue(newIPSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static void IPSliceVar(ж<slice<net.IP>> Ꮡp, @string name, slice<net.IP> value, @string usage) {
    CommandLine.VarP(new ipSliceValueжValue(newIPSliceValue(value, Ꮡp)), name, "", usage);
}

public static void IPSliceVarP(ж<slice<net.IP>> Ꮡp, @string name, @string shorthand, slice<net.IP> value, @string usage) {
    CommandLine.VarP(new ipSliceValueжValue(newIPSliceValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<net.IP>> IPSlice(this ж<FlagSet> Ꮡf, @string name, slice<net.IP> value, @string usage) {
    ref var p = ref heap<slice<net.IP>>(out var Ꮡp);
    p = new net.IP[]{}.slice();
    Ꮡf.IPSliceVarP(Ꮡp, name, "", value, usage);
    return Ꮡp;
}

public static ж<slice<net.IP>> IPSliceP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<net.IP> value, @string usage) {
    ref var p = ref heap<slice<net.IP>>(out var Ꮡp);
    p = new net.IP[]{}.slice();
    Ꮡf.IPSliceVarP(Ꮡp, name, shorthand, value, usage);
    return Ꮡp;
}

public static ж<slice<net.IP>> IPSlice(@string name, slice<net.IP> value, @string usage) {
    return CommandLine.IPSliceP(name, "", value, usage);
}

public static ж<slice<net.IP>> IPSliceP(@string name, @string shorthand, slice<net.IP> value, @string usage) {
    return CommandLine.IPSliceP(name, shorthand, value, usage);
}

} // end pflag_package

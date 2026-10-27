namespace go.github.com.spf13;

using fmt = fmt_package;
using net = net_package;
using strings = strings_package;

partial class pflag_package {

partial struct ipNetValue /*global::go.net_package.IPNet*/;

internal static @string String(this ipNetValue ipnet) {
    ref var n = ref heap<net.IPNet>(out var Ꮡn);
    n = (net.IPNet)ipnet;
    return Ꮡn.String();
}

internal static error Set(this ref ipNetValue ipnet, @string value) {
    var (_, n, err) = net.ParseCIDR(strings.TrimSpace(value));
    if (err != default!) {
        return err;
    }
    ipnet = (ipNetValue)(n.Value);
    return default!;
}

internal static @string Type(this ref ipNetValue _) {
    return "ipNet";
}

internal static ж<ipNetValue> newIPNetValue(net.IPNet val, ж<net.IPNet> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<net.IPNet, ipNetValue>();
}

internal static (any, error) ipNetConv(@string sval) {
    var (_, n, err) = net.ParseCIDR(strings.TrimSpace(sval));
    if (err == default!) {
        return (n.Value, default!);
    }
    return (default!, fmt.Errorf("invalid string being converted to IPNet: %s"u8, sval));
}

public static (net.IPNet, error) GetIPNet(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ipNet", ipNetConv);
    if (err != default!) {
        return (new net.IPNet(nil), err);
    }
    return (val._<net.IPNet>(), default!);
}

public static void IPNetVar(this ж<FlagSet> Ꮡf, ж<net.IPNet> Ꮡp, @string name, net.IPNet value, @string usage) {
    Ꮡf.VarP(new ipNetValueжValue(newIPNetValue(value, Ꮡp)), name, "", usage);
}

public static void IPNetVarP(this ж<FlagSet> Ꮡf, ж<net.IPNet> Ꮡp, @string name, @string shorthand, net.IPNet value, @string usage) {
    Ꮡf.VarP(new ipNetValueжValue(newIPNetValue(value, Ꮡp)), name, shorthand, usage);
}

public static void IPNetVar(ж<net.IPNet> Ꮡp, @string name, net.IPNet value, @string usage) {
    CommandLine.VarP(new ipNetValueжValue(newIPNetValue(value, Ꮡp)), name, "", usage);
}

public static void IPNetVarP(ж<net.IPNet> Ꮡp, @string name, @string shorthand, net.IPNet value, @string usage) {
    CommandLine.VarP(new ipNetValueжValue(newIPNetValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<net.IPNet> IPNet(this ж<FlagSet> Ꮡf, @string name, net.IPNet value, @string usage) {
    var p = @new<net.IPNet>();
    Ꮡf.IPNetVarP(p, name, "", value, usage);
    return p;
}

public static ж<net.IPNet> IPNetP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, net.IPNet value, @string usage) {
    var p = @new<net.IPNet>();
    Ꮡf.IPNetVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<net.IPNet> IPNet(@string name, net.IPNet value, @string usage) {
    return CommandLine.IPNetP(name, "", value, usage);
}

public static ж<net.IPNet> IPNetP(@string name, @string shorthand, net.IPNet value, @string usage) {
    return CommandLine.IPNetP(name, shorthand, value, usage);
}

} // end pflag_package

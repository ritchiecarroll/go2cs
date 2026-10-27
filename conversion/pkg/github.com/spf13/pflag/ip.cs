namespace go.github.com.spf13;

using fmt = fmt_package;
using net = net_package;
using strings = strings_package;

partial class pflag_package {

partial struct ipValue /*global::go.net_package.IP*/;

internal static ж<ipValue> newIPValue(net.IP val, ж<net.IP> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<net.IP, ipValue>();
}

internal static @string String(this ref ipValue i) {
    return ((net.IP)(i)).String();
}

internal static error Set(this ref ipValue i, @string s) {
    if (s == ""u8) {
        return default!;
    }
    var ip = net.ParseIP(strings.TrimSpace(s));
    if (ip == default!) {
        return fmt.Errorf("failed to parse IP: %q"u8, s);
    }
    i = (ipValue)ip;
    return default!;
}

internal static @string Type(this ref ipValue i) {
    return "ip";
}

internal static (any, error) ipConv(@string sval) {
    var ip = net.ParseIP(sval);
    if (ip != default!) {
        return (ip, default!);
    }
    return (default!, fmt.Errorf("invalid string being converted to IP address: %s"u8, sval));
}

public static (net.IP, error) GetIP(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ip", ipConv);
    if (err != default!) {
        return (default!, err);
    }
    return (val._<net.IP>(), default!);
}

public static void IPVar(this ж<FlagSet> Ꮡf, ж<net.IP> Ꮡp, @string name, net.IP value, @string usage) {
    Ꮡf.VarP(new ipValueжValue(newIPValue(value, Ꮡp)), name, "", usage);
}

public static void IPVarP(this ж<FlagSet> Ꮡf, ж<net.IP> Ꮡp, @string name, @string shorthand, net.IP value, @string usage) {
    Ꮡf.VarP(new ipValueжValue(newIPValue(value, Ꮡp)), name, shorthand, usage);
}

public static void IPVar(ж<net.IP> Ꮡp, @string name, net.IP value, @string usage) {
    CommandLine.VarP(new ipValueжValue(newIPValue(value, Ꮡp)), name, "", usage);
}

public static void IPVarP(ж<net.IP> Ꮡp, @string name, @string shorthand, net.IP value, @string usage) {
    CommandLine.VarP(new ipValueжValue(newIPValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<net.IP> IP(this ж<FlagSet> Ꮡf, @string name, net.IP value, @string usage) {
    var p = @new<net.IP>();
    Ꮡf.IPVarP(p, name, "", value, usage);
    return p;
}

public static ж<net.IP> IPP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, net.IP value, @string usage) {
    var p = @new<net.IP>();
    Ꮡf.IPVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<net.IP> IP(@string name, net.IP value, @string usage) {
    return CommandLine.IPP(name, "", value, usage);
}

public static ж<net.IP> IPP(@string name, @string shorthand, net.IP value, @string usage) {
    return CommandLine.IPP(name, shorthand, value, usage);
}

} // end pflag_package

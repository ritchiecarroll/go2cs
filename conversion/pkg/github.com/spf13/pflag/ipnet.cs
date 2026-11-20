// Converted from Go source: "ipnet.go" in module github.com/spf13/pflag@v1.0.9.
// Modified from the original by conversion to C# with go2cs 1.26.11.1.

namespace go.github.com.spf13;

using fmt = fmt_package;
using net = net_package;
using strings = strings_package;
using static pflag_package;

partial class pflag_package {

partial struct ipNetValue /*global::go.net_package.IPNet*/;

} partial class pflagᴛipNetValue {

internal static @string String(this ipNetValue ipnet) {
    ref var n = ref heap<net.IPNet>((net.IPNet)ipnet, out var Ꮡn);
    return Ꮡn.String();
}

internal static error Set(this ref ipNetValue ipnet, @string value) {
    var (_, n, err) = net.ParseCIDR(strings.TrimSpace(value));
    if (err != nil) {
        return err;
    }
    ipnet = (ipNetValue)(n.Value);
    return default;
}

internal static @string Type(this ref ipNetValue _) {
    return "ipNet";
}

} partial class pflag_package {

internal static ж<ipNetValue> newIPNetValue(net.IPNet val, ж<net.IPNet> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<net.IPNet, ipNetValue>();
}

internal static (any, error) ipNetConv(@string sval) {
    var (_, n, err) = net.ParseCIDR(strings.TrimSpace(sval));
    if (err == nil) {
        return (n.Value, default);
    }
    return (default, fmt.Errorf("invalid string being converted to IPNet: %s"u8, sval));
}

} partial class pflagᴛFlagSet {

public static (net.IPNet, error) GetIPNet(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ipNet", ipNetConv);
    if (err != nil) {
        return (new net.IPNet(), err);
    }
    return (val._<net.IPNet>(), default);
}

public static void IPNetVar(this ж<FlagSet> Ꮡf, ж<net.IPNet> p, @string name, net.IPNet value, @string usage) {
    Ꮡf.VarP(new ipNetValueжValue(newIPNetValue(value, p)), name, "", usage);
}

public static void IPNetVarP(this ж<FlagSet> Ꮡf, ж<net.IPNet> p, @string name, @string shorthand, net.IPNet value, @string usage) {
    Ꮡf.VarP(new ipNetValueжValue(newIPNetValue(value, p)), name, shorthand, usage);
}

} partial class pflag_package {

public static void IPNetVar(ж<net.IPNet> p, @string name, net.IPNet value, @string usage) {
    CommandLine.VarP(new ipNetValueжValue(newIPNetValue(value, p)), name, "", usage);
}

public static void IPNetVarP(ж<net.IPNet> p, @string name, @string shorthand, net.IPNet value, @string usage) {
    CommandLine.VarP(new ipNetValueжValue(newIPNetValue(value, p)), name, shorthand, usage);
}

} partial class pflagᴛFlagSet {

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

} partial class pflag_package {

public static ж<net.IPNet> IPNet(@string name, net.IPNet value, @string usage) {
    return CommandLine.IPNetP(name, "", value, usage);
}

public static ж<net.IPNet> IPNetP(@string name, @string shorthand, net.IPNet value, @string usage) {
    return CommandLine.IPNetP(name, shorthand, value, usage);
}

} // end pflag_package

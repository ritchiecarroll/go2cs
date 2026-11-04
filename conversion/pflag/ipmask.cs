namespace go.github.com.spf13;

using fmt = fmt_package;
using net = net_package;
using strconv = strconv_package;

partial class pflag_package {

partial struct ipMaskValue /*global::go.net_package.IPMask*/;

internal static ж<ipMaskValue> newIPMaskValue(net.IPMask val, ж<net.IPMask> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<net.IPMask, ipMaskValue>();
}

internal static @string String(this ref ipMaskValue i) {
    return ((net.IPMask)(i)).String();
}

internal static error Set(this ref ipMaskValue i, @string s) {
    var ip = ParseIPv4Mask(s);
    if (ip == nil) {
        return fmt.Errorf("failed to parse IP mask: %q"u8, s);
    }
    i = (ipMaskValue)ip;
    return default;
}

internal static @string Type(this ref ipMaskValue i) {
    return "ipMask";
}

public static net.IPMask ParseIPv4Mask(@string s) {
    var mᴷ = new stackAppend<nint>();

    var mask = net.ParseIP(s);
    if (mask == nil) {
        if (len(s) != 8) {
            return default;
        }
        var m = slice<nint>([]);
        for (nint i = 0; i < 4; i++) {
            @string b = "0x" + s.slice(2 * i, 2 * i + 2);
            var (d, err) = strconv.ParseInt(b, 0, 0);
            if (err != nil) {
                return default;
            }
            m = append(mᴷ, m, (nint)d);
        }
        @string sΔ1 = fmt.Sprintf("%d.%d.%d.%d"u8, m[0], m[1], m[2], m[3]);
        mask = net.ParseIP(sΔ1);
        if (mask == nil) {
            return default;
        }
    }
    return net.IPv4Mask(mask[12], mask[13], mask[14], mask[15]);
}

internal static (any, error) parseIPv4Mask(@string sval) {
    var mask = ParseIPv4Mask(sval);
    if (mask == nil) {
        return (default, fmt.Errorf("unable to parse %s as net.IPMask"u8, sval));
    }
    return (mask, default);
}

public static (net.IPMask, error) GetIPv4Mask(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "ipMask", parseIPv4Mask);
    if (err != nil) {
        return (default, err);
    }
    return (val._<net.IPMask>(), default);
}

public static void IPMaskVar(this ж<FlagSet> Ꮡf, ж<net.IPMask> p, @string name, net.IPMask value, @string usage) {
    Ꮡf.VarP(new ipMaskValueжValue(newIPMaskValue(value, p)), name, "", usage);
}

public static void IPMaskVarP(this ж<FlagSet> Ꮡf, ж<net.IPMask> p, @string name, @string shorthand, net.IPMask value, @string usage) {
    Ꮡf.VarP(new ipMaskValueжValue(newIPMaskValue(value, p)), name, shorthand, usage);
}

public static void IPMaskVar(ж<net.IPMask> p, @string name, net.IPMask value, @string usage) {
    CommandLine.VarP(new ipMaskValueжValue(newIPMaskValue(value, p)), name, "", usage);
}

public static void IPMaskVarP(ж<net.IPMask> p, @string name, @string shorthand, net.IPMask value, @string usage) {
    CommandLine.VarP(new ipMaskValueжValue(newIPMaskValue(value, p)), name, shorthand, usage);
}

public static ж<net.IPMask> IPMask(this ж<FlagSet> Ꮡf, @string name, net.IPMask value, @string usage) {
    var p = @new<net.IPMask>();
    Ꮡf.IPMaskVarP(p, name, "", value, usage);
    return p;
}

public static ж<net.IPMask> IPMaskP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, net.IPMask value, @string usage) {
    var p = @new<net.IPMask>();
    Ꮡf.IPMaskVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<net.IPMask> IPMask(@string name, net.IPMask value, @string usage) {
    return CommandLine.IPMaskP(name, "", value, usage);
}

public static ж<net.IPMask> IPMaskP(@string name, @string shorthand, net.IPMask value, @string usage) {
    return CommandLine.IPMaskP(name, shorthand, value, usage);
}

} // end pflag_package

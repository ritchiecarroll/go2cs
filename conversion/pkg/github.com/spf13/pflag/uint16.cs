namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct uint16Value /*num:uint16*/;

internal static ж<uint16Value> newUint16Value(uint16 val, ж<uint16> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<uint16, uint16Value>();
}

internal static error Set(this ref uint16Value i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 16);
    i = (uint16Value)(uint16)v;
    return err;
}

internal static @string Type(this ref uint16Value i) {
    return "uint16";
}

internal static @string String(this ref uint16Value i) {
    return strconv.FormatUint((uint64)(uint16)(i), 10);
}

internal static (any, error) uint16Conv(@string sval) {
    var (v, err) = strconv.ParseUint(sval, 0, 16);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((uint16)v, default!);
}

public static (uint16, error) GetUint16(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "uint16", uint16Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<uint16>(), default!);
}

public static void Uint16Var(this ж<FlagSet> Ꮡf, ж<uint16> Ꮡp, @string name, uint16 value, @string usage) {
    Ꮡf.VarP(new uint16ValueжValue(newUint16Value(value, Ꮡp)), name, "", usage);
}

public static void Uint16VarP(this ж<FlagSet> Ꮡf, ж<uint16> Ꮡp, @string name, @string shorthand, uint16 value, @string usage) {
    Ꮡf.VarP(new uint16ValueжValue(newUint16Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Uint16Var(ж<uint16> Ꮡp, @string name, uint16 value, @string usage) {
    CommandLine.VarP(new uint16ValueжValue(newUint16Value(value, Ꮡp)), name, "", usage);
}

public static void Uint16VarP(ж<uint16> Ꮡp, @string name, @string shorthand, uint16 value, @string usage) {
    CommandLine.VarP(new uint16ValueжValue(newUint16Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<uint16> Uint16(this ж<FlagSet> Ꮡf, @string name, uint16 value, @string usage) {
    var p = @new<uint16>();
    Ꮡf.Uint16VarP(p, name, "", value, usage);
    return p;
}

public static ж<uint16> Uint16P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, uint16 value, @string usage) {
    var p = @new<uint16>();
    Ꮡf.Uint16VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<uint16> Uint16(@string name, uint16 value, @string usage) {
    return CommandLine.Uint16P(name, "", value, usage);
}

public static ж<uint16> Uint16P(@string name, @string shorthand, uint16 value, @string usage) {
    return CommandLine.Uint16P(name, shorthand, value, usage);
}

} // end pflag_package

namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct uint32Value /*num:uint32*/;

internal static ж<uint32Value> newUint32Value(uint32 val, ж<uint32> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<uint32, uint32Value>();
}

internal static error Set(this ref uint32Value i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 32);
    i = (uint32Value)(uint32)v;
    return err;
}

internal static @string Type(this ref uint32Value i) {
    return "uint32";
}

internal static @string String(this ref uint32Value i) {
    return strconv.FormatUint((uint64)(uint32)(i), 10);
}

internal static (any, error) uint32Conv(@string sval) {
    var (v, err) = strconv.ParseUint(sval, 0, 32);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((uint32)v, default!);
}

public static (uint32, error) GetUint32(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "uint32", uint32Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<uint32>(), default!);
}

public static void Uint32Var(this ж<FlagSet> Ꮡf, ж<uint32> Ꮡp, @string name, uint32 value, @string usage) {
    Ꮡf.VarP(new uint32ValueжValue(newUint32Value(value, Ꮡp)), name, "", usage);
}

public static void Uint32VarP(this ж<FlagSet> Ꮡf, ж<uint32> Ꮡp, @string name, @string shorthand, uint32 value, @string usage) {
    Ꮡf.VarP(new uint32ValueжValue(newUint32Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Uint32Var(ж<uint32> Ꮡp, @string name, uint32 value, @string usage) {
    CommandLine.VarP(new uint32ValueжValue(newUint32Value(value, Ꮡp)), name, "", usage);
}

public static void Uint32VarP(ж<uint32> Ꮡp, @string name, @string shorthand, uint32 value, @string usage) {
    CommandLine.VarP(new uint32ValueжValue(newUint32Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<uint32> Uint32(this ж<FlagSet> Ꮡf, @string name, uint32 value, @string usage) {
    var p = @new<uint32>();
    Ꮡf.Uint32VarP(p, name, "", value, usage);
    return p;
}

public static ж<uint32> Uint32P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, uint32 value, @string usage) {
    var p = @new<uint32>();
    Ꮡf.Uint32VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<uint32> Uint32(@string name, uint32 value, @string usage) {
    return CommandLine.Uint32P(name, "", value, usage);
}

public static ж<uint32> Uint32P(@string name, @string shorthand, uint32 value, @string usage) {
    return CommandLine.Uint32P(name, shorthand, value, usage);
}

} // end pflag_package

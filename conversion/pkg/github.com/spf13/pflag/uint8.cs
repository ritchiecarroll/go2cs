namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct uint8Value /*num:uint8*/;

internal static ж<uint8Value> newUint8Value(uint8 val, ж<uint8> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<uint8, uint8Value>();
}

internal static error Set(this ref uint8Value i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 8);
    i = (uint8Value)(uint8)v;
    return err;
}

internal static @string Type(this ref uint8Value i) {
    return "uint8";
}

internal static @string String(this ref uint8Value i) {
    return strconv.FormatUint((uint64)(uint8)(i), 10);
}

internal static (any, error) uint8Conv(@string sval) {
    var (v, err) = strconv.ParseUint(sval, 0, 8);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((uint8)v, default!);
}

public static (uint8, error) GetUint8(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "uint8", uint8Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<uint8>(), default!);
}

public static void Uint8Var(this ж<FlagSet> Ꮡf, ж<uint8> Ꮡp, @string name, uint8 value, @string usage) {
    Ꮡf.VarP(new uint8ValueжValue(newUint8Value(value, Ꮡp)), name, "", usage);
}

public static void Uint8VarP(this ж<FlagSet> Ꮡf, ж<uint8> Ꮡp, @string name, @string shorthand, uint8 value, @string usage) {
    Ꮡf.VarP(new uint8ValueжValue(newUint8Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Uint8Var(ж<uint8> Ꮡp, @string name, uint8 value, @string usage) {
    CommandLine.VarP(new uint8ValueжValue(newUint8Value(value, Ꮡp)), name, "", usage);
}

public static void Uint8VarP(ж<uint8> Ꮡp, @string name, @string shorthand, uint8 value, @string usage) {
    CommandLine.VarP(new uint8ValueжValue(newUint8Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<uint8> Uint8(this ж<FlagSet> Ꮡf, @string name, uint8 value, @string usage) {
    var p = @new<uint8>();
    Ꮡf.Uint8VarP(p, name, "", value, usage);
    return p;
}

public static ж<uint8> Uint8P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, uint8 value, @string usage) {
    var p = @new<uint8>();
    Ꮡf.Uint8VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<uint8> Uint8(@string name, uint8 value, @string usage) {
    return CommandLine.Uint8P(name, "", value, usage);
}

public static ж<uint8> Uint8P(@string name, @string shorthand, uint8 value, @string usage) {
    return CommandLine.Uint8P(name, shorthand, value, usage);
}

} // end pflag_package

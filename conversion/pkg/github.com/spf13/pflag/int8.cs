namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct int8Value /*num:int8*/;

internal static ж<int8Value> newInt8Value(int8 val, ж<int8> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<int8, int8Value>();
}

internal static error Set(this ref int8Value i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 8);
    i = (int8Value)(int8)v;
    return err;
}

internal static @string Type(this ref int8Value i) {
    return "int8";
}

internal static @string String(this ref int8Value i) {
    return strconv.FormatInt((int64)(int8)(i), 10);
}

internal static (any, error) int8Conv(@string sval) {
    var (v, err) = strconv.ParseInt(sval, 0, 8);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((int8)v, default!);
}

public static (int8, error) GetInt8(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int8", int8Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<int8>(), default!);
}

public static void Int8Var(this ж<FlagSet> Ꮡf, ж<int8> Ꮡp, @string name, int8 value, @string usage) {
    Ꮡf.VarP(new int8ValueжValue(newInt8Value(value, Ꮡp)), name, "", usage);
}

public static void Int8VarP(this ж<FlagSet> Ꮡf, ж<int8> Ꮡp, @string name, @string shorthand, int8 value, @string usage) {
    Ꮡf.VarP(new int8ValueжValue(newInt8Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Int8Var(ж<int8> Ꮡp, @string name, int8 value, @string usage) {
    CommandLine.VarP(new int8ValueжValue(newInt8Value(value, Ꮡp)), name, "", usage);
}

public static void Int8VarP(ж<int8> Ꮡp, @string name, @string shorthand, int8 value, @string usage) {
    CommandLine.VarP(new int8ValueжValue(newInt8Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<int8> Int8(this ж<FlagSet> Ꮡf, @string name, int8 value, @string usage) {
    var p = @new<int8>();
    Ꮡf.Int8VarP(p, name, "", value, usage);
    return p;
}

public static ж<int8> Int8P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, int8 value, @string usage) {
    var p = @new<int8>();
    Ꮡf.Int8VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<int8> Int8(@string name, int8 value, @string usage) {
    return CommandLine.Int8P(name, "", value, usage);
}

public static ж<int8> Int8P(@string name, @string shorthand, int8 value, @string usage) {
    return CommandLine.Int8P(name, shorthand, value, usage);
}

} // end pflag_package

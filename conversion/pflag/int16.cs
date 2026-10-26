namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct int16Value /*num:int16*/;

internal static ж<int16Value> newInt16Value(int16 val, ж<int16> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<int16, int16Value>();
}

internal static error Set(this ref int16Value i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 16);
    i = (int16Value)(int16)v;
    return err;
}

internal static @string Type(this ref int16Value i) {
    return "int16";
}

internal static @string String(this ref int16Value i) {
    return strconv.FormatInt((int64)(int16)(i), 10);
}

internal static (any, error) int16Conv(@string sval) {
    var (v, err) = strconv.ParseInt(sval, 0, 16);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((int16)v, default!);
}

public static (int16, error) GetInt16(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int16", int16Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<int16>(), default!);
}

public static void Int16Var(this ж<FlagSet> Ꮡf, ж<int16> Ꮡp, @string name, int16 value, @string usage) {
    Ꮡf.VarP(new int16ValueжValue(newInt16Value(value, Ꮡp)), name, "", usage);
}

public static void Int16VarP(this ж<FlagSet> Ꮡf, ж<int16> Ꮡp, @string name, @string shorthand, int16 value, @string usage) {
    Ꮡf.VarP(new int16ValueжValue(newInt16Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Int16Var(ж<int16> Ꮡp, @string name, int16 value, @string usage) {
    CommandLine.VarP(new int16ValueжValue(newInt16Value(value, Ꮡp)), name, "", usage);
}

public static void Int16VarP(ж<int16> Ꮡp, @string name, @string shorthand, int16 value, @string usage) {
    CommandLine.VarP(new int16ValueжValue(newInt16Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<int16> Int16(this ж<FlagSet> Ꮡf, @string name, int16 value, @string usage) {
    var p = @new<int16>();
    Ꮡf.Int16VarP(p, name, "", value, usage);
    return p;
}

public static ж<int16> Int16P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, int16 value, @string usage) {
    var p = @new<int16>();
    Ꮡf.Int16VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<int16> Int16(@string name, int16 value, @string usage) {
    return CommandLine.Int16P(name, "", value, usage);
}

public static ж<int16> Int16P(@string name, @string shorthand, int16 value, @string usage) {
    return CommandLine.Int16P(name, shorthand, value, usage);
}

} // end pflag_package

namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct int32Value /*num:int32*/;

internal static ж<int32Value> newInt32Value(int32 val, ж<int32> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<int32, int32Value>();
}

internal static error Set(this ref int32Value i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 32);
    i = (int32Value)(int32)v;
    return err;
}

internal static @string Type(this ref int32Value i) {
    return "int32";
}

internal static @string String(this ref int32Value i) {
    return strconv.FormatInt((int64)(int32)(i), 10);
}

internal static (any, error) int32Conv(@string sval) {
    var (v, err) = strconv.ParseInt(sval, 0, 32);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((int32)v, default!);
}

public static (int32, error) GetInt32(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int32", int32Conv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<int32>(), default!);
}

public static void Int32Var(this ж<FlagSet> Ꮡf, ж<int32> Ꮡp, @string name, int32 value, @string usage) {
    Ꮡf.VarP(new int32ValueжValue(newInt32Value(value, Ꮡp)), name, "", usage);
}

public static void Int32VarP(this ж<FlagSet> Ꮡf, ж<int32> Ꮡp, @string name, @string shorthand, int32 value, @string usage) {
    Ꮡf.VarP(new int32ValueжValue(newInt32Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Int32Var(ж<int32> Ꮡp, @string name, int32 value, @string usage) {
    CommandLine.VarP(new int32ValueжValue(newInt32Value(value, Ꮡp)), name, "", usage);
}

public static void Int32VarP(ж<int32> Ꮡp, @string name, @string shorthand, int32 value, @string usage) {
    CommandLine.VarP(new int32ValueжValue(newInt32Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<int32> Int32(this ж<FlagSet> Ꮡf, @string name, int32 value, @string usage) {
    var p = @new<int32>();
    Ꮡf.Int32VarP(p, name, "", value, usage);
    return p;
}

public static ж<int32> Int32P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, int32 value, @string usage) {
    var p = @new<int32>();
    Ꮡf.Int32VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<int32> Int32(@string name, int32 value, @string usage) {
    return CommandLine.Int32P(name, "", value, usage);
}

public static ж<int32> Int32P(@string name, @string shorthand, int32 value, @string usage) {
    return CommandLine.Int32P(name, shorthand, value, usage);
}

} // end pflag_package

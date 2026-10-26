namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct float64Value /*num:float64*/;

internal static ж<float64Value> newFloat64Value(float64 val, ж<float64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<float64, float64Value>();
}

internal static error Set(this ref float64Value f, @string s) {
    var (v, err) = strconv.ParseFloat(s, 64);
    f = (float64Value)v;
    return err;
}

internal static @string Type(this ref float64Value f) {
    return "float64";
}

internal static @string String(this ref float64Value f) {
    return strconv.FormatFloat((float64)(f), (rune)'g', -1, 64);
}

internal static (any, error) float64Conv(@string sval) {
    var (ᴛ1, ᴛ2) = strconv.ParseFloat(sval, 64);
    return (ᴛ1, ᴛ2);
}

public static (float64, error) GetFloat64(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "float64", float64Conv);
    if (err != default!) {
        return (0D, err);
    }
    return (val._<float64>(), default!);
}

public static void Float64Var(this ж<FlagSet> Ꮡf, ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    Ꮡf.VarP(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, "", usage);
}

public static void Float64VarP(this ж<FlagSet> Ꮡf, ж<float64> Ꮡp, @string name, @string shorthand, float64 value, @string usage) {
    Ꮡf.VarP(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Float64Var(ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    CommandLine.VarP(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, "", usage);
}

public static void Float64VarP(ж<float64> Ꮡp, @string name, @string shorthand, float64 value, @string usage) {
    CommandLine.VarP(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<float64> Float64(this ж<FlagSet> Ꮡf, @string name, float64 value, @string usage) {
    var p = @new<float64>();
    Ꮡf.Float64VarP(p, name, "", value, usage);
    return p;
}

public static ж<float64> Float64P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, float64 value, @string usage) {
    var p = @new<float64>();
    Ꮡf.Float64VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<float64> Float64(@string name, float64 value, @string usage) {
    return CommandLine.Float64P(name, "", value, usage);
}

public static ж<float64> Float64P(@string name, @string shorthand, float64 value, @string usage) {
    return CommandLine.Float64P(name, shorthand, value, usage);
}

} // end pflag_package

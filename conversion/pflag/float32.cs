namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct float32Value /*num:float32*/;

internal static ж<float32Value> newFloat32Value(float32 val, ж<float32> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<float32, float32Value>();
}

internal static error Set(this ref float32Value f, @string s) {
    var (v, err) = strconv.ParseFloat(s, 32);
    f = (float32Value)(float32)v;
    return err;
}

internal static @string Type(this ref float32Value f) {
    return "float32";
}

internal static @string String(this ref float32Value f) {
    return strconv.FormatFloat((float64)(float32)(f), (rune)'g', -1, 32);
}

internal static (any, error) float32Conv(@string sval) {
    var (v, err) = strconv.ParseFloat(sval, 32);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((float32)v, default!);
}

public static (float32, error) GetFloat32(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "float32", float32Conv);
    if (err != default!) {
        return (0F, err);
    }
    return (val._<float32>(), default!);
}

public static void Float32Var(this ж<FlagSet> Ꮡf, ж<float32> Ꮡp, @string name, float32 value, @string usage) {
    Ꮡf.VarP(new float32ValueжValue(newFloat32Value(value, Ꮡp)), name, "", usage);
}

public static void Float32VarP(this ж<FlagSet> Ꮡf, ж<float32> Ꮡp, @string name, @string shorthand, float32 value, @string usage) {
    Ꮡf.VarP(new float32ValueжValue(newFloat32Value(value, Ꮡp)), name, shorthand, usage);
}

public static void Float32Var(ж<float32> Ꮡp, @string name, float32 value, @string usage) {
    CommandLine.VarP(new float32ValueжValue(newFloat32Value(value, Ꮡp)), name, "", usage);
}

public static void Float32VarP(ж<float32> Ꮡp, @string name, @string shorthand, float32 value, @string usage) {
    CommandLine.VarP(new float32ValueжValue(newFloat32Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<float32> Float32(this ж<FlagSet> Ꮡf, @string name, float32 value, @string usage) {
    var p = @new<float32>();
    Ꮡf.Float32VarP(p, name, "", value, usage);
    return p;
}

public static ж<float32> Float32P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, float32 value, @string usage) {
    var p = @new<float32>();
    Ꮡf.Float32VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<float32> Float32(@string name, float32 value, @string usage) {
    return CommandLine.Float32P(name, "", value, usage);
}

public static ж<float32> Float32P(@string name, @string shorthand, float32 value, @string usage) {
    return CommandLine.Float32P(name, shorthand, value, usage);
}

} // end pflag_package

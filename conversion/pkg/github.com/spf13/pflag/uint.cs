namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct uintValue /*num:nuint*/;

internal static ж<uintValue> newUintValue(nuint val, ж<nuint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<nuint, uintValue>();
}

internal static error Set(this ref uintValue i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 64);
    i = (uintValue)(nuint)v;
    return err;
}

internal static @string Type(this ref uintValue i) {
    return "uint";
}

internal static @string String(this ref uintValue i) {
    return strconv.FormatUint((uint64)(nuint)(i), 10);
}

internal static (any, error) uintConv(@string sval) {
    var (v, err) = strconv.ParseUint(sval, 0, 0);
    if (err != default!) {
        return ((nint)0, err);
    }
    return ((nuint)v, default!);
}

public static (nuint, error) GetUint(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "uint", uintConv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<nuint>(), default!);
}

public static void UintVar(this ж<FlagSet> Ꮡf, ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    Ꮡf.VarP(new uintValueжValue(newUintValue(value, Ꮡp)), name, "", usage);
}

public static void UintVarP(this ж<FlagSet> Ꮡf, ж<nuint> Ꮡp, @string name, @string shorthand, nuint value, @string usage) {
    Ꮡf.VarP(new uintValueжValue(newUintValue(value, Ꮡp)), name, shorthand, usage);
}

public static void UintVar(ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    CommandLine.VarP(new uintValueжValue(newUintValue(value, Ꮡp)), name, "", usage);
}

public static void UintVarP(ж<nuint> Ꮡp, @string name, @string shorthand, nuint value, @string usage) {
    CommandLine.VarP(new uintValueжValue(newUintValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<nuint> Uint(this ж<FlagSet> Ꮡf, @string name, nuint value, @string usage) {
    var p = @new<nuint>();
    Ꮡf.UintVarP(p, name, "", value, usage);
    return p;
}

public static ж<nuint> UintP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, nuint value, @string usage) {
    var p = @new<nuint>();
    Ꮡf.UintVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<nuint> Uint(@string name, nuint value, @string usage) {
    return CommandLine.UintP(name, "", value, usage);
}

public static ж<nuint> UintP(@string name, @string shorthand, nuint value, @string usage) {
    return CommandLine.UintP(name, shorthand, value, usage);
}

} // end pflag_package

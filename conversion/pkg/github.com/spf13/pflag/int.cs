namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct intValue /*num:nint*/;

internal static ж<intValue> newIntValue(nint val, ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<nint, intValue>();
}

internal static error Set(this ref intValue i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 64);
    i = (intValue)(nint)v;
    return err;
}

internal static @string Type(this ref intValue i) {
    return "int";
}

internal static @string String(this ref intValue i) {
    return strconv.Itoa((nint)(i));
}

internal static (any, error) intConv(@string sval) {
    var (ᴛ1, ᴛ2) = strconv.Atoi(sval);
    return (ᴛ1, ᴛ2);
}

public static (nint, error) GetInt(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "int", intConv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<nint>(), default!);
}

public static void IntVar(this ж<FlagSet> Ꮡf, ж<nint> Ꮡp, @string name, nint value, @string usage) {
    Ꮡf.VarP(new intValueжValue(newIntValue(value, Ꮡp)), name, "", usage);
}

public static void IntVarP(this ж<FlagSet> Ꮡf, ж<nint> Ꮡp, @string name, @string shorthand, nint value, @string usage) {
    Ꮡf.VarP(new intValueжValue(newIntValue(value, Ꮡp)), name, shorthand, usage);
}

public static void IntVar(ж<nint> Ꮡp, @string name, nint value, @string usage) {
    CommandLine.VarP(new intValueжValue(newIntValue(value, Ꮡp)), name, "", usage);
}

public static void IntVarP(ж<nint> Ꮡp, @string name, @string shorthand, nint value, @string usage) {
    CommandLine.VarP(new intValueжValue(newIntValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<nint> Int(this ж<FlagSet> Ꮡf, @string name, nint value, @string usage) {
    var p = @new<nint>();
    Ꮡf.IntVarP(p, name, "", value, usage);
    return p;
}

public static ж<nint> IntP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, nint value, @string usage) {
    var p = @new<nint>();
    Ꮡf.IntVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<nint> Int(@string name, nint value, @string usage) {
    return CommandLine.IntP(name, "", value, usage);
}

public static ж<nint> IntP(@string name, @string shorthand, nint value, @string usage) {
    return CommandLine.IntP(name, shorthand, value, usage);
}

} // end pflag_package

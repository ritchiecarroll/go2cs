namespace go.github.com.spf13;

using strconv = strconv_package;

partial class pflag_package {

partial struct countValue /*num:nint*/;

internal static ж<countValue> newCountValue(nint val, ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<nint, countValue>();
}

internal static error Set(this ref countValue i, @string s) {
    if (s == "+1"u8) {
        i = i + 1;
        return default!;
    }
    var (v, err) = strconv.ParseInt(s, 0, 0);
    i = (countValue)(nint)v;
    return err;
}

internal static @string Type(this ref countValue i) {
    return "count";
}

internal static @string String(this ref countValue i) {
    return strconv.Itoa((nint)(i));
}

internal static (any, error) countConv(@string sval) {
    var (i, err) = strconv.Atoi(sval);
    if (err != default!) {
        return (default!, err);
    }
    return (i, default!);
}

public static (nint, error) GetCount(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "count", countConv);
    if (err != default!) {
        return (0, err);
    }
    return (val._<nint>(), default!);
}

public static void CountVar(this ж<FlagSet> Ꮡf, ж<nint> Ꮡp, @string name, @string usage) {
    Ꮡf.CountVarP(Ꮡp, name, "", usage);
}

public static void CountVarP(this ж<FlagSet> Ꮡf, ж<nint> Ꮡp, @string name, @string shorthand, @string usage) {
    var flag = Ꮡf.VarPF(new countValueжValue(newCountValue(0, Ꮡp)), name, shorthand, usage);
    flag.Value.NoOptDefVal = "+1";
}

public static void CountVar(ж<nint> Ꮡp, @string name, @string usage) {
    CommandLine.CountVar(Ꮡp, name, usage);
}

public static void CountVarP(ж<nint> Ꮡp, @string name, @string shorthand, @string usage) {
    CommandLine.CountVarP(Ꮡp, name, shorthand, usage);
}

public static ж<nint> Count(this ж<FlagSet> Ꮡf, @string name, @string usage) {
    var p = @new<nint>();
    Ꮡf.CountVarP(p, name, "", usage);
    return p;
}

public static ж<nint> CountP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, @string usage) {
    var p = @new<nint>();
    Ꮡf.CountVarP(p, name, shorthand, usage);
    return p;
}

public static ж<nint> Count(@string name, @string usage) {
    return CommandLine.CountP(name, "", usage);
}

public static ж<nint> CountP(@string name, @string shorthand, @string usage) {
    return CommandLine.CountP(name, shorthand, usage);
}

} // end pflag_package

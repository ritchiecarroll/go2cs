namespace go.github.com.spf13;

partial class pflag_package {

partial struct stringValue /*@string*/;

internal static ж<stringValue> newStringValue(@string val, ж<@string> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<@string, stringValue>();
}

internal static error Set(this ref stringValue s, @string val) {
    s = (stringValue)val;
    return default!;
}

internal static @string Type(this ref stringValue s) {
    return "string";
}

internal static @string String(this ref stringValue s) {
    return (@string)(s);
}

internal static (any, error) stringConv(@string sval) {
    return (sval, default!);
}

public static (@string, error) GetString(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "string", stringConv);
    if (err != default!) {
        return ("", err);
    }
    return (val._<@string>(), default!);
}

public static void StringVar(this ж<FlagSet> Ꮡf, ж<@string> Ꮡp, @string name, @string value, @string usage) {
    Ꮡf.VarP(new stringValueжValue(newStringValue(value, Ꮡp)), name, "", usage);
}

public static void StringVarP(this ж<FlagSet> Ꮡf, ж<@string> Ꮡp, @string name, @string shorthand, @string value, @string usage) {
    Ꮡf.VarP(new stringValueжValue(newStringValue(value, Ꮡp)), name, shorthand, usage);
}

public static void StringVar(ж<@string> Ꮡp, @string name, @string value, @string usage) {
    CommandLine.VarP(new stringValueжValue(newStringValue(value, Ꮡp)), name, "", usage);
}

public static void StringVarP(ж<@string> Ꮡp, @string name, @string shorthand, @string value, @string usage) {
    CommandLine.VarP(new stringValueжValue(newStringValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<@string> String(this ж<FlagSet> Ꮡf, @string name, @string value, @string usage) {
    var p = @new<@string>();
    Ꮡf.StringVarP(p, name, "", value, usage);
    return p;
}

public static ж<@string> StringP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, @string value, @string usage) {
    var p = @new<@string>();
    Ꮡf.StringVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<@string> String(@string name, @string value, @string usage) {
    return CommandLine.StringP(name, "", value, usage);
}

public static ж<@string> StringP(@string name, @string shorthand, @string value, @string usage) {
    return CommandLine.StringP(name, shorthand, value, usage);
}

} // end pflag_package

namespace go.github.com.spf13;

using static pflag_package;

partial class pflag_package {

partial struct stringValue /*@string*/;

internal static ж<stringValue> newStringValue(@string val, ж<@string> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<@string, stringValue>();
}

} partial class pflagᴛstringValue {

internal static error Set(this ref stringValue s, @string val) {
    s = (stringValue)val;
    return default;
}
internal static @string Type(this ref stringValue s) {
    return "string";
}

internal static @string String(this ref stringValue s) { return (@string)(s); }

} partial class pflag_package {

internal static (any, error) stringConv(@string sval) {
    return (sval, default);
}

} partial class pflagᴛFlagSet {

public static (@string, error) GetString(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "string", stringConv);
    if (err != nil) {
        return ("", err);
    }
    return (val._<@string>(), default);
}

public static void StringVar(this ж<FlagSet> Ꮡf, ж<@string> p, @string name, @string value, @string usage) {
    Ꮡf.VarP(new stringValueжValue(newStringValue(value, p)), name, "", usage);
}

public static void StringVarP(this ж<FlagSet> Ꮡf, ж<@string> p, @string name, @string shorthand, @string value, @string usage) {
    Ꮡf.VarP(new stringValueжValue(newStringValue(value, p)), name, shorthand, usage);
}

} partial class pflag_package {

public static void StringVar(ж<@string> p, @string name, @string value, @string usage) {
    CommandLine.VarP(new stringValueжValue(newStringValue(value, p)), name, "", usage);
}

public static void StringVarP(ж<@string> p, @string name, @string shorthand, @string value, @string usage) {
    CommandLine.VarP(new stringValueжValue(newStringValue(value, p)), name, shorthand, usage);
}

} partial class pflagᴛFlagSet {

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

} partial class pflag_package {

public static ж<@string> String(@string name, @string value, @string usage) {
    return CommandLine.StringP(name, "", value, usage);
}

public static ж<@string> StringP(@string name, @string shorthand, @string value, @string usage) {
    return CommandLine.StringP(name, shorthand, value, usage);
}

} // end pflag_package

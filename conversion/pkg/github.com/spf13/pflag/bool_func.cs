namespace go.github.com.spf13;

partial class pflag_package {

internal delegate error boolfuncValue(@string _);

internal static error Set(this boolfuncValue f, @string s) {
    return f(s);
}

internal static @string Type(this boolfuncValue f) {
    return "boolfunc";
}

internal static @string String(this boolfuncValue f) {
    return "";
}

internal static bool IsBoolFlag(this boolfuncValue f) {
    return true;
}

public static void BoolFunc(this ж<FlagSet> Ꮡf, @string name, @string usage, Func<@string, error> fn) {
    Ꮡf.BoolFuncP(name, "", usage, fn);
}

public static void BoolFuncP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, @string usage, Func<@string, error> fn) {
    Value val = new boolfuncValueᴠValue(NilSafeDelegateConversion<boolfuncValue, Func<@string, error>>(fn));
    var flag = Ꮡf.VarPF(val, name, shorthand, usage);
    flag.Value.NoOptDefVal = "true";
}

public static void BoolFunc(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.BoolFuncP(name, "", usage, fn);
}

public static void BoolFuncP(@string name, @string shorthand, @string usage, Func<@string, error> fn) {
    CommandLine.BoolFuncP(name, shorthand, usage, fn);
}

} // end pflag_package

namespace go.github.com.spf13;

partial class pflag_package {

internal delegate error funcValue(@string _);

internal static error Set(this funcValue f, @string s) {
    return f(s);
}

internal static @string Type(this funcValue f) {
    return "func";
}

internal static @string String(this funcValue f) {
    return "";
}

public static void Func(this ж<FlagSet> Ꮡf, @string name, @string usage, Func<@string, error> fn) {
    Ꮡf.FuncP(name, "", usage, fn);
}

public static void FuncP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, @string usage, Func<@string, error> fn) {
    Value val = new funcValueᴠValue(NilSafeDelegateConversion<funcValue, Func<@string, error>>(fn));
    Ꮡf.VarP(val, name, shorthand, usage);
}

public static void Func(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.FuncP(name, "", usage, fn);
}

public static void FuncP(@string name, @string shorthand, @string usage, Func<@string, error> fn) {
    CommandLine.FuncP(name, shorthand, usage, fn);
}

} // end pflag_package

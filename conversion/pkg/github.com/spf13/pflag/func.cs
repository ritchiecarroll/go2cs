// Converted from Go source: "func.go" in module github.com/spf13/pflag@v1.0.9.
// Modified from the original by conversion to C# with go2cs 1.26.11.1.

namespace go.github.com.spf13;

using static pflag_package;

partial class pflag_package {

internal delegate error funcValue(@string _);

} partial class pflagᴛfuncValue {

internal static error Set(this funcValue f, @string s) { return f(s); }

internal static @string Type(this funcValue f) { return "func"; }

internal static @string String(this funcValue f) { return ""; }

} partial class pflagᴛFlagSet {

public static void Func(this ж<FlagSet> Ꮡf, @string name, @string usage, Func<@string, error> fn) {
    Ꮡf.FuncP(name, "", usage, fn);
}

public static void FuncP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, @string usage, Func<@string, error> fn) {
    Value val = new funcValueᴠValue(NilSafeDelegateConversion<funcValue, Func<@string, error>>(fn));
    Ꮡf.VarP(val, name, shorthand, usage);
}

} partial class pflag_package {

public static void Func(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.FuncP(name, "", usage, fn);
}

public static void FuncP(@string name, @string shorthand, @string usage, Func<@string, error> fn) {
    CommandLine.FuncP(name, shorthand, usage, fn);
}

} // end pflag_package

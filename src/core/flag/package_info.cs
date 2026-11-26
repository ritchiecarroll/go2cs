// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
using encoding = go.encoding_package;
using time = go.time_package;
using Δio = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.flag_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<boolFuncValue, Value>]
[assembly: GoImplement<boolValue, Value>(Pointer = true)]
[assembly: GoImplement<durationValue, Value>(Pointer = true)]
[assembly: GoImplement<float64Value, Value>(Pointer = true)]
[assembly: GoImplement<funcValue, Value>]
[assembly: GoImplement<int64Value, Value>(Pointer = true)]
[assembly: GoImplement<intValue, Value>(Pointer = true)]
[assembly: GoImplement<stringValue, Value>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<textValue, Getter>]
[assembly: GoImplement<uint64Value, Value>(Pointer = true)]
[assembly: GoImplement<uintValue, Value>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Flag))]
[assembly: GoFieldAccess(typeof(FlagSet))]
[assembly: GoFieldAccess(typeof(strconv_package.NumError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("flag/flag.go", "flag.cs", "AGXKAbi4lIKCgpSClIKU3KKCpoKCgpSCppSUAAwYooKmgoKClIKmlMqigqaCgoKUgqaUyqKCpoKCgpSCppTKooKmgoKClIKmlMqigqaCgqaUyqKCpoKCgpSCppTKooKmgoKClIKmlOqCgoKUgoKUgpSCpoKmgqaCgoKm3JTKlJQAQYgBkoKCgoKUhqqigpSokqiSqqKqooK8oqqigryiqJKqoqiSlIKCAAgSgoKClIKUlJSCgpSClIKokqoACwiCgoKUyoKCgpSmABQQAAoEgoKCgoKCgqa4gpSCxrTUtNSkrNKCgoKCgoKCuIK4lLiCgoKUlKbKgoKCAAQ+ABoCqLKClJQADyCCgriAloCasoKUrLKogJaAloCWgJiiqqKqooKCqqKqoqqiqqKCgqqiqqKqoqqigoKqoqqiqqKqooKCqqKqoqqiqqKCgqqiqqKqoqqigoKqoqqiqqKqooKCqqKssqyyrLKCgqyyAAIQ0gACENKssqyyrLKssgACEvSCgoK4goKCgoKUlJSClIKUAAIS4qiygoKq0oKCqsKClPqygpSCgpSCgoKCgqaCgriCgoKCgoKCgriCgoKClKaCgoKmgsqUgpSClIKmgpSCruKCgoKCgpSClJSkgpS0tqiSqrSokgAHEJSClO6mgqyyuJKssoI=", "423-425:1;552-559:1;609-641:1", "", "", "", "575:51,576:13")]
// </GoSourcePositionMaps>

namespace go;

using static go.flag_package;
using encoding = go.encoding_package;
using time = go.time_package;
using Δio = go.io_package;
using ꓸꓸꓸany = variadic<any>;

[GoPackage("flag")]
public static partial class flag_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface boolFlag {}
    internal partial struct boolValue {}
    internal partial struct durationValue {}
    internal partial struct float64Value {}
    internal partial struct int64Value {}
    internal partial struct intValue {}
    internal partial struct stringValue {}
    internal partial struct textValue {}
    internal partial struct uint64Value {}
    internal partial struct uintValue {}
    public partial interface Getter {}
    public partial interface Value {}
    public partial struct ErrorHandling {}
    public partial struct Flag {}
    public partial struct FlagSet {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(flag_package.FlagSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛFlagSet;
[GoMethodHost(typeof(flag_package.boolFuncValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛboolFuncValue;
[GoMethodHost(typeof(flag_package.boolValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛboolValue;
[GoMethodHost(typeof(flag_package.durationValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛdurationValue;
[GoMethodHost(typeof(flag_package.float64Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛfloat64Value;
[GoMethodHost(typeof(flag_package.funcValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛfuncValue;
[GoMethodHost(typeof(flag_package.int64Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛint64Value;
[GoMethodHost(typeof(flag_package.intValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛintValue;
[GoMethodHost(typeof(flag_package.stringValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛstringValue;
[GoMethodHost(typeof(flag_package.textValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛtextValue;
[GoMethodHost(typeof(flag_package.uint64Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛuint64Value;
[GoMethodHost(typeof(flag_package.uintValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flagᴛuintValue;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class flagᴛFlagSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Δio.Writer Output(this ref FlagSet f) => flag_package.Output⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref FlagSet f) => flag_package.Name⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ErrorHandling ErrorHandling(this ref FlagSet f) => flag_package.ErrorHandling⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetOutput(this ref FlagSet f, Δio.Writer output) => flag_package.SetOutput⁔(ref f, output);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void VisitAll(this ref FlagSet f, Action<ж<Flag>> fn) => flag_package.VisitAll⁔(ref f, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Visit(this ref FlagSet f, Action<ж<Flag>> fn) => flag_package.Visit⁔(ref f, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Flag> Lookup(this ref FlagSet f, @string name) => flag_package.Lookup⁔(ref f, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Set(this ref FlagSet f, @string name, @string value) => flag_package.Set⁔(ref f, name, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error set(this ref FlagSet f, @string name, @string value) => flag_package.set⁔(ref f, name, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void PrintDefaults(this ж<FlagSet> Ꮡf) => flag_package.PrintDefaults⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void defaultUsage(this ж<FlagSet> Ꮡf) => flag_package.defaultUsage⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NFlag(this ref FlagSet f) => flag_package.NFlag⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Arg(this ref FlagSet f, nint i) => flag_package.Arg⁔(ref f, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NArg(this ref FlagSet f) => flag_package.NArg⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Args(this ref FlagSet f) => flag_package.Args⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void BoolVar(this ref FlagSet f, ж<bool> Ꮡp, @string name, bool value, @string usage) => flag_package.BoolVar⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<bool> Bool(this ref FlagSet f, @string name, bool value, @string usage) => flag_package.Bool⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void IntVar(this ref FlagSet f, ж<nint> Ꮡp, @string name, nint value, @string usage) => flag_package.IntVar⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<nint> Int(this ref FlagSet f, @string name, nint value, @string usage) => flag_package.Int⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Int64Var(this ref FlagSet f, ж<int64> Ꮡp, @string name, int64 value, @string usage) => flag_package.Int64Var⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<int64> Int64(this ref FlagSet f, @string name, int64 value, @string usage) => flag_package.Int64⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void UintVar(this ref FlagSet f, ж<nuint> Ꮡp, @string name, nuint value, @string usage) => flag_package.UintVar⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<nuint> Uint(this ref FlagSet f, @string name, nuint value, @string usage) => flag_package.Uint⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Uint64Var(this ref FlagSet f, ж<uint64> Ꮡp, @string name, uint64 value, @string usage) => flag_package.Uint64Var⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<uint64> Uint64(this ref FlagSet f, @string name, uint64 value, @string usage) => flag_package.Uint64⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StringVar(this ref FlagSet f, ж<@string> Ꮡp, @string name, @string value, @string usage) => flag_package.StringVar⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<@string> String(this ref FlagSet f, @string name, @string value, @string usage) => flag_package.String⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Float64Var(this ref FlagSet f, ж<float64> Ꮡp, @string name, float64 value, @string usage) => flag_package.Float64Var⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<float64> Float64(this ref FlagSet f, @string name, float64 value, @string usage) => flag_package.Float64⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void DurationVar(this ref FlagSet f, ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) => flag_package.DurationVar⁔(ref f, Ꮡp, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<time.Duration> Duration(this ref FlagSet f, @string name, time.Duration value, @string usage) => flag_package.Duration⁔(ref f, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void TextVar(this ref FlagSet f, encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) => flag_package.TextVar⁔(ref f, p, name, value, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Func(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) => flag_package.Func⁔(ref f, name, usage, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void BoolFunc(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) => flag_package.BoolFunc⁔(ref f, name, usage, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Var(this ref FlagSet f, Value value, @string name, @string usage) => flag_package.Var⁔(ref f, value, name, usage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string sprintf(this ref FlagSet f, @string format, params ꓸꓸꓸany aʗp) => flag_package.sprintf⁔(ref f, format, aʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error failf(this ж<FlagSet> Ꮡf, @string format, params ꓸꓸꓸany aʗp) => flag_package.failf⁔(Ꮡf, format, aʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void usage(this ж<FlagSet> Ꮡf) => flag_package.usage⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, error) parseOne(this ж<FlagSet> Ꮡf) => flag_package.parseOne⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Parse(this ж<FlagSet> Ꮡf, slice<@string> arguments) => flag_package.Parse⁔(Ꮡf, arguments);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Parsed(this ref FlagSet f) => flag_package.Parsed⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Init(this ref FlagSet f, @string name, ErrorHandling errorHandling) => flag_package.Init⁔(ref f, name, errorHandling);
}
partial class flagᴛboolFuncValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this boolFuncValue f, @string s) => flag_package.Set⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this boolFuncValue f) => flag_package.String⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsBoolFlag(this boolFuncValue f) => flag_package.IsBoolFlag⁔(f);
}
partial class flagᴛboolValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref boolValue b, @string s) => flag_package.Set⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref boolValue b) => flag_package.Get⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref boolValue b) => flag_package.String⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsBoolFlag(this ref boolValue b) => flag_package.IsBoolFlag⁔(ref b);
}
partial class flagᴛdurationValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref durationValue d, @string s) => flag_package.Set⁔(ref d, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref durationValue d) => flag_package.Get⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<durationValue> Ꮡd) => flag_package.String⁔(Ꮡd);
}
partial class flagᴛfloat64Value {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref float64Value f, @string s) => flag_package.Set⁔(ref f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref float64Value f) => flag_package.Get⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref float64Value f) => flag_package.String⁔(ref f);
}
partial class flagᴛfuncValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this funcValue f, @string s) => flag_package.Set⁔(f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this funcValue f) => flag_package.String⁔(f);
}
partial class flagᴛint64Value {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref int64Value i, @string s) => flag_package.Set⁔(ref i, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref int64Value i) => flag_package.Get⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref int64Value i) => flag_package.String⁔(ref i);
}
partial class flagᴛintValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref intValue i, @string s) => flag_package.Set⁔(ref i, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref intValue i) => flag_package.Get⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref intValue i) => flag_package.String⁔(ref i);
}
partial class flagᴛstringValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref stringValue s, @string val) => flag_package.Set⁔(ref s, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref stringValue s) => flag_package.Get⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref stringValue s) => flag_package.String⁔(ref s);
}
partial class flagᴛtextValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this textValue v, @string s) => flag_package.Set⁔(v, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this textValue v) => flag_package.Get⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this textValue v) => flag_package.String⁔(v);
}
partial class flagᴛuint64Value {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref uint64Value i, @string s) => flag_package.Set⁔(ref i, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref uint64Value i) => flag_package.Get⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref uint64Value i) => flag_package.String⁔(ref i);
}
partial class flagᴛuintValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Set(this ref uintValue i, @string s) => flag_package.Set⁔(ref i, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Get(this ref uintValue i) => flag_package.Get⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref uintValue i) => flag_package.String⁔(ref i);
}
// </MethodForwarders>

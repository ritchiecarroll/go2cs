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
// </ImportedTypeAliases>

using go;
using static go.flag_package;

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
[assembly: GoPositionMap("flag/flag.go", "flag.cs", "AGbKAbi4lIKCgpSClIKU3KKCxoKCgpSCppSUAA4YooLGgoKClIKmlOqigsaCgoKUgqaU6qKCxoKCgpSCppTqooLGgoKClIKmlOqigsaCgqaU6qKCxoKCgpSCppTqooLGgoKClIKmlAAICoKCgpSCgpSClILGgqaCpoKCgqYACQyUAAgKlJQAQ4gBkoKCgoKUhsqigpSokqiSqqKqooLcosqigtyiyJLKosiSlIKCAAgSgoKClIKUlJSCgpSClILIkqoACwiCgoKUyoKCgpSmABQQAAoEgoKCgoKCgqa4gpSCxrTUtNSkzNKCgoKCgoKCuIK4lLiCgoKUlKbKgoKCAAY+ABoCyLKClJQAESCCgtiAtoC6soKUzLLIgLaAtoC2gLiiyqLKooKCyqLKosqiyqKCgsqiyqLKosqigoLKosqiyqLKooKCyqLKosqiyqKCgsqiyqLKosqigoLKosqiyqLKooKCyqLMssyyzLKCgsyyAAQQ0gAEENLMssyyzLLMsgAEEvSCgoK4goKCgoKUlJSClIKUAAQS4siygoKq0oKCqsKClPqygpSCgpSCgoKCgqaCgriCgoKCgoKCgriCgoKClKaCgoKmgsqUgpSClIKmgpSCruKCgoKCgpSClJSkgpS0tqiSyrSokgAHEJSClO6mgqyyuJLMsoI=", "423-425:1;552-559:1;609-641:1", "", "", "", "636:21,637:13")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using encoding = encoding_package;
using Δio = io_package;
using time = time_package;
using ꓸꓸꓸany = Span<any>;
using static flag_package;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(flag_package.FlagSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛFlagSet;
[GoMethodHost(typeof(flag_package.boolFuncValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛboolFuncValue;
[GoMethodHost(typeof(flag_package.boolValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛboolValue;
[GoMethodHost(typeof(flag_package.durationValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛdurationValue;
[GoMethodHost(typeof(flag_package.float64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛfloat64Value;
[GoMethodHost(typeof(flag_package.funcValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛfuncValue;
[GoMethodHost(typeof(flag_package.int64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛint64Value;
[GoMethodHost(typeof(flag_package.intValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛintValue;
[GoMethodHost(typeof(flag_package.stringValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛstringValue;
[GoMethodHost(typeof(flag_package.textValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛtextValue;
[GoMethodHost(typeof(flag_package.uint64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛuint64Value;
[GoMethodHost(typeof(flag_package.uintValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class flagᴛuintValue;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class flagᴛboolValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref boolValue b, @string s) => flag_package.Setᴍ(ref b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref boolValue b) => flag_package.Getᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref boolValue b) => flag_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsBoolFlag(this ref boolValue b) => flag_package.IsBoolFlagᴍ(ref b);
}
partial class flagᴛintValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref intValue i, @string s) => flag_package.Setᴍ(ref i, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref intValue i) => flag_package.Getᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref intValue i) => flag_package.Stringᴍ(ref i);
}
partial class flagᴛint64Value {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref int64Value i, @string s) => flag_package.Setᴍ(ref i, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref int64Value i) => flag_package.Getᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref int64Value i) => flag_package.Stringᴍ(ref i);
}
partial class flagᴛuintValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref uintValue i, @string s) => flag_package.Setᴍ(ref i, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref uintValue i) => flag_package.Getᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref uintValue i) => flag_package.Stringᴍ(ref i);
}
partial class flagᴛuint64Value {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref uint64Value i, @string s) => flag_package.Setᴍ(ref i, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref uint64Value i) => flag_package.Getᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref uint64Value i) => flag_package.Stringᴍ(ref i);
}
partial class flagᴛstringValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref stringValue s, @string val) => flag_package.Setᴍ(ref s, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref stringValue s) => flag_package.Getᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref stringValue s) => flag_package.Stringᴍ(ref s);
}
partial class flagᴛfloat64Value {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref float64Value f, @string s) => flag_package.Setᴍ(ref f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref float64Value f) => flag_package.Getᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref float64Value f) => flag_package.Stringᴍ(ref f);
}
partial class flagᴛdurationValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this ref durationValue d, @string s) => flag_package.Setᴍ(ref d, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this ref durationValue d) => flag_package.Getᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<durationValue> Ꮡd) => flag_package.Stringᴍ(Ꮡd);
}
partial class flagᴛtextValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this textValue v, @string s) => flag_package.Setᴍ(v, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Get(this textValue v) => flag_package.Getᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this textValue v) => flag_package.Stringᴍ(v);
}
partial class flagᴛfuncValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this funcValue f, @string s) => flag_package.Setᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this funcValue f) => flag_package.Stringᴍ(f);
}
partial class flagᴛboolFuncValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Set(this boolFuncValue f, @string s) => flag_package.Setᴍ(f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this boolFuncValue f) => flag_package.Stringᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsBoolFlag(this boolFuncValue f) => flag_package.IsBoolFlagᴍ(f);
}
partial class flagᴛFlagSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Δio.Writer Output(this ref FlagSet f) => flag_package.Outputᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref FlagSet f) => flag_package.Nameᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ErrorHandling ErrorHandling(this ref FlagSet f) => flag_package.ErrorHandlingᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetOutput(this ref FlagSet f, Δio.Writer output) => flag_package.SetOutputᴍ(ref f, output);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void VisitAll(this ref FlagSet f, Action<ж<Flag>> fn) => flag_package.VisitAllᴍ(ref f, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Visit(this ref FlagSet f, Action<ж<Flag>> fn) => flag_package.Visitᴍ(ref f, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Flag> Lookup(this ref FlagSet f, @string name) => flag_package.Lookupᴍ(ref f, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Set(this ref FlagSet f, @string name, @string value) => flag_package.Setᴍ(ref f, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error set(this ref FlagSet f, @string name, @string value) => flag_package.setᴍ(ref f, name, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void PrintDefaults(this ж<FlagSet> Ꮡf) => flag_package.PrintDefaultsᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void defaultUsage(this ж<FlagSet> Ꮡf) => flag_package.defaultUsageᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NFlag(this ref FlagSet f) => flag_package.NFlagᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Arg(this ref FlagSet f, nint i) => flag_package.Argᴍ(ref f, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NArg(this ref FlagSet f) => flag_package.NArgᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Args(this ref FlagSet f) => flag_package.Argsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void BoolVar(this ref FlagSet f, ж<bool> p, @string name, bool value, @string usage) => flag_package.BoolVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<bool> Bool(this ref FlagSet f, @string name, bool value, @string usage) => flag_package.Boolᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void IntVar(this ref FlagSet f, ж<nint> p, @string name, nint value, @string usage) => flag_package.IntVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<nint> Int(this ref FlagSet f, @string name, nint value, @string usage) => flag_package.Intᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Int64Var(this ref FlagSet f, ж<int64> p, @string name, int64 value, @string usage) => flag_package.Int64Varᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<int64> Int64(this ref FlagSet f, @string name, int64 value, @string usage) => flag_package.Int64ᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void UintVar(this ref FlagSet f, ж<nuint> p, @string name, nuint value, @string usage) => flag_package.UintVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<nuint> Uint(this ref FlagSet f, @string name, nuint value, @string usage) => flag_package.Uintᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Uint64Var(this ref FlagSet f, ж<uint64> p, @string name, uint64 value, @string usage) => flag_package.Uint64Varᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<uint64> Uint64(this ref FlagSet f, @string name, uint64 value, @string usage) => flag_package.Uint64ᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void StringVar(this ref FlagSet f, ж<@string> p, @string name, @string value, @string usage) => flag_package.StringVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<@string> String(this ref FlagSet f, @string name, @string value, @string usage) => flag_package.Stringᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Float64Var(this ref FlagSet f, ж<float64> p, @string name, float64 value, @string usage) => flag_package.Float64Varᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<float64> Float64(this ref FlagSet f, @string name, float64 value, @string usage) => flag_package.Float64ᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void DurationVar(this ref FlagSet f, ж<time.Duration> p, @string name, time.Duration value, @string usage) => flag_package.DurationVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<time.Duration> Duration(this ref FlagSet f, @string name, time.Duration value, @string usage) => flag_package.Durationᴍ(ref f, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void TextVar(this ref FlagSet f, encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) => flag_package.TextVarᴍ(ref f, p, name, value, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Func(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) => flag_package.Funcᴍ(ref f, name, usage, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void BoolFunc(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) => flag_package.BoolFuncᴍ(ref f, name, usage, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Var(this ref FlagSet f, Value value, @string name, @string usage) => flag_package.Varᴍ(ref f, value, name, usage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string sprintf(this ref FlagSet f, @string format, params ꓸꓸꓸany aʗp) => flag_package.sprintfᴍ(ref f, format, aʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error failf(this ж<FlagSet> Ꮡf, @string format, params ꓸꓸꓸany aʗp) => flag_package.failfᴍ(Ꮡf, format, aʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void usage(this ж<FlagSet> Ꮡf) => flag_package.usageᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, error) parseOne(this ж<FlagSet> Ꮡf) => flag_package.parseOneᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Parse(this ж<FlagSet> Ꮡf, slice<@string> arguments) => flag_package.Parseᴍ(Ꮡf, arguments);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Parsed(this ref FlagSet f) => flag_package.Parsedᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Init(this ref FlagSet f, @string name, ErrorHandling errorHandling) => flag_package.Initᴍ(ref f, name, errorHandling);
}
// </GlyphForwarders>

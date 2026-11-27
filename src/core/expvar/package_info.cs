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
using static go.expvar_package;
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
[assembly: GoImplement<Float, Var>(Pointer = true)]
[assembly: GoImplement<Int, Var>(Pointer = true)]
[assembly: GoImplement<Map, Var>(Pointer = true)]
[assembly: GoImplement<go.expvar_package.Func, Var>]
[assembly: GoImplement<go.expvar_package.String, Var>(Pointer = true)]
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.Handler>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Float))]
[assembly: GoFieldAccess(typeof(Int))]
[assembly: GoFieldAccess(typeof(Map))]
[assembly: GoFieldAccess(typeof(go.expvar_package.String))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("expvar/expvar.go", "expvar.cs", "AD50gqaCpoKmgqaCAAcQgqaCpoKokoKCgoKCgsySAA8ggqaCpoKCgoKCpoKCgoKClIKCgpS0xMaCgoKo4oKCgoLY4oKUgoKUoqSCgoKmuIKCgriokoKCgoKCyoK6koKCgoKCyoK64oKCgoKCtKoACAKCgoKCgrT+goKqoqaCpoLugqaCggAJEtKClIKCgqKooqqSgoKmgoKCpoKCgqaCgoKssgAIBoKCrLKmgqaCgoKmgoKUlILokoKCgpSUtLS0tNa0toI=", "136-136:1;139-139:2;145-160:3")]
// </GoSourcePositionMaps>

namespace go;

using static go.expvar_package;

[GoPackage("expvar")]
public static partial class expvar_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface jsonVar {}
    public partial interface Var {}
    public partial struct Float {}
    public partial struct Int {}
    public partial struct KeyValue {}
    public partial struct Map {}
    public partial struct String {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(expvar_package.Float)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class expvarᴛFloat;
[GoMethodHost(typeof(expvar_package.Func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class expvarᴛFunc;
[GoMethodHost(typeof(expvar_package.Int)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class expvarᴛInt;
[GoMethodHost(typeof(expvar_package.Map)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class expvarᴛMap;
[GoMethodHost(typeof(expvar_package.String)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class expvarᴛString;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class expvarᴛFloat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Value(this ж<Float> Ꮡv) => expvar_package.Value⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Float> Ꮡv) => expvar_package.String⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendJSON(this ж<Float> Ꮡv, slice<byte> b) => expvar_package.appendJSON⁔(Ꮡv, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ж<Float> Ꮡv, float64 delta) => expvar_package.Add⁔(Ꮡv, delta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ж<Float> Ꮡv, float64 value) => expvar_package.Set⁔(Ꮡv, value);
}
partial class expvarᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static any Value(this Func f) => expvar_package.Value⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Func f) => expvar_package.String⁔(f);
}
partial class expvarᴛInt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Value(this ж<Int> Ꮡv) => expvar_package.Value⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Int> Ꮡv) => expvar_package.String⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendJSON(this ж<Int> Ꮡv, slice<byte> b) => expvar_package.appendJSON⁔(Ꮡv, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ж<Int> Ꮡv, int64 delta) => expvar_package.Add⁔(Ꮡv, delta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ж<Int> Ꮡv, int64 value) => expvar_package.Set⁔(Ꮡv, value);
}
partial class expvarᴛMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Map> Ꮡv) => expvar_package.String⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendJSON(this ж<Map> Ꮡv, slice<byte> b) => expvar_package.appendJSON⁔(Ꮡv, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendJSONMayExpand(this ж<Map> Ꮡv, slice<byte> b, bool expand) => expvar_package.appendJSONMayExpand⁔(Ꮡv, b, expand);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Map> Init(this ж<Map> Ꮡv) => expvar_package.Init⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addKey(this ж<Map> Ꮡv, @string key) => expvar_package.addKey⁔(Ꮡv, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Var Get(this ж<Map> Ꮡv, @string key) => expvar_package.Get⁔(Ꮡv, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ж<Map> Ꮡv, @string key, Var av) => expvar_package.Set⁔(Ꮡv, key, av);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ж<Map> Ꮡv, @string key, int64 delta) => expvar_package.Add⁔(Ꮡv, key, delta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddFloat(this ж<Map> Ꮡv, @string key, float64 delta) => expvar_package.AddFloat⁔(Ꮡv, key, delta);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Delete(this ж<Map> Ꮡv, @string key) => expvar_package.Delete⁔(Ꮡv, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Do(this ж<Map> Ꮡv, Action<KeyValue> f) => expvar_package.Do⁔(Ꮡv, f);
}
partial class expvarᴛString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Value(this ж<String> Ꮡv) => expvar_package.Value⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<String> Ꮡv) => expvar_package.String⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendJSON(this ж<String> Ꮡv, slice<byte> b) => expvar_package.appendJSON⁔(Ꮡv, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ж<String> Ꮡv, @string value) => expvar_package.Set⁔(Ꮡv, value);
}
// </MethodForwarders>

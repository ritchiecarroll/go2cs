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
using buffer = go.log.slog.@internal.buffer_package;
using context = go.context_package;
using time = go.time_package;
using ꓸꓸꓸAttr = Span<slog_package.Attr>;
using ꓸꓸꓸany = Span<any>;
// </ImportedTypeAliases>

using go;
using static go.log.slog_package;
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
[assembly: GoImplement<JSONHandler, Handler>(Pointer = true)]
[assembly: GoImplement<Level, Leveler>(Pointer = true)]
[assembly: GoImplement<Level, Leveler>]
[assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]
[assembly: GoImplement<MultiHandler, Handler>(Pointer = true)]
[assembly: GoImplement<TextHandler, Handler>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<defaultHandler, Handler>(Pointer = true)]
[assembly: GoImplement<discardHandler, Handler>]
[assembly: GoImplement<handlerWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(LevelVar))]
[assembly: GoFieldAccess(typeof(Logger))]
[assembly: GoFieldAccess(typeof(Source))]
[assembly: GoFieldAccess(typeof(commonHandler))]
[assembly: GoFieldAccess(typeof(jsonEncoder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("log/slog/attr.go", "attr.cs", "ABEkkqiSqqKokqiSqJKqoqiSAAIUAAkCAAIQ8qaCmIKClKqiqJKmgqqi")]
[assembly: GoPositionMap("log/slog/handler.go", "handler.cs", "AGDIAYLcgqwACAKCgoKCgoKC1oKmggBLmAGUAAscooKClKbmgpSUgoKCgoKCuIKCgqamlNaCgoKq8oKCgqaCgpSCgoKCgpS4goKCgpSmgoKClJSCgoKClJSCgpSCgoLWxIKCgoKC3IKCyoKCgoKCgpSUgoKmlIKmupKClAAOHoKCxqLugoKUpoKClIKClKaCggAGEqKCgoKUgqaCupKClKSCgryigoKCpqzSgoKCgqaUpoKmgoKClIKUuIKUypSClIKClIK4gpSmgqaCgoKUlIKUlKiSgpSCgoK0grSkpoKCgoKmgpQACArCktyCgri4goKUlIK0pKKClLiKwoKCgoLKyIKCgg==", "350-355:1;586-601:1", "", "213=Clip/1/2/3,Clip/2/2/5;424=New/1/1/5", "", "630:21,631:13")]
[assembly: GoPositionMap("log/slog/json_handler.go", "json_handler.cs", "ABw80oKUAAkYoqqipoIAAj4AHgLosqaUgoKmopS0tOqAgtbGtLSCgoCClNakAAgSlKaCgrjCgpSSgpSCpoKmgoIADRDSgJKApIKCgoKClIKUgpS0tLTGgoKkgoKUgoKClIKCggAIEoKClIKCgoKUlIKUAA0a", "159-167:1;184-184:1;185-185:2", "", "", "", "199:21,201:13")]
[assembly: GoPositionMap("log/slog/level.go", "level.cs", "ADl2AAkCgoKUgoKUppSkpKS82AACENKCgpSqoqqiAAIQ0ubSgoK4goKCgoKCpqS0tLS0lILqkAAKGJKokqaCqqKqoqqigoKUgg==", "60-69:1;127-131:1", "", "", "", "147:21,148:13")]
[assembly: GoPositionMap("log/slog/logger.go", "logger.cs", "ABtYABgCgoKmgqiAnvLugoKCAAsYgoKClIKEkoK4goKCAAwagoKogJrigpSCggACEgAIAoKUgoKokoKUqLKokoKUrLIAAhoADAKosqiyqLKosqiyqLKosqiyqLKs0oKUgpSCgpSClIKCqLKClIKUgoKUgpSCgqiyqLKosqiyqLKosqiyqLKosqiy")]
[assembly: GoPositionMap("log/slog/multi_handler.go", "multi_handler.cs", "AAsasoKCAAkUgoKCpqaygoKCgrimgoKClKaCgoKU")]
[assembly: GoPositionMap("log/slog/record.go", "record.cs", "AC900gAHFtKCqLKqwoKCpoKCAAoOwoKCgoKUgriCgpSCpoKCgoIABBDSgoKCgpSCgpSClMySgoKCgqYABRbilIKUprYAFyjCgoKUgpSClKiwqtKCpoKC")]
[assembly: GoPositionMap("log/slog/text_handler.go", "text_handler.cs", "ABo40oKUAAkYoqqipoIAAkoAJAKmopS0tIKCgqaClJSClLSkrLKCpoKClKaCgpSCgqaClIKUgoKUlA==")]
[assembly: GoPositionMap("log/slog/value.go", "value.cs", "ADp8ABAagoKUAAYQkpSkxKSkpLS+sqiSqJKokqiSqJKCgpQACRyiypSCgpS4qJKq+IKCgoKmlKiSgoKCpgACJAAPApSkpKSkpKSkpKSkpKSkpKSkpKSktL6ylIKUpKSkpKSkpKSkpM6ygpSCpoKqopKUqqKSlKqikpSmgqqikqamgqqikqamgqqikpSokpSClKS0zKKqooKUpoKssoKCgpSUpKSkpKSkypKCyqqilKSkpKSkpKSkpAAOKgAKAoKCgriCgpSUggAKBoKCgoKUgoKCgoKCgpSCgoKm", "502-506:1", "", "", "", "574:21,575:13")]
// </GoSourcePositionMaps>

namespace go.log;

[GoPackage("slog")]
public static partial class slog_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial class groupptr {}
    internal partial class stringptr {}
    internal partial class timeLocation {}
    internal partial struct commonHandler {}
    internal partial struct defaultHandler {}
    internal partial struct discardHandler {}
    internal partial struct handleState {}
    internal partial struct handlerWriter {}
    internal partial struct jsonEncoder {}
    internal partial struct kind {}
    internal partial struct timeTime {}
    public partial interface Handler {}
    public partial interface Leveler {}
    public partial interface LogValuer {}
    public partial struct Attr {}
    public partial struct HandlerOptions {}
    public partial struct JSONHandler {}
    public partial struct Kind {}
    public partial struct Level {}
    public partial struct LevelVar {}
    public partial struct Logger {}
    public partial struct MultiHandler {}
    [GoValueClone("front")] public partial struct Record {}
    public partial struct Source {}
    public partial struct TextHandler {}
    public partial struct Value {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslogꓸinternalꓸbuffer() => builtin.initPackage(typeof(go.log.slog.@internal.buffer_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(slog_package.Attr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛAttr;
[GoMethodHost(typeof(slog_package.JSONHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛJSONHandler;
[GoMethodHost(typeof(slog_package.Kind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛKind;
[GoMethodHost(typeof(slog_package.Level)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛLevel;
[GoMethodHost(typeof(slog_package.LevelVar)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛLevelVar;
[GoMethodHost(typeof(slog_package.Logger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛLogger;
[GoMethodHost(typeof(slog_package.MultiHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛMultiHandler;
[GoMethodHost(typeof(slog_package.Record)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛRecord;
[GoMethodHost(typeof(slog_package.Source)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛSource;
[GoMethodHost(typeof(slog_package.TextHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛTextHandler;
[GoMethodHost(typeof(slog_package.Value)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛValue;
[GoMethodHost(typeof(slog_package.commonHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛcommonHandler;
[GoMethodHost(typeof(slog_package.defaultHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛdefaultHandler;
[GoMethodHost(typeof(slog_package.discardHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛdiscardHandler;
[GoMethodHost(typeof(slog_package.handleState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛhandleState;
[GoMethodHost(typeof(slog_package.handlerWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class slogᴛhandlerWriter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class slogᴛAttr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this slog_package.Attr a, slog_package.Attr b) => slog_package.Equal⁔(a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this slog_package.Attr a) => slog_package.String⁔(a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmpty(this slog_package.Attr a) => slog_package.isEmpty⁔(a);
}
partial class slogᴛJSONHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref slog_package.JSONHandler h, context.Context _, slog_package.Level level) => slog_package.Enabled⁔(ref h, _, level);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithAttrs(this ref slog_package.JSONHandler h, slice<slog_package.Attr> attrs) => slog_package.WithAttrs⁔(ref h, attrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithGroup(this ref slog_package.JSONHandler h, @string name) => slog_package.WithGroup⁔(ref h, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Handle(this ref slog_package.JSONHandler h, context.Context _, slog_package.Record r) => slog_package.Handle⁔(ref h, _, r);
}
partial class slogᴛKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this slog_package.Kind k) => slog_package.String⁔(k);
}
partial class slogᴛLevel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this slog_package.Level l) => slog_package.String⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this slog_package.Level l) => slog_package.MarshalJSON⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalJSON(this ж<slog_package.Level> Ꮡl, slice<byte> data) => slog_package.UnmarshalJSON⁔(Ꮡl, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this slog_package.Level l, slice<byte> b) => slog_package.AppendText⁔(l, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this slog_package.Level l) => slog_package.MarshalText⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<slog_package.Level> Ꮡl, slice<byte> data) => slog_package.UnmarshalText⁔(Ꮡl, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ parse(this ж<slog_package.Level> Ꮡl, @string s) => slog_package.parse⁔(Ꮡl, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Level Level(this slog_package.Level l) => slog_package.Level⁔(l);
}
partial class slogᴛLevelVar {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Level Level(this ж<slog_package.LevelVar> Ꮡv) => slog_package.Level⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this ж<slog_package.LevelVar> Ꮡv, slog_package.Level l) => slog_package.Set⁔(Ꮡv, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<slog_package.LevelVar> Ꮡv) => slog_package.String⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<slog_package.LevelVar> Ꮡv, slice<byte> b) => slog_package.AppendText⁔(Ꮡv, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this ж<slog_package.LevelVar> Ꮡv) => slog_package.MarshalText⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<slog_package.LevelVar> Ꮡv, slice<byte> data) => slog_package.UnmarshalText⁔(Ꮡv, data);
}
partial class slogᴛLogger {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<slog_package.Logger> clone(this ref slog_package.Logger l) => slog_package.clone⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler Handler(this ref slog_package.Logger l) => slog_package.Handler⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<slog_package.Logger> With(this ж<slog_package.Logger> Ꮡl, params ꓸꓸꓸany argsʗp) => slog_package.With⁔(Ꮡl, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<slog_package.Logger> WithGroup(this ж<slog_package.Logger> Ꮡl, @string name) => slog_package.WithGroup⁔(Ꮡl, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref slog_package.Logger l, context.Context ctx, slog_package.Level level) => slog_package.Enabled⁔(ref l, ctx, level);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Log(this ref slog_package.Logger l, context.Context ctx, slog_package.Level level, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Log⁔(ref l, ctx, level, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void LogAttrs(this ref slog_package.Logger l, context.Context ctx, slog_package.Level level, @string msg, params ꓸꓸꓸAttr attrsʗp) => slog_package.LogAttrs⁔(ref l, ctx, level, msg, attrsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Debug(this ref slog_package.Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Debug⁔(ref l, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void DebugContext(this ref slog_package.Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.DebugContext⁔(ref l, ctx, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Info(this ref slog_package.Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Info⁔(ref l, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void InfoContext(this ref slog_package.Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.InfoContext⁔(ref l, ctx, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Warn(this ref slog_package.Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Warn⁔(ref l, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void WarnContext(this ref slog_package.Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.WarnContext⁔(ref l, ctx, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Error(this ref slog_package.Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Error⁔(ref l, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ErrorContext(this ref slog_package.Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.ErrorContext⁔(ref l, ctx, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void log(this ref slog_package.Logger l, context.Context ctx, slog_package.Level level, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.log⁔(ref l, ctx, level, msg, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logAttrs(this ref slog_package.Logger l, context.Context ctx, slog_package.Level level, @string msg, params ꓸꓸꓸAttr attrsʗp) => slog_package.logAttrs⁔(ref l, ctx, level, msg, attrsʗp);
}
partial class slogᴛMultiHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref slog_package.MultiHandler h, context.Context ctx, slog_package.Level l) => slog_package.Enabled⁔(ref h, ctx, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Handle(this ref slog_package.MultiHandler h, context.Context ctx, slog_package.Record r) => slog_package.Handle⁔(ref h, ctx, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithAttrs(this ref slog_package.MultiHandler h, slice<slog_package.Attr> attrs) => slog_package.WithAttrs⁔(ref h, attrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithGroup(this ref slog_package.MultiHandler h, @string name) => slog_package.WithGroup⁔(ref h, name);
}
partial class slogᴛRecord {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Record Clone(this slog_package.Record r) => slog_package.Clone⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumAttrs(this slog_package.Record r) => slog_package.NumAttrs⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Attrs(this slog_package.Record r, Func<slog_package.Attr, bool> f) => slog_package.Attrs⁔(r, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddAttrs(this ref slog_package.Record r, params ꓸꓸꓸAttr attrsʗp) => slog_package.AddAttrs⁔(ref r, attrsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ref slog_package.Record r, params ꓸꓸꓸany argsʗp) => slog_package.Add⁔(ref r, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<slog_package.Source> Source(this slog_package.Record r) => slog_package.Source⁔(r);
}
partial class slogᴛSource {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.Value group(this ref slog_package.Source s) => slog_package.group⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmpty(this ж<slog_package.Source> Ꮡs) => slog_package.isEmpty⁔(Ꮡs);
}
partial class slogᴛTextHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Enabled(this ref slog_package.TextHandler h, context.Context _, slog_package.Level level) => slog_package.Enabled⁔(ref h, _, level);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithAttrs(this ref slog_package.TextHandler h, slice<slog_package.Attr> attrs) => slog_package.WithAttrs⁔(ref h, attrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Handler WithGroup(this ref slog_package.TextHandler h, @string name) => slog_package.WithGroup⁔(ref h, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Handle(this ref slog_package.TextHandler h, context.Context _, slog_package.Record r) => slog_package.Handle⁔(ref h, _, r);
}
partial class slogᴛValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Kind Kind(this slog_package.Value v) => slog_package.Kind⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static any Any(this slog_package.Value v) => slog_package.Any⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this slog_package.Value v) => slog_package.String⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string str(this slog_package.Value v) => slog_package.str⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64(this slog_package.Value v) => slog_package.Int64⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this slog_package.Value v) => slog_package.Uint64⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Bool(this slog_package.Value v) => slog_package.Bool⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @bool(this slog_package.Value v) => slog_package.bool⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static time.Duration Duration(this slog_package.Value v) => slog_package.Duration⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration duration(this slog_package.Value v) => slog_package.duration⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Float64(this slog_package.Value v) => slog_package.Float64⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 @float(this slog_package.Value v) => slog_package.float⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static time.Time Time(this slog_package.Value v) => slog_package.Time⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time time(this slog_package.Value v) => slog_package.time⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.LogValuer LogValuer(this slog_package.Value v) => slog_package.LogValuer⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<slog_package.Attr> Group(this slog_package.Value v) => slog_package.Group⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<slog_package.Attr> group(this slog_package.Value v) => slog_package.group⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this slog_package.Value v, slog_package.Value w) => slog_package.Equal⁔(v, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmptyGroup(this slog_package.Value v) => slog_package.isEmptyGroup⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> append(this slog_package.Value v, slice<byte> dst) => slog_package.append⁔(v, dst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slog_package.Value /*rv*/ Resolve(this slog_package.Value v) => slog_package.Resolve⁔(v);
}
partial class slogᴛcommonHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<slog_package.commonHandler> clone(this ref slog_package.commonHandler h) => slog_package.clone⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool enabled(this ref slog_package.commonHandler h, slog_package.Level l) => slog_package.enabled⁔(ref h, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<slog_package.commonHandler> withAttrs(this ж<slog_package.commonHandler> Ꮡh, slice<slog_package.Attr> @as) => slog_package.withAttrs⁔(Ꮡh, @as);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<slog_package.commonHandler> withGroup(this ref slog_package.commonHandler h, @string name) => slog_package.withGroup⁔(ref h, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handle(this ж<slog_package.commonHandler> Ꮡh, slog_package.Record r) => slog_package.handle⁔(Ꮡh, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string attrSep(this ref slog_package.commonHandler h) => slog_package.attrSep⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.handleState newHandleState(this ж<slog_package.commonHandler> Ꮡh, ж<buffer.Buffer> Ꮡbuf, bool freeBuf, @string sep) => slog_package.newHandleState⁔(Ꮡh, Ꮡbuf, freeBuf, sep);
}
partial class slogᴛdefaultHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Enabled(this ref slog_package.defaultHandler _Δp0, context.Context _Δp1, slog_package.Level l) => slog_package.Enabled⁔(ref _Δp0, _Δp1, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Handle(this ж<slog_package.defaultHandler> Ꮡh, context.Context ctx, slog_package.Record r) => slog_package.Handle⁔(Ꮡh, ctx, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.Handler WithAttrs(this ref slog_package.defaultHandler h, slice<slog_package.Attr> @as) => slog_package.WithAttrs⁔(ref h, @as);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.Handler WithGroup(this ref slog_package.defaultHandler h, @string name) => slog_package.WithGroup⁔(ref h, name);
}
partial class slogᴛdiscardHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Enabled(this slog_package.discardHandler dh, context.Context _Δp1, slog_package.Level _Δp2) => slog_package.Enabled⁔(dh, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Handle(this slog_package.discardHandler dh, context.Context _Δp1, slog_package.Record _Δp2) => slog_package.Handle⁔(dh, _Δp1, _Δp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.Handler WithAttrs(this slog_package.discardHandler dh, slice<slog_package.Attr> attrs) => slog_package.WithAttrs⁔(dh, attrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slog_package.Handler WithGroup(this slog_package.discardHandler dh, @string name) => slog_package.WithGroup⁔(dh, name);
}
partial class slogᴛhandleState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendNonBuiltIns(this ж<slog_package.handleState> Ꮡs, slog_package.Record r) => slog_package.appendNonBuiltIns⁔(Ꮡs, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ref slog_package.handleState s) => slog_package.free⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openGroups(this ref slog_package.handleState s) => slog_package.openGroups⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openGroup(this ref slog_package.handleState s, @string name) => slog_package.openGroup⁔(ref s, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeGroup(this ref slog_package.handleState s, @string name) => slog_package.closeGroup⁔(ref s, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool appendAttrs(this ж<slog_package.handleState> Ꮡs, slice<slog_package.Attr> @as) => slog_package.appendAttrs⁔(Ꮡs, @as);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool appendAttr(this ж<slog_package.handleState> Ꮡs, slog_package.Attr a) => slog_package.appendAttr⁔(Ꮡs, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendError(this ref slog_package.handleState s, error err) => slog_package.appendError⁔(ref s, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendKey(this ref slog_package.handleState s, @string key) => slog_package.appendKey⁔(ref s, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendTwoStrings(this ref slog_package.handleState s, @string x, @string y) => slog_package.appendTwoStrings⁔(ref s, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendString(this ref slog_package.handleState s, @string str) => slog_package.appendString⁔(ref s, str);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendValue(this ж<slog_package.handleState> Ꮡs, slog_package.Value v) => slog_package.appendValue⁔(Ꮡs, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void appendTime(this ж<slog_package.handleState> Ꮡs, time.Time t) => slog_package.appendTime⁔(Ꮡs, t);
}
partial class slogᴛhandlerWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref slog_package.handlerWriter w, slice<byte> buf) => slog_package.Write⁔(ref w, buf);
}
// </MethodForwarders>

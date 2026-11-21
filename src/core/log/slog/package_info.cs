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
using static go.log.slog_package;

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
[assembly: GoPositionMap("log/slog/attr.go", "attr.cs", "ABIkkqiSqqKokqiSqJKqoqiSAAIUAAkCAAIQ8qaCmIKClKqiyJKmgqqi")]
[assembly: GoPositionMap("log/slog/handler.go", "handler.cs", "AGHIAYL8gqwACAKCgoKCgoKC1oKmggBPmAGUAAscooKClKbmgpSUgoKCgoKCuIKCgqamlNaCgoKq8oKCgqaCgpSCgoKCgpS4goKCgpSmgoKClJSCgoKClJSCgpSCgoL2xIKCgoKC3IKCyoKCgoKCgpSUgoKmlIKm2pKClAAQHoKC5qLugoKUxoKClIKClKaCggAKEqKCgoKUgqaCupKClKSCgryigoKCpqzSgoKCgqaUpoKmgoKClIKUuIKUypSClIKClIK4gpSmgqaCgoKUlIKUlKiSgpSCgoK0grSkpoKCgoKmgpQACArCktyCgri4goKUlIK0pKKClNiKwoKCgoLK6IKCgg==", "350-355:1;586-601:1", "", "220=Clip/1/2/3,Clip/2/2/5;439=New/1/1/5", "", "651:21,652:13")]
[assembly: GoPositionMap("log/slog/json_handler.go", "json_handler.cs", "AB080oKUAAsYoqqipoIAAj4AHgIACAiyppSCgqailLS06oCC1sa0tIKCgIKU1qQACBKUpoKCuMKClJKClIKmgqaCggANENKAkoCkgoKCgoKUgpSClLS0tMaCgqSCgpSCgoKUgoKCAAgSgoKUgoKCgpSUgpQADRo=", "159-167:1;184-184:1;185-185:2", "", "", "", "204:21,206:13")]
[assembly: GoPositionMap("log/slog/level.go", "level.cs", "ADx2AAkCgoKUgoKUppSkpKS82AACENKCgpSqoqqiAAIQ0ubSgoK4goKCgoKCpqS0tLS0lILqkAAOGJKokqaCqqKqoqqigoKUgg==", "60-69:1;127-131:1", "", "", "", "150:21,151:13")]
[assembly: GoPositionMap("log/slog/logger.go", "logger.cs", "ABxYABgCgoKmgqiAnvLugoKCAA0YgoKClIKEkoK4goKCABAagoKogJrigpSCggACEgAIAoKUgoLIkoKUqLLIkoKUzLIABBoADAKosqiyqLKosqiyqLKosqiyqLKs0oKUgpSCgpSClIKCqLKClIKUgoKUgpSCgsiyqLKosqiyqLKosqiyqLKosqiy")]
[assembly: GoPositionMap("log/slog/multi_handler.go", "multi_handler.cs", "AAwasoKCAAsUgoKCpqaygoKCgrimgoKClKaCgoKU")]
[assembly: GoPositionMap("log/slog/record.go", "record.cs", "ADB00gAJFtKCqLKqwoKCpoKCAAoOwoKCgoKUgriCgpSCpoKCgoIABBDSgoKCgpSCgpSClOySgoKCgqYABRbilIKUprYAGSjCgoKUgpSClKiwytKCpoKC")]
[assembly: GoPositionMap("log/slog/text_handler.go", "text_handler.cs", "ABs40oKUAAsYoqqipoIAAkoAJALGopS0tIKCgqaClJSClLSkrLKCpoKClKaCgpSCgqaClIKUgoKUlA==")]
[assembly: GoPositionMap("log/slog/value.go", "value.cs", "ADt8ABIagoKUAAoQkpSkxKSkpLTesqiSqJKokqiSqJKCgpQACRyiypSCgpS4qJKq+IKCgoKmlKiSgoKCpgACJAAPApSkpKSkpKSkpKSkpKSkpKSkpKSktN6ylIKUpKSkpKSkpKSkpM6ygpSCpoKqopKUqqKSlKqikpSmgqqikqamgqqikqamgqqikpSokpSClKS0zKKqooKUpoKssoKCgpSUpKSkpKSkypKCyqqilKSkpKSkpKSkpAASKgAKAoKCgriCgpSUggAMBoKCgoKUgoKCgoKCgpSCgoKm", "502-506:1", "", "", "", "589:21,590:13")]
// </GoSourcePositionMaps>

namespace go.log;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using time = time_package;
using ꓸꓸꓸAttr = Span<slog_package.Attr>;
using ꓸꓸꓸany = Span<any>;
using static slog_package;
using context = context_package;
using buffer = go.log.slog.@internal.buffer_package;
using go.log.slog.@internal;
using go.encoding;
using go.unicode;
using go.sync;
using log = log_package;
using go.log.slog;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(slog_package.Attr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛAttr;
[GoMethodHost(typeof(slog_package.JSONHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛJSONHandler;
[GoMethodHost(typeof(slog_package.Kind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛKind;
[GoMethodHost(typeof(slog_package.Level)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLevel;
[GoMethodHost(typeof(slog_package.LevelVar)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLevelVar;
[GoMethodHost(typeof(slog_package.Logger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛLogger;
[GoMethodHost(typeof(slog_package.MultiHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛMultiHandler;
[GoMethodHost(typeof(slog_package.Record)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛRecord;
[GoMethodHost(typeof(slog_package.Source)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛSource;
[GoMethodHost(typeof(slog_package.TextHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛTextHandler;
[GoMethodHost(typeof(slog_package.Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛValue;
[GoMethodHost(typeof(slog_package.commonHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛcommonHandler;
[GoMethodHost(typeof(slog_package.defaultHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛdefaultHandler;
[GoMethodHost(typeof(slog_package.discardHandler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛdiscardHandler;
[GoMethodHost(typeof(slog_package.handleState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛhandleState;
[GoMethodHost(typeof(slog_package.handlerWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class slogᴛhandlerWriter;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class slogᴛAttr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this Attr a, Attr b) => slog_package.Equalᴍ(a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Attr a) => slog_package.Stringᴍ(a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmpty(this Attr a) => slog_package.isEmptyᴍ(a);
}
partial class slogᴛdefaultHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Enabled(this ref defaultHandler _Δp0, context.Context _Δp1, Level l) => slog_package.Enabledᴍ(ref _Δp0, _Δp1, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Handle(this ж<defaultHandler> Ꮡh, context.Context ctx, Record r) => slog_package.Handleᴍ(Ꮡh, ctx, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Handler WithAttrs(this ref defaultHandler h, slice<Attr> @as) => slog_package.WithAttrsᴍ(ref h, @as);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Handler WithGroup(this ref defaultHandler h, @string name) => slog_package.WithGroupᴍ(ref h, name);
}
partial class slogᴛcommonHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<commonHandler> clone(this ref commonHandler h) => slog_package.cloneᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool enabled(this ref commonHandler h, Level l) => slog_package.enabledᴍ(ref h, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<commonHandler> withAttrs(this ж<commonHandler> Ꮡh, slice<Attr> @as) => slog_package.withAttrsᴍ(Ꮡh, @as);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<commonHandler> withGroup(this ref commonHandler h, @string name) => slog_package.withGroupᴍ(ref h, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error handle(this ж<commonHandler> Ꮡh, Record r) => slog_package.handleᴍ(Ꮡh, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string attrSep(this ref commonHandler h) => slog_package.attrSepᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static handleState newHandleState(this ж<commonHandler> Ꮡh, ж<buffer.Buffer> buf, bool freeBuf, @string sep) => slog_package.newHandleStateᴍ(Ꮡh, buf, freeBuf, sep);
}
partial class slogᴛhandleState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendNonBuiltIns(this ж<handleState> Ꮡs, Record r) => slog_package.appendNonBuiltInsᴍ(Ꮡs, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ref handleState s) => slog_package.freeᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openGroups(this ref handleState s) => slog_package.openGroupsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openGroup(this ref handleState s, @string name) => slog_package.openGroupᴍ(ref s, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeGroup(this ref handleState s, @string name) => slog_package.closeGroupᴍ(ref s, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool appendAttrs(this ж<handleState> Ꮡs, slice<Attr> @as) => slog_package.appendAttrsᴍ(Ꮡs, @as);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool appendAttr(this ж<handleState> Ꮡs, Attr a) => slog_package.appendAttrᴍ(Ꮡs, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendError(this ref handleState s, error err) => slog_package.appendErrorᴍ(ref s, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendKey(this ref handleState s, @string key) => slog_package.appendKeyᴍ(ref s, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendTwoStrings(this ref handleState s, @string x, @string y) => slog_package.appendTwoStringsᴍ(ref s, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendString(this ref handleState s, @string str) => slog_package.appendStringᴍ(ref s, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendValue(this ж<handleState> Ꮡs, Value v) => slog_package.appendValueᴍ(Ꮡs, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void appendTime(this ж<handleState> Ꮡs, time.Time t) => slog_package.appendTimeᴍ(Ꮡs, t);
}
partial class slogᴛdiscardHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Enabled(this discardHandler dh, context.Context _Δp1, Level _Δp2) => slog_package.Enabledᴍ(dh, _Δp1, _Δp2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Handle(this discardHandler dh, context.Context _Δp1, Record _Δp2) => slog_package.Handleᴍ(dh, _Δp1, _Δp2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Handler WithAttrs(this discardHandler dh, slice<Attr> attrs) => slog_package.WithAttrsᴍ(dh, attrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Handler WithGroup(this discardHandler dh, @string name) => slog_package.WithGroupᴍ(dh, name);
}
partial class slogᴛJSONHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Enabled(this ref JSONHandler h, context.Context _, Level level) => slog_package.Enabledᴍ(ref h, _, level);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithAttrs(this ref JSONHandler h, slice<Attr> attrs) => slog_package.WithAttrsᴍ(ref h, attrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithGroup(this ref JSONHandler h, @string name) => slog_package.WithGroupᴍ(ref h, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Handle(this ref JSONHandler h, context.Context _, Record r) => slog_package.Handleᴍ(ref h, _, r);
}
partial class slogᴛLevel {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Level l) => slog_package.Stringᴍ(l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this Level l) => slog_package.MarshalJSONᴍ(l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalJSON(this ж<Level> Ꮡl, slice<byte> data) => slog_package.UnmarshalJSONᴍ(Ꮡl, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this Level l, slice<byte> b) => slog_package.AppendTextᴍ(l, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this Level l) => slog_package.MarshalTextᴍ(l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ж<Level> Ꮡl, slice<byte> data) => slog_package.UnmarshalTextᴍ(Ꮡl, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ parse(this ж<Level> Ꮡl, @string s) => slog_package.parseᴍ(Ꮡl, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Level Level(this Level l) => slog_package.Levelᴍ(l);
}
partial class slogᴛLevelVar {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Level Level(this ж<LevelVar> Ꮡv) => slog_package.Levelᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ж<LevelVar> Ꮡv, Level l) => slog_package.Setᴍ(Ꮡv, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<LevelVar> Ꮡv) => slog_package.Stringᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<LevelVar> Ꮡv, slice<byte> b) => slog_package.AppendTextᴍ(Ꮡv, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this ж<LevelVar> Ꮡv) => slog_package.MarshalTextᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ж<LevelVar> Ꮡv, slice<byte> data) => slog_package.UnmarshalTextᴍ(Ꮡv, data);
}
partial class slogᴛhandlerWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref handlerWriter w, slice<byte> buf) => slog_package.Writeᴍ(ref w, buf);
}
partial class slogᴛLogger {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Logger> clone(this ref Logger l) => slog_package.cloneᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler Handler(this ref Logger l) => slog_package.Handlerᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Logger> With(this ж<Logger> Ꮡl, params ꓸꓸꓸany argsʗp) => slog_package.Withᴍ(Ꮡl, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Logger> WithGroup(this ж<Logger> Ꮡl, @string name) => slog_package.WithGroupᴍ(Ꮡl, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Enabled(this ref Logger l, context.Context ctx, Level level) => slog_package.Enabledᴍ(ref l, ctx, level);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Log(this ref Logger l, context.Context ctx, Level level, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Logᴍ(ref l, ctx, level, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void LogAttrs(this ref Logger l, context.Context ctx, Level level, @string msg, params ꓸꓸꓸAttr attrsʗp) => slog_package.LogAttrsᴍ(ref l, ctx, level, msg, attrsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Debug(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Debugᴍ(ref l, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void DebugContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.DebugContextᴍ(ref l, ctx, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Info(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Infoᴍ(ref l, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void InfoContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.InfoContextᴍ(ref l, ctx, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Warn(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Warnᴍ(ref l, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void WarnContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.WarnContextᴍ(ref l, ctx, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Error(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.Errorᴍ(ref l, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void ErrorContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.ErrorContextᴍ(ref l, ctx, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void log(this ref Logger l, context.Context ctx, Level level, @string msg, params ꓸꓸꓸany argsʗp) => slog_package.logᴍ(ref l, ctx, level, msg, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logAttrs(this ref Logger l, context.Context ctx, Level level, @string msg, params ꓸꓸꓸAttr attrsʗp) => slog_package.logAttrsᴍ(ref l, ctx, level, msg, attrsʗp);
}
partial class slogᴛMultiHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Enabled(this ref MultiHandler h, context.Context ctx, Level l) => slog_package.Enabledᴍ(ref h, ctx, l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Handle(this ref MultiHandler h, context.Context ctx, Record r) => slog_package.Handleᴍ(ref h, ctx, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithAttrs(this ref MultiHandler h, slice<Attr> attrs) => slog_package.WithAttrsᴍ(ref h, attrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithGroup(this ref MultiHandler h, @string name) => slog_package.WithGroupᴍ(ref h, name);
}
partial class slogᴛRecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Record Clone(this Record r) => slog_package.Cloneᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumAttrs(this Record r) => slog_package.NumAttrsᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Attrs(this Record r, Func<Attr, bool> f) => slog_package.Attrsᴍ(r, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddAttrs(this ref Record r, params ꓸꓸꓸAttr attrsʗp) => slog_package.AddAttrsᴍ(ref r, attrsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ref Record r, params ꓸꓸꓸany argsʗp) => slog_package.Addᴍ(ref r, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Source> Source(this Record r) => slog_package.Sourceᴍ(r);
}
partial class slogᴛSource {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Value group(this ref Source s) => slog_package.groupᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmpty(this ж<Source> Ꮡs) => slog_package.isEmptyᴍ(Ꮡs);
}
partial class slogᴛTextHandler {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Enabled(this ref TextHandler h, context.Context _, Level level) => slog_package.Enabledᴍ(ref h, _, level);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithAttrs(this ref TextHandler h, slice<Attr> attrs) => slog_package.WithAttrsᴍ(ref h, attrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Handler WithGroup(this ref TextHandler h, @string name) => slog_package.WithGroupᴍ(ref h, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Handle(this ref TextHandler h, context.Context _, Record r) => slog_package.Handleᴍ(ref h, _, r);
}
partial class slogᴛKind {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Kind k) => slog_package.Stringᴍ(k);
}
partial class slogᴛValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Kind Kind(this Value v) => slog_package.Kindᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static any Any(this Value v) => slog_package.Anyᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Value v) => slog_package.Stringᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string str(this Value v) => slog_package.strᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Int64(this Value v) => slog_package.Int64ᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Uint64(this Value v) => slog_package.Uint64ᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Bool(this Value v) => slog_package.Boolᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool @bool(this Value v) => slog_package.@boolᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration Duration(this Value v) => slog_package.Durationᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration duration(this Value v) => slog_package.durationᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static float64 Float64(this Value v) => slog_package.Float64ᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static float64 @float(this Value v) => slog_package.@floatᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Time Time(this Value v) => slog_package.Timeᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time time(this Value v) => slog_package.timeᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static LogValuer LogValuer(this Value v) => slog_package.LogValuerᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<Attr> Group(this Value v) => slog_package.Groupᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Attr> group(this Value v) => slog_package.groupᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this Value v, Value w) => slog_package.Equalᴍ(v, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmptyGroup(this Value v) => slog_package.isEmptyGroupᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> append(this Value v, slice<byte> dst) => slog_package.appendᴍ(v, dst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Value /*rv*/ Resolve(this Value v) => slog_package.Resolveᴍ(v);
}
// </GlyphForwarders>
